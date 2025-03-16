using ExtensionMarket.Attributes;
using ExtensionMarket.Dtos;
using ExtensionMarket.Interfaces;
using ExtensionMarket.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExtensionMarket.Controllers;

/// <summary>
/// 扩展控制器 - 提供扩展管理和搜索功能
/// </summary>
[ApiController]
[Route("[controller]")]
[Authorize]
public class ExtensionsController(IExtensionService extensionService, IFileStorageService fileStorageService) : ControllerBase
{
    /// <summary>
    /// 获取所有扩展
    /// </summary>
    /// <returns>扩展列表</returns>
    /// <response code="200">成功获取扩展列表</response>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetExtensions()
    {
        var extensions = await extensionService.GetAllExtensionsAsync();
        var extensionDtos = extensions.Select(MapExtensionToDto).Where(dto => dto.LatestVersion is not null).ToList();
        return Ok(extensionDtos);
    }

    /// <summary>
    /// 根据ID获取扩展详情
    /// </summary>
    /// <param name="id">扩展ID</param>
    /// <returns>扩展详情</returns>
    /// <response code="200">成功获取扩展详情</response>
    /// <response code="404">未找到指定ID的扩展</response>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetExtension(Guid id)
    {
        var extension = await extensionService.GetExtensionByIdAsync(id);
        if (extension == null)
        {
            return NotFound();
        }

        return Ok(MapExtensionToDto(extension));
    }

    /// <summary>
    /// 获取指定作者的所有扩展
    /// </summary>
    /// <param name="authorId">作者ID</param>
    /// <returns>扩展列表</returns>
    /// <response code="200">成功获取作者的扩展列表</response>
    [HttpGet("author/{authorId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetExtensionsByAuthor(Guid authorId)
    {
        var extensions = await extensionService.GetExtensionsByAuthorIdAsync(authorId);
        var extensionDtos = extensions.Select(MapExtensionToDto).ToList();
        return Ok(extensionDtos);
    }

    /// <summary>
    /// 搜索扩展
    /// </summary>
    /// <param name="term">搜索关键词</param>
    /// <param name="tags">标签列表，以逗号分隔</param>
    /// <returns>符合条件的扩展列表</returns>
    /// <response code="200">成功获取搜索结果</response>
    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<IActionResult> SearchExtensions([FromQuery] string term, [FromQuery] string tags)
    {
        List<string>? tagList = null;
        if (!string.IsNullOrEmpty(tags))
        {
            tagList = [.. tags.Split(',')];
        }

        var extensions = await extensionService.SearchExtensionsAsync(term, tagList);
        var extensionDtos = extensions.Select(MapExtensionToDto).ToList();
        return Ok(extensionDtos);
    }

    /// <summary>
    /// 创建扩展
    /// </summary>
    /// <param name="model">创建扩展DTO</param>
    /// <returns>创建的扩展</returns>
    /// <response code="201">成功创建扩展</response>
    /// <response code="400">扩展文件不能为空</response>
    [HttpPost]
    [AuthorizeRoles(UserRole.Developer, UserRole.Admin)]
    public async Task<IActionResult> CreateExtension(CreateExtensionDto model)
    {
        var currentUserId = new Guid(User.FindFirst("UserId")?.Value!);

        try
        {
            var createdExtension = await extensionService.CreateExtensionAsync(
                model.Name,
                model.Description,
                currentUserId,
                model.Tags);

            return CreatedAtAction(
                nameof(GetExtension),
                new { id = createdExtension.Id },
                MapExtensionToDto(createdExtension));
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// 更新扩展
    /// </summary>
    /// <param name="id">扩展ID</param>
    /// <param name="model">扩展更新DTO</param>
    /// <returns>更新后的扩展</returns>
    /// <response code="200">成功更新扩展</response>
    /// <response code="400">扩展ID不匹配</response>
    /// <response code="404">未找到指定ID的扩展</response>
    [HttpPut("{id:guid}")]
    [AuthorizeRoles(UserRole.Developer, UserRole.Admin)]
    public async Task<IActionResult> UpdateExtension(Guid id, [FromBody] UpdateExtensionDto model)
    {
        try
        {
            // Get the extension to check ownership
            var extension = await extensionService.GetExtensionByIdAsync(id);
            if (extension == null)
            {
                return NotFound();
            }

            // Verify the current user is the author or an admin
            var currentUserId = User.FindFirst("UserId")?.Value;
            if (currentUserId != extension.AuthorId.ToString() && !User.IsInRole(((int)UserRole.Admin).ToString()))
            {
                return Forbid();
            }

            // Call the service with the individual properties from the DTO
            var updatedExtension = await extensionService.UpdateExtensionAsync(
                id,
                model.Name,
                model.Description,
                model.Tags);

            return Ok(MapExtensionToDto(updatedExtension));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// 删除扩展
    /// </summary>
    /// <param name="id">扩展ID</param>
    /// <returns>删除成功</returns>
    /// <response code="204">删除成功</response>
    /// <response code="404">未找到指定ID的扩展</response>
    [HttpDelete("{id:guid}")]
    [AuthorizeRoles(UserRole.Developer, UserRole.Admin)]
    public async Task<IActionResult> DeleteExtension(Guid id)
    {
        try
        {
            // Get the extension to check ownership
            var extension = await extensionService.GetExtensionByIdAsync(id);
            if (extension == null)
            {
                return NotFound();
            }

            // Verify the current user is the author or an admin
            var currentUserId = User.FindFirst("UserId")?.Value;
            if (currentUserId != extension.AuthorId.ToString() && !User.IsInRole(((int)UserRole.Admin).ToString()))
            {
                return Forbid();
            }

            var result = await extensionService.DeleteExtensionAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// 添加扩展版本
    /// </summary>
    /// <param name="extensionId">扩展ID</param>
    /// <param name="model">创建扩展版本DTO</param>
    /// <returns>添加的版本</returns>
    /// <response code="200">成功添加版本</response>
    /// <response code="400">扩展文件不能为空</response>
    /// <response code="404">未找到指定ID的扩展</response>
    [HttpPost("{extensionId:guid}/versions")]
    [AuthorizeRoles(UserRole.Developer, UserRole.Admin)]
    public async Task<IActionResult> AddExtensionVersion(Guid extensionId, [FromForm] CreateExtensionVersionDto model)
    {
        try
        {
            // Get the extension to check ownership
            var extension = await extensionService.GetExtensionByIdAsync(extensionId);
            if (extension == null)
            {
                return NotFound();
            }

            // Verify the current user is the author or an admin
            var currentUserId = User.FindFirst("UserId")?.Value;
            if (currentUserId != extension.AuthorId.ToString() && !User.IsInRole(((int)UserRole.Admin).ToString()))
            {
                return Forbid();
            }

            var createdVersion = await extensionService.AddExtensionVersionAsync(
                extensionId,
                model.VersionNumber,
                model.QuantumVersionSupport,
                model.ReleaseNotes,
                model.ExtensionFile);

            return Ok(MapVersionToDto(createdVersion));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// 获取扩展版本列表
    /// </summary>
    /// <param name="extensionId">扩展ID</param>
    /// <returns>版本列表</returns>
    /// <response code="200">成功获取版本列表</response>
    [HttpGet("{extensionId:guid}/versions")]
    [AllowAnonymous]
    public async Task<IActionResult> GetExtensionVersions(Guid extensionId)
    {
        var versions = await extensionService.GetExtensionVersionsAsync(extensionId);
        var versionDtos = versions.Select(MapVersionToDto).ToList();
        return Ok(versionDtos);
    }

    /// <summary>
    /// 下载扩展版本
    /// </summary>
    /// <param name="extensionId">扩展ID</param>
    /// <param name="versionNumber">版本号</param>
    /// <returns>下载的文件</returns>
    /// <response code="200">成功下载文件</response>
    /// <response code="404">未找到指定ID的扩展或版本</response>
    [HttpGet("{extensionId:guid}/versions/{versionNumber}/download")]
    [AllowAnonymous]
    public async Task<IActionResult> DownloadExtension(Guid extensionId, string versionNumber)
    {
        try
        {
            // Find the version
            var versions = await extensionService.GetExtensionVersionsAsync(extensionId);
            var version = versions.FirstOrDefault(v => v.VersionNumber == versionNumber && v.Status == ExtensionVersionStatus.Published);

            if (version == null)
            {
                return NotFound();
            }

            // Get the file
            var (fileContent, contentType, fileName) = await fileStorageService.GetExtensionFileAsync(extensionId, versionNumber);

            // Increment download count
            await extensionService.IncrementDownloadCountAsync(version.Id);

            // Return file
            return File(fileContent, contentType, fileName);
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// 检查扩展兼容性
    /// </summary>
    /// <param name="extensionId">扩展ID</param>
    /// <param name="version">Quantum版本</param>
    /// <returns>兼容性结果</returns>
    /// <response code="200">成功获取兼容性结果</response>
    /// <response code="400">Quantum版本不能为空</response>
    [HttpGet("compatibility/{extensionId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckCompatibility(Guid extensionId, [FromQuery] string version)
    {
        if (string.IsNullOrEmpty(version))
        {
            return BadRequest("Quantum version is required.");
        }

        var isCompatible = await extensionService.IsCompatibleWithQuantumVersionAsync(extensionId, version);
        return Ok(isCompatible);
    }

    /// <summary>
    /// 更新扩展版本状态（仅限管理员）
    /// </summary>
    /// <param name="versionId">版本ID</param>
    /// <param name="model">状态更新DTO</param>
    /// <returns>更新后的版本</returns>
    /// <response code="200">成功更新状态</response>
    /// <response code="404">未找到指定ID的版本</response>
    [HttpPut("versions/{versionId:guid}/status")]
    [AuthorizeRoles(UserRole.Admin)]
    public async Task<IActionResult> UpdateVersionStatus(Guid versionId, [FromBody] UpdateVersionStatusDto model)
    {
        try
        {
            // Get the version
            var version = await extensionService.GetExtensionVersionByIdAsync(versionId);
            if (version == null)
            {
                return NotFound();
            }

            // Update the status
            var updatedVersion = await extensionService.UpdateExtensionVersionStatusAsync(versionId, model.Status);
            if (updatedVersion == null)
            {
                return NotFound();
            }

            return Ok(MapVersionToDto(updatedVersion));
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// 获取所有扩展版本（仅限管理员）
    /// </summary>
    /// <returns>所有版本列表</returns>
    /// <response code="200">成功获取版本列表</response>
    [HttpGet("versions/all")]
    [AuthorizeRoles(UserRole.Admin)]
    public async Task<IActionResult> GetAllExtensionVersions()
    {
        try
        {
            // Get all extensions
            var extensions = await extensionService.GetAllExtensionsAsync();

            // Collect all versions
            var allVersions = new List<ExtensionVersionDto>();
            foreach (var extension in extensions)
            {
                var versions = await extensionService.GetExtensionVersionsAsync(extension.Id);
                allVersions.AddRange(versions.Select(v =>
                {
                    var dto = MapVersionToDto(v);
                    // Add extension name to make it easier to identify
                    dto.ExtensionName = extension.Name;
                    return dto;
                }));
            }

            return Ok(allVersions);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// 将Extension实体映射为ExtensionDto
    /// </summary>
    /// <param name="extension">扩展实体</param>
    /// <returns>扩展DTO</returns>
    private static ExtensionDto MapExtensionToDto(Extension extension)
    {
        var latestVersion = extension.Versions.OrderByDescending(v => v.UploadedDate)
            .FirstOrDefault(ver => ver.Status is ExtensionVersionStatus.Published);

        return new ExtensionDto
        {
            Id = extension.Id,
            Name = extension.Name,
            Description = extension.Description,
            AuthorId = extension.AuthorId,
            AuthorName = extension.Author?.Username ?? "Unknown",
            Tags = extension.Tags,
            CreatedDate = extension.CreatedDate,
            LastUpdated = extension.LastUpdated,
            LatestVersion = latestVersion != null ? MapVersionToDto(latestVersion) : null
        };
    }

    /// <summary>
    /// 将ExtensionVersion实体映射为ExtensionVersionDto
    /// </summary>
    /// <param name="version">扩展版本实体</param>
    /// <returns>扩展版本DTO</returns>
    private static ExtensionVersionDto MapVersionToDto(ExtensionVersion version) =>
        new()
        {
            Id = version.Id,
            ExtensionId = version.ExtensionId,
            VersionNumber = version.VersionNumber,
            QuantumVersionSupport = version.QuantumVersionSupport,
            ReleaseNotes = version.ReleaseNotes,
            Status = version.Status.ToString(),
            DownloadCount = version.DownloadCount,
            DownloadUrl = version.DownloadUrl,
            UploadedDate = version.UploadedDate
        };
}