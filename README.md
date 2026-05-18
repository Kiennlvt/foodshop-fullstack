# FoodShop — Full-Stack E-Commerce App

A production-ready food shop built with ASP.NET Core 8 + React + Tailwind CSS.

---

## Tech Stack

### Backend
| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 8 Web API |
| ORM | Entity Framework Core 8 (Code First) |
| Database | SQL Server |
| Auth | JWT Bearer Tokens |
| Caching | Redis (Decorator pattern via `ICacheService`) |
| Logging | Serilog (Console + File sinks, structured HTTP logging) |
| Mapping | AutoMapper |
| Password Hashing | BCrypt.Net |
| Docs | Swagger / OpenAPI |
| Architecture | Clean Architecture (Controller → Service → Repository) |

### Frontend
| Layer | Technology |
|---|---|
| Framework | React 18 + Vite |
| Routing | React Router v6 |
| HTTP Client | Axios (with interceptors) |
| Styling | Tailwind CSS |
| State | React Context (Auth + Cart) |
| Toasts | react-hot-toast |

---

## How to Run

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server) (or SQL Server Express / LocalDB)
- [Redis](https://redis.io/download/) (local instance on default port 6379)
- [Node.js 18+](https://nodejs.org)

---

### 1. Backend Setup

```bash
cd backend/FoodShop.API

# Restore NuGet packages
dotnet restore

# Update appsettings.json with your connection strings:
# "DefaultConnection": "Server=localhost;Database=FoodShopDb;Trusted_Connection=True;TrustServerCertificate=True;"
# "Redis": "localhost:6379"

# Install EF Core CLI tools (once)
dotnet tool install --global dotnet-ef

# Create & apply database migrations
dotnet ef migrations add InitialCreate
dotnet ef database update

# Run the API (https://localhost:62468)
dotnet run
```

Swagger UI: https://localhost:62468/swagger

> The database is auto-seeded on first run with categories, products, and two users.

**Demo Accounts:**
| Email | Password | Role |
|---|---|---|
| admin@foodshop.com | Admin@123 | Admin |
| john@example.com | User@123 | User |

---

### 2. Frontend Setup

```bash
cd frontend

# Install dependencies
npm install

# Start dev server (http://localhost:5173)
npm run dev
```

---

## Project Structure

```
foodshop/
├── backend/FoodShop.API/
│   ├── Controllers/        # Auth, ProductsCategories, CartOrders
│   ├── Data/               # AppDbContext, DataSeeder
│   ├── DTOs/               # Request/Response objects per domain
│   ├── Entities/           # User, Product, Category, Cart, Order
│   ├── Helpers/            # JWT claims extensions
│   ├── Interfaces/
│   │   ├── IServices.cs    # IAuth/Product/Category/Cart/Order/Cache service contracts
│   │   └── Repositories/   # IRepository contracts
│   ├── Mappings/           # AutoMapper profile
│   ├── Middleware/         # ExceptionMiddleware, RequestLoggingMiddleware
│   ├── Repositories/       # Data access implementations
│   ├── Services/
│   │   ├── AuthService.cs
│   │   ├── ProductCategoryService.cs
│   │   ├── CartService.cs
│   │   ├── OrderService.cs
│   │   ├── CacheService.cs            # RedisCacheService
│   │   ├── CachedProductService.cs    # Decorator wrapping ProductService
│   │   └── CachedCategoryService.cs   # Decorator wrapping CategoryService
│   ├── Program.cs          # DI, JWT, Redis, Serilog, Swagger, CORS, Seeding
│   └── appsettings.json
│
└── frontend/
    └── src/
        ├── api/            # Axios client + all API calls
        ├── components/     # Navbar, ProductCard, PrivateRoute, StatusBadge...
        ├── context/        # AuthContext, CartContext
        └── pages/          # Login, Register, ProductList, ProductDetail, Cart, Orders
```

---

## API Endpoints

| Method | Endpoint | Auth |
|---|---|---|
| POST | /api/auth/register | Public |
| POST | /api/auth/login | Public |
| GET | /api/products?page=1&pageSize=12&categoryId=1&search=apple&sortBy=price_asc | Public |
| GET | /api/products/{id} | Public |
| POST | /api/products | Admin |
| PUT | /api/products/{id} | Admin |
| DELETE | /api/products/{id} | Admin |
| GET | /api/categories | Public |
| POST | /api/categories | Admin |
| GET | /api/cart | User |
| POST | /api/cart/add | User |
| PUT | /api/cart/items/{productId} | User |
| DELETE | /api/cart/items/{productId} | User |
| POST | /api/orders | User |
| GET | /api/orders/my | User |
| GET | /api/orders | Admin |
| PATCH | /api/orders/{id}/status | Admin |

---

## Highlights for CV

- **Redis caching** — Decorator pattern (`CachedProductService`, `CachedCategoryService`) keeps controllers/services unaware of caching
- **Structured logging** — Serilog with Console + File sinks; `RequestLoggingMiddleware` logs every HTTP request with timing
- **N+1 prevention** — all queries use `.Include()` / `.ThenInclude()`
- **Indexing** — DB indexes on `Product.Name`, `Product.CategoryId`, `Order.UserId`, `Order.Status`
- **Pagination** — server-side with `PagedResult<T>` and configurable page size
- **Soft deletes** — products use `IsActive` flag, never hard-deleted
- **JWT security** — role-based `[Authorize(Roles = "Admin")]` on all sensitive endpoints
- **Global error handling** — single `ExceptionMiddleware` maps exceptions to HTTP status codes
- **AutoMapper** — zero manual DTO mapping in controllers or services
- **Clean Architecture** — strict Controller → Service → Repository layering
- **Price snapshots** — `OrderDetail.UnitPrice` captures price at time of purchase
