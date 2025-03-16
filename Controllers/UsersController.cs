using ExtensionMarket.Attributes;
using ExtensionMarket.Dtos;
using ExtensionMarket.Interfaces;
using ExtensionMarket.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExtensionMarket.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController(IUserService userService, IJwtService jwtService) : ControllerBase
{
    // GET: /users
    [HttpGet]
    [AuthorizeRoles(UserRole.Admin)]
    public async Task<IActionResult> GetUsers()
    {
        var users = await userService.GetAllUsersAsync();
        var userDtos = users.Select(MapUserToDto).ToList();
        return Ok(userDtos);
    }

    // GET: /users/{id}
    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var user = await userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        // Check if the user is requesting their own profile or is an admin
        var currentUserId = User.FindFirst("UserId")?.Value;
        if (currentUserId != id.ToString() && !User.IsInRole(((int)UserRole.Admin).ToString()))
        {
            return Forbid();
        }

        return Ok(MapUserToDto(user));
    }

    // POST: /users
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateUser([FromBody] UserRegistrationDto model)
    {
        try
        {
            var createdUser = await userService.CreateUserAsync(
                model.Username,
                model.Email,
                model.Password,
                [UserRole.Developer] // Default role for new users
            );

            return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, MapUserToDto(createdUser));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PUT: /users/{id}
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UserUpdateDto model)
    {
        var existingUser = await userService.GetUserByIdAsync(id);
        if (existingUser == null)
        {
            return NotFound();
        }

        // Check if the user is updating their own profile or is an admin
        var currentUserId = User.FindFirst("UserId")?.Value;
        if (currentUserId != id.ToString() && !User.IsInRole(((int)UserRole.Admin).ToString()))
        {
            return Forbid();
        }

        try
        {
            var updatedUser = await userService.UpdateUserAsync(
                id,
                model.Username,
                model.Email
            );

            return Ok(MapUserToDto(updatedUser));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // DELETE: /users/{id}
    [HttpDelete("{id:guid}")]
    [AuthorizeRoles(UserRole.Admin)]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var result = await userService.DeleteUserAsync(id);
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    // POST: /users/{id}/roles
    [HttpPost("{id:guid}/roles")]
    [AuthorizeRoles(UserRole.Admin)]
    public async Task<IActionResult> AddUserRole(Guid id, [FromBody] UserRoleDto model)
    {
        var result = await userService.AddUserRoleAsync(id, model.Role);
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    // DELETE: /users/{id}/roles
    [HttpDelete("{id:guid}/roles")]
    [AuthorizeRoles(UserRole.Admin)]
    public async Task<IActionResult> RemoveUserRole(Guid id, [FromBody] UserRoleDto model)
    {
        var result = await userService.RemoveUserRoleAsync(id, model.Role);
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    // POST: /users/login
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] UserLoginDto model)
    {
        // 验证用户凭据
        var isValid = await userService.ValidateUserCredentialsAsync(model.Email, model.Password);
        if (!isValid)
        {
            return Unauthorized("Invalid email or password.");
        }

        // 获取用户信息
        var user = await userService.GetUserByEmailAsync(model.Email);
        if (user == null)
        {
            return Unauthorized("User not found.");
        }

        // 生成JWT令牌
        var token = jwtService.GenerateToken(user);


        // 创建响应
        var response = MapUserToDto(user);
        return Ok(new { User = response, Token = token });
    }

    /// <summary>
    /// 将User实体映射为UserDto
    /// </summary>
    /// <param name="user">用户实体</param>
    /// <returns>用户DTO</returns>
    private static UserDto MapUserToDto(User user) =>
        new()
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Roles = user.Roles,
            CreatedDate = user.CreatedDate,
            LastLogin = user.LastLogin
        };
}