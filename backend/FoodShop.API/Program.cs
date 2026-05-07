using System.Text;
using FoodShop.API.Data;
using FoodShop.API.Interfaces;
using FoodShop.API.Interfaces.Repositories;
using FoodShop.API.Mappings;
using FoodShop.API.Middleware;
using FoodShop.API.Repositories;
using FoodShop.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

// ── Serilog: bootstrap before host builds ─────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json").AddEnvironmentVariables().Build())
    .Enrich.FromLogContext()
    .CreateLogger();

try
{
    Log.Information("Starting FoodShop API...");
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

    // Database
    builder.Services.AddDbContext<AppDbContext>(o =>
        o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Redis Cache
    builder.Services.AddStackExchangeRedisCache(o =>
    {
        o.Configuration = builder.Configuration.GetConnectionString("Redis");
        o.InstanceName  = "FoodShop:";
    });
    builder.Services.AddSingleton<ICacheService, RedisCacheService>();

    // JWT Auth
    var jwtKey = builder.Configuration["Jwt:Key"]!;
    builder.Services
        .AddAuthentication(o =>
        {
            o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            o.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(o =>
        {
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ValidateIssuer   = true, ValidIssuer   = builder.Configuration["Jwt:Issuer"],
                ValidateAudience = true, ValidAudience = builder.Configuration["Jwt:Audience"],
                ValidateLifetime = true, ClockSkew     = TimeSpan.Zero
            };
        });
    builder.Services.AddAuthorization();

    // CORS
    builder.Services.AddCors(o => o.AddPolicy("AllowFrontend", p =>
        p.WithOrigins("http://localhost:5173", "http://localhost:3000")
         .AllowAnyHeader().AllowAnyMethod()));

    // AutoMapper
    builder.Services.AddAutoMapper(typeof(MappingProfile));

    // Repositories
    builder.Services.AddScoped<IUserRepository,     UserRepository>();
    builder.Services.AddScoped<IProductRepository,  ProductRepository>();
    builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
    builder.Services.AddScoped<ICartRepository,     CartRepository>();
    builder.Services.AddScoped<IOrderRepository,    OrderRepository>();

    // Services (Decorator pattern: concrete -> cached wrapper)
    builder.Services.AddScoped<IAuthService,  AuthService>();
    builder.Services.AddScoped<ICartService,  CartService>();
    builder.Services.AddScoped<IOrderService, OrderService>();
    builder.Services.AddScoped<ProductService>();
    builder.Services.AddScoped<CategoryService>();
    builder.Services.AddScoped<IProductService,  CachedProductService>();
    builder.Services.AddScoped<ICategoryService, CachedCategoryService>();

    // Swagger
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "FoodShop API", Version = "v1",
            Description = "Food Shop REST API — Redis caching + Serilog logging" });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Bearer token. Example: \"Bearer {token}\"",
            Name = "Authorization", In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey, Scheme = "Bearer"
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement {{
            new OpenApiSecurityScheme { Reference = new OpenApiReference
                { Type = ReferenceType.SecurityScheme, Id = "Bearer" }},
            Array.Empty<string>()
        }});
    });
    builder.Services.AddControllers();

    var app = builder.Build();

    // Middleware pipeline
    app.UseExceptionMiddleware();  // Global error handler
    app.UseRequestLogging();       // Structured HTTP logging

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FoodShop API v1"));
    }

    app.UseHttpsRedirection();
    app.UseCors("AllowFrontend");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    // Seed
    using (var scope = app.Services.CreateScope())
        await DataSeeder.SeedAsync(scope.ServiceProvider.GetRequiredService<AppDbContext>());

    app.Run();
}
catch (Exception ex) { Log.Fatal(ex, "FoodShop API terminated unexpectedly."); }
finally { Log.CloseAndFlush(); }
