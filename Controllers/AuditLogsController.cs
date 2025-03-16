using ExtensionMarket.Attributes;
using ExtensionMarket.Dtos;
using ExtensionMarket.Interfaces;
using ExtensionMarket.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExtensionMarket.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
[AuthorizeRoles(UserRole.Admin)]
public class AuditLogsController(IAuditLogService auditLogService) : ControllerBase
{
    // GET: /auditlogs
    [HttpGet]
    public async Task<IActionResult> GetAuditLogs()
    {
        var auditLogs = await auditLogService.GetAllAuditLogsAsync();
        var auditLogDtos = auditLogs.Select(MapAuditLogToDto).ToList();
        return Ok(auditLogDtos);
    }

    // GET: /auditlogs/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAuditLog(Guid id)
    {
        var auditLog = await auditLogService.GetAuditLogByIdAsync(id);
        if (auditLog == null)
        {
            return NotFound();
        }

        return Ok(MapAuditLogToDto(auditLog));
    }

    // GET: /auditlogs/user/{userId}
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetAuditLogsByUser(Guid userId)
    {
        var auditLogs = await auditLogService.GetAuditLogsByUserIdAsync(userId);
        var auditLogDtos = auditLogs.Select(MapAuditLogToDto).ToList();
        return Ok(auditLogDtos);
    }

    // GET: /auditlogs/extension/{extensionId}
    [HttpGet("extension/{extensionId}")]
    public async Task<IActionResult> GetAuditLogsByExtension(Guid extensionId)
    {
        var auditLogs = await auditLogService.GetAuditLogsByExtensionIdAsync(extensionId);
        var auditLogDtos = auditLogs.Select(MapAuditLogToDto).ToList();
        return Ok(auditLogDtos);
    }

    // GET: /auditlogs/daterange?startDate=2023-01-01&endDate=2023-12-31
    [HttpGet("daterange")]
    public async Task<IActionResult> GetAuditLogsByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        if (startDate > endDate)
        {
            return BadRequest("Start date must be before end date.");
        }

        var auditLogs = await auditLogService.GetAuditLogsByDateRangeAsync(startDate, endDate);
        var auditLogDtos = auditLogs.Select(MapAuditLogToDto).ToList();
        return Ok(auditLogDtos);
    }

    /// <summary>
    /// 将AuditLog实体映射为AuditLogDto
    /// </summary>
    /// <param name="auditLog">审计日志实体</param>
    /// <returns>审计日志DTO</returns>
    private static AuditLogDto MapAuditLogToDto(AuditLog auditLog) =>
        new()
        {
            Id = auditLog.Id,
            Action = auditLog.Action,
            UserId = auditLog.UserId,
            Timestamp = auditLog.Timestamp,
            Details = auditLog.Details,
            ExtensionId = auditLog.ExtensionId,
            ExtensionVersionId = auditLog.ExtensionVersionId
        };
}