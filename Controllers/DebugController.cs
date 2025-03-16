#if DEBUG
using ExtensionMarket.Interfaces;
using ExtensionMarket.Models;
using Microsoft.AspNetCore.Mvc;

namespace ExtensionMarket.Controllers;

[ApiController]
[Route("[controller]")]
public class DebugController(IJwtService jwtService, IUserService userService) : ControllerBase
{
    /// <summary>
    /// 获取管理员账户的Token（仅在DEBUG模式下可用）
    /// </summary>
    /// <returns>管理员账户的Token</returns>
    [HttpGet("admin-token")]
    public async Task<IActionResult> GetAdminToken()
    {
        // 查找第一个具有Admin角色的用户
        var users = await userService.GetAllUsersAsync();
        var adminUser = users.FirstOrDefault(u => u.Roles.Contains(UserRole.Admin));

        // 如果没有找到管理员用户，创建一个新的管理员用户
        adminUser ??= await userService.CreateUserAsync(
                username: "admin",
                email: "admin@example.com",
                password: "Admin@123",
                roles: [UserRole.Admin, UserRole.Developer]
            );

        // 生成JWT令牌
        var token = jwtService.GenerateToken(adminUser);

        return Ok(new { Token = token, User = adminUser });
    }
}
#endif
