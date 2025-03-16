using ExtensionMarket.Models;
using System.ComponentModel.DataAnnotations;

namespace ExtensionMarket.Dtos;

/// <summary>
/// 扩展创建请求DTO
/// </summary>
public class CreateExtensionDto
{
    /// <summary>
    /// 扩展名称
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 扩展描述
    /// </summary>
    [Required]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 标签列表
    /// </summary>
    public List<string> Tags { get; set; } = [];
}

/// <summary>
/// 扩展更新请求DTO
/// </summary>
public class UpdateExtensionDto
{
    /// <summary>
    /// 扩展名称
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 扩展描述
    /// </summary>
    [Required]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 标签列表
    /// </summary>
    public List<string> Tags { get; set; } = [];
}

/// <summary>
/// 扩展版本创建请求DTO
/// </summary>
public class CreateExtensionVersionDto
{
    /// <summary>
    /// 版本号
    /// </summary>
    [Required]
    public string VersionNumber { get; set; } = string.Empty;

    /// <summary>
    /// 支持的Quantum版本
    /// </summary>
    [Required]
    public string QuantumVersionSupport { get; set; } = string.Empty;

    /// <summary>
    /// 发布说明
    /// </summary>
    public string ReleaseNotes { get; set; } = string.Empty;

    [Required]
    public IFormFile ExtensionFile { get; set; } = null!;
}

/// <summary>
/// 扩展响应DTO
/// </summary>
public class ExtensionDto
{
    /// <summary>
    /// 扩展ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 扩展名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 扩展描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 作者ID
    /// </summary>
    public Guid AuthorId { get; set; }

    /// <summary>
    /// 作者用户名
    /// </summary>
    public string? AuthorName { get; set; }

    /// <summary>
    /// 标签列表
    /// </summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime LastUpdated { get; set; }

    /// <summary>
    /// 最新版本
    /// </summary>
    public ExtensionVersionDto? LatestVersion { get; set; }
}

/// <summary>
/// 扩展版本响应DTO
/// </summary>
public class ExtensionVersionDto
{
    /// <summary>
    /// 版本ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 扩展ID
    /// </summary>
    public Guid ExtensionId { get; set; }

    /// <summary>
    /// 扩展名称
    /// </summary>
    public string? ExtensionName { get; set; }

    /// <summary>
    /// 版本号
    /// </summary>
    public string VersionNumber { get; set; } = string.Empty;

    /// <summary>
    /// 支持的Quantum版本
    /// </summary>
    public string QuantumVersionSupport { get; set; } = string.Empty;

    /// <summary>
    /// 发布说明
    /// </summary>
    public string ReleaseNotes { get; set; } = string.Empty;

    /// <summary>
    /// 下载URL
    /// </summary>
    public string DownloadUrl { get; set; } = string.Empty;

    /// <summary>
    /// 状态
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// 上传时间
    /// </summary>
    public DateTime UploadedDate { get; set; }

    /// <summary>
    /// 下载次数
    /// </summary>
    public int DownloadCount { get; set; }
}

/// <summary>
/// 更新扩展版本状态DTO
/// </summary>
public class UpdateVersionStatusDto
{
    /// <summary>
    /// 版本状态
    /// </summary>
    [Required]
    public ExtensionVersionStatus Status { get; set; }
}
