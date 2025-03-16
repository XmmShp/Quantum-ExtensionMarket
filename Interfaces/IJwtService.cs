using ExtensionMarket.Models;

namespace ExtensionMarket.Interfaces;

public interface IJwtService
{
    /// <summary>
    /// 为用户生成JWT令牌
    /// </summary>
    /// <param name="user">用户对象</param>
    /// <returns>JWT令牌字符串</returns>
    string GenerateToken(User user);
}