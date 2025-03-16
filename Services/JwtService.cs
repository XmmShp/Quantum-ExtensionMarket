using ExtensionMarket.Interfaces;
using ExtensionMarket.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ExtensionMarket.Services;

/// <summary>
/// JWT服务 - 负责生成JWT令牌
/// </summary>
public class JwtService : IJwtService
{
    /// <summary>
    /// 为用户生成JWT令牌
    /// </summary>
    /// <param name="user">用户对象</param>
    /// <returns>JWT令牌字符串</returns>
    public string GenerateToken(User user)
    {
        var authClaims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("UserId", user.Id.ToString())
        };
        authClaims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, ((int)role).ToString())));

        // 添加角色声明
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configurations.JwtKey));
        var tokenValidityInMinutes = Configurations.JwtTokenValidityInMinutes;

        var token = new JwtSecurityToken(
            issuer: Configurations.JwtIssuer,
            audience: Configurations.JwtAudience,
            expires: DateTime.Now.AddMinutes(tokenValidityInMinutes),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}