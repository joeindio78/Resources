using Microsoft.Extensions.Options;
using Resources.API.Models;
using Resources.API.Services;
using Resources.API.Settings;
using Xunit;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Resources.API.Tests.Services;

public class AuthServiceTests
{
    private readonly AuthService _authService;
    private readonly JwtSettings _jwtSettings;
    private readonly AdminUserSettings _adminSettings;

    public AuthServiceTests()
    {
        _jwtSettings = new JwtSettings { Key = "your-super-secret-key-with-at-least-32-characters" };
        _adminSettings = new AdminUserSettings { Email = "admin@admin.io", Password = "admin" };

        var jwtOptions = Options.Create(_jwtSettings);
        var adminOptions = Options.Create(_adminSettings);

        _authService = new AuthService(jwtOptions, adminOptions);
    }

    [Fact]
    public void Login_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var request = new LoginRequest(_adminSettings.Email, _adminSettings.Password);

        // Act
        var result = _authService.Login(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Token);
        Assert.NotEmpty(result.Token);
    }

    [Theory]
    [InlineData("wrong@email.com", "admin")]
    [InlineData("admin@admin.io", "wrong")]
    [InlineData("wrong@email.com", "wrong")]
    public void Login_WithInvalidCredentials_ReturnsNull(string email, string password)
    {
        // Arrange
        var request = new LoginRequest(email, password);

        // Act
        var result = _authService.Login(request);

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData("", "admin")]
    [InlineData("admin@admin.io", "")]
    [InlineData("", "")]
    [InlineData(null, "admin")]
    [InlineData("admin@admin.io", null)]
    [InlineData(null, null)]
    public void Login_WithEmptyOrNullCredentials_ReturnsNull(string email, string password)
    {
        // Arrange
        var request = new LoginRequest(email, password);

        // Act
        var result = _authService.Login(request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Login_WithValidCredentials_ReturnsValidJwtToken()
    {
        // Arrange
        var request = new LoginRequest(_adminSettings.Email, _adminSettings.Password);

        // Act
        var result = _authService.Login(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Token);
        
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.Token);

        // Verify token claims
        Assert.Contains(token.Claims, c => c.Type == "email" && c.Value == _adminSettings.Email);
        Assert.Contains(token.Claims, c => c.Type == "role" && c.Value == "Admin");
        
        // Verify token expiration
        Assert.True(token.ValidTo > DateTime.UtcNow);
        Assert.True(token.ValidTo <= DateTime.UtcNow.AddDays(8)); // Should be around 7 days
    }

    [Theory]
    [InlineData("ADMIN@ADMIN.IO", "admin")] // Upper case email
    [InlineData("admin@admin.io", "ADMIN")] // Upper case password
    [InlineData("ADMIN@ADMIN.IO", "ADMIN")] // Both upper case
    [InlineData(" admin@admin.io ", "admin")] // Email with whitespace
    [InlineData("admin@admin.io", " admin ")] // Password with whitespace
    public void Login_WithCredentialVariations_ReturnsNull(string email, string password)
    {
        // Arrange
        var request = new LoginRequest(email, password);

        // Act
        var result = _authService.Login(request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Login_WithValidCredentials_TokenHasValidSignature()
    {
        // Arrange
        var request = new LoginRequest(_adminSettings.Email, _adminSettings.Password);

        // Act
        var result = _authService.Login(request);

        // Assert
        Assert.NotNull(result);
        
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.Key);
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        // This will throw if the token is invalid
        var principal = tokenHandler.ValidateToken(result.Token, validationParameters, out var validatedToken);
        
        Assert.NotNull(principal);
        Assert.NotNull(validatedToken);
        Assert.True(validatedToken is JwtSecurityToken);
        var jwtToken = (JwtSecurityToken)validatedToken;
        Assert.Equal("HS256", jwtToken.SignatureAlgorithm);
    }
} 