using Microsoft.IdentityModel.Tokens;
using NoResourcesRPG.Server.Database.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NoResourcesRPG.Server;

public static class Token
{
    #region Token Key
    private static readonly string _key = "459B542197674A37BBA0621BD6BCB483";
    public static readonly byte[] KeyBytes = Encoding.UTF8.GetBytes(_key);
    #endregion
    #region Actual Token
    public static string Create(UserEntity user, IConfiguration configuration, TimeSpan sessionTime)
    {
        IEnumerable<Claim> claims = [
            new(ClaimTypes.NameIdentifier, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new Claim(ClaimTypes.GivenName, user.DisplayName ?? user.UserName ?? ""),
            new(ClaimTypes.Name, user.UserName ?? "")
        ];
        SymmetricSecurityKey key = new(KeyBytes);
        SigningCredentials creds = new(key, SecurityAlgorithms.HmacSha256Signature);
        SecurityTokenDescriptor tokenDescriptor = new()
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(sessionTime),
            Issuer = configuration["Jwt:Issuer"],
            Audience = configuration["Jwt:Audience"],
            SigningCredentials = creds
        };
        JwtSecurityTokenHandler tokenHandler = new();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
    #endregion
}
