using GeminiRAG.Core.Interfaces;
using GeminiRAG.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeminiRAG.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ExportController : ControllerBase
{
    private readonly IQueryHistoryService _historyService;
    private readonly IExportService _exportService;
    private readonly IUserContextService _userContext;

    public ExportController(
        IQueryHistoryService historyService, 
        IExportService exportService,
        IUserContextService userContext)
    {
        _historyService = historyService;
        _exportService = exportService;
        _userContext = userContext;
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        var userId = _userContext.GetCurrentUserId();
        var history = await _historyService.GetHistoryAsync(userId);
        return Ok(history);
    }

    [HttpGet("json")]
    public async Task<IActionResult> ExportJson()
    {
        var userId = _userContext.GetCurrentUserId();
        var history = await _historyService.GetHistoryAsync(userId);
        var tempPath = Path.Combine(Path.GetTempPath(), $"query-history-{DateTime.Now:yyyyMMdd}.json");
        
        await _exportService.ExportToJsonAsync(history, tempPath);
        
        var fileBytes = await System.IO.File.ReadAllBytesAsync(tempPath);
        System.IO.File.Delete(tempPath);
        
        return File(fileBytes, "application/json", $"query-history-{DateTime.Now:yyyyMMdd}.json");
    }

    [HttpGet("markdown")]
    public async Task<IActionResult> ExportMarkdown()
    {
        var userId = _userContext.GetCurrentUserId();
        var history = await _historyService.GetHistoryAsync(userId);
        var tempPath = Path.Combine(Path.GetTempPath(), $"query-history-{DateTime.Now:yyyyMMdd}.md");
        
        await _exportService.ExportToMarkdownAsync(history, tempPath);
        
        var fileBytes = await System.IO.File.ReadAllBytesAsync(tempPath);
        System.IO.File.Delete(tempPath);
        
        return File(fileBytes, "text/markdown", $"query-history-{DateTime.Now:yyyyMMdd}.md");
    }
}
