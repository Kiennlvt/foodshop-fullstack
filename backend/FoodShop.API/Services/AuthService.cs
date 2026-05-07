using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FoodShop.API.DTOs.Auth;
using FoodShop.API.Entities;
using FoodShop.API.Interfaces;
using FoodShop.API.Interfaces.Repositories;
using Microsoft.IdentityModel.Tokens;

namespace FoodShop.API.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IConfiguration _config;

    public AuthService(IUserRepository userRepo, IConfiguration config)
    {
        _userRepo = userRepo;
        _config = config;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        if (await _userRepo.EmailExistsAsync(dto.Email))
            throw new InvalidOperationException("Email is already in use.");

        var user = new User
        {
            Username     = dto.Username,
            Email        = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role         = "User"
        };

        await _userRepo.AddAsync(user);
        return BuildAuthResponse(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userRepo.GetByEmailAsync(dto.Email)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        return BuildAuthResponse(user);
    }

    // ── Private ──────────────────────────────────────────────────────────────
    private AuthResponseDto BuildAuthResponse(User user)
        => new(user.Id, user.Username, user.Email, user.Role, GenerateToken(user));

    private string GenerateToken(User user)
    {
        var jwtKey = _config["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT key is not configured.");

        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name,           user.Username),
            new Claim(ClaimTypes.Email,          user.Email),
            new Claim(ClaimTypes.Role,           user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var expires = DateTime.UtcNow.AddDays(
            int.Parse(_config["Jwt:ExpiryDays"] ?? "7"));

        var token = new JwtSecurityToken(
            issuer:   _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims:   claims,
            expires:  expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
