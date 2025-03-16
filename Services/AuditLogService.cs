using ExtensionMarket.Data;
using ExtensionMarket.Interfaces;
using ExtensionMarket.Models;
using Microsoft.EntityFrameworkCore;

namespace ExtensionMarket.Services;

public class AuditLogService(ApplicationDbContext context) : IAuditLogService
{
    public async Task<IEnumerable<AuditLog>> GetAllAuditLogsAsync()
    {
        return await context.AuditLogs
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetAuditLogsByUserIdAsync(Guid userId)
    {
        return await context.AuditLogs
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetAuditLogsByExtensionIdAsync(Guid extensionId)
    {
        return await context.AuditLogs
            .Where(a => a.ExtensionId == extensionId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    public async Task<AuditLog> CreateAuditLogAsync(string action, Guid userId, string details, Guid? extensionId = null, Guid? extensionVersionId = null)
    {
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            Action = action,
            UserId = userId,
            Details = details,
            Timestamp = DateTime.UtcNow,
            ExtensionId = extensionId,
            ExtensionVersionId = extensionVersionId
        };

        await context.AuditLogs.AddAsync(auditLog);
        await context.SaveChangesAsync();

        return auditLog;
    }

    public async Task<AuditLog?> GetAuditLogByIdAsync(Guid id)
    {
        return await context.AuditLogs
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<AuditLog>> GetAuditLogsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await context.AuditLogs
            .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }
}