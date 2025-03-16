using ExtensionMarket.Models;

namespace ExtensionMarket.Interfaces;

public interface IAuditLogService
{
    Task<IEnumerable<AuditLog>> GetAllAuditLogsAsync();
    Task<IEnumerable<AuditLog>> GetAuditLogsByUserIdAsync(Guid userId);
    Task<IEnumerable<AuditLog>> GetAuditLogsByExtensionIdAsync(Guid extensionId);
    Task<AuditLog> CreateAuditLogAsync(string action, Guid userId, string details, Guid? extensionId = null, Guid? extensionVersionId = null);
    Task<AuditLog?> GetAuditLogByIdAsync(Guid id);
    Task<IEnumerable<AuditLog>> GetAuditLogsByDateRangeAsync(DateTime startDate, DateTime endDate);
}