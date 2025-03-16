namespace ExtensionMarket.Dtos;

/// <summary>
/// 审计日志响应DTO
/// </summary>
public class AuditLogDto
{
    /// <summary>
    /// 日志ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 操作
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// 详情
    /// </summary>
    public string Details { get; set; } = string.Empty;

    /// <summary>
    /// 扩展ID
    /// </summary>
    public Guid? ExtensionId { get; set; }

    /// <summary>
    /// 扩展版本ID
    /// </summary>
    public Guid? ExtensionVersionId { get; set; }
}