using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Resources.API.Models;
using Resources.API.Settings;

namespace Resources.API.Services;

public interface IAuthService
{
    LoginResponse? Login(LoginRequest request);
}

public class AuthService : IAuthService
{
    private readonly JwtSettings _jwtSettings;
    private readonly AdminUserSettings _adminSettings;

    public AuthService(IOptions<JwtSettings> jwtSettings, IOptions<AdminUserSettings> adminSettings)
    {
        _jwtSettings = jwtSettings.Value;
        _adminSettings = adminSettings.Value;
    }

    public LoginResponse? Login(LoginRequest request)
    {
        if (request.Email != _adminSettings.Email || request.Password != _adminSettings.Password)
        {
            return null;
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.Key);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Email, request.Email),
                new Claim(ClaimTypes.Role, "Admin")
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return new LoginResponse(tokenHandler.WriteToken(token));
    }
} 