using ExtensionMarket.Models;
using System.ComponentModel.DataAnnotations;

namespace ExtensionMarket.Dtos;

/// <summary>
/// 用户注册请求DTO
/// </summary>
public class UserRegistrationDto
{
    /// <summary>
    /// 用户名
    /// </summary>
    [Required(ErrorMessage = "用户名是必填项")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 电子邮箱
    /// </summary>
    [Required(ErrorMessage = "电子邮箱是必填项")]
    [EmailAddress(ErrorMessage = "无效的电子邮箱格式")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 密码
    /// </summary>
    [Required(ErrorMessage = "密码是必填项")]
    [MinLength(6, ErrorMessage = "密码长度至少为6个字符")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// 用户更新请求DTO
/// </summary>
public class UserUpdateDto
{
    /// <summary>
    /// 用户名
    /// </summary>
    [Required]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 电子邮箱
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// 用户登录请求DTO
/// </summary>
public class UserLoginDto
{
    /// <summary>
    /// 电子邮箱
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 密码
    /// </summary>
    [Required]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// 用户角色请求DTO
/// </summary>
public class UserRoleDto
{
    /// <summary>
    /// 角色
    /// </summary>
    [Required]
    public UserRole Role { get; set; }
}

/// <summary>
/// 用户响应DTO
/// </summary>
public class UserDto
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 电子邮箱
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 角色列表
    /// </summary>
    public List<UserRole> Roles { get; set; } = [];

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// 最后登录时间
    /// </summary>
    public DateTime LastLogin { get; set; }
}
