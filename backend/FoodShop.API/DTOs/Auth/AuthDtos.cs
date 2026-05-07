namespace FoodShop.API.DTOs.Auth;

public record RegisterDto(
    string Username,
    string Email,
    string Password
);

public record LoginDto(
    string Email,
    string Password
);

public record AuthResponseDto(
    int Id,
    string Username,
    string Email,
    string Role,
    string Token
);
