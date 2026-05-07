using FluentAssertions;
using FoodShop.API.DTOs.Auth;
using FoodShop.API.Entities;
using FoodShop.API.Interfaces.Repositories;
using FoodShop.API.Services;
using Moq;

namespace FoodShop.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly AuthService _sut; // System Under Test

    public AuthServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _sut = new AuthService(_userRepoMock.Object, TestHelpers.CreateJwtConfig());
    }

    // ── Register ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_ValidData_ReturnsAuthResponse()
    {
        // Arrange
        var dto = new RegisterDto("kien", "kien@test.com", "Password123");

        _userRepoMock.Setup(r => r.EmailExistsAsync(dto.Email))
                     .ReturnsAsync(false);

        _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>()))
                     .ReturnsAsync((User u) => { u.Id = 1; return u; });

        // Act
        var result = await _sut.RegisterAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be("kien");
        result.Email.Should().Be("kien@test.com");
        result.Role.Should().Be("User");
        result.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_DuplicateEmail_ThrowsException()
    {
        // Arrange
        var dto = new RegisterDto("kien", "kien@test.com", "Password123");

        _userRepoMock.Setup(r => r.EmailExistsAsync(dto.Email))
                     .ReturnsAsync(true); // Email đã tồn tại

        // Act
        var act = async () => await _sut.RegisterAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("*already in use*");
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_ValidCredentials_ReturnsTokenWithCorrectRole()
    {
        // Arrange
        var dto = new LoginDto("admin@test.com", "Admin@123");
        var user = new User
        {
            Id           = 1,
            Username     = "admin",
            Email        = "admin@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role         = "Admin"
        };

        _userRepoMock.Setup(r => r.GetByEmailAsync(dto.Email))
                     .ReturnsAsync(user);

        // Act
        var result = await _sut.LoginAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Role.Should().Be("Admin");
        result.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WrongPassword_ThrowsUnauthorizedException()
    {
        // Arrange
        var dto = new LoginDto("kien@test.com", "WrongPassword");
        var user = new User
        {
            Email        = "kien@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword")
        };

        _userRepoMock.Setup(r => r.GetByEmailAsync(dto.Email))
                     .ReturnsAsync(user);

        // Act
        var act = async () => await _sut.LoginAsync(dto);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
                 .WithMessage("*Invalid credentials*");
    }

    [Fact]
    public async Task Login_EmailNotFound_ThrowsUnauthorizedException()
    {
        // Arrange
        var dto = new LoginDto("notfound@test.com", "Password123");

        _userRepoMock.Setup(r => r.GetByEmailAsync(dto.Email))
                     .ReturnsAsync((User?)null); // Không tìm thấy

        // Act
        var act = async () => await _sut.LoginAsync(dto);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
