using GeminiRAG.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GeminiRAG.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExportController : ControllerBase
{
    private readonly IQueryHistoryService _historyService;
    private readonly IExportService _exportService;

    public ExportController(IQueryHistoryService historyService, IExportService exportService)
    {
        _historyService = historyService;
        _exportService = exportService;
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        var history = await _historyService.GetHistoryAsync();
        return Ok(history);
    }

    [HttpGet("json")]
    public async Task<IActionResult> ExportJson()
    {
        var history = await _historyService.GetHistoryAsync();
        var tempPath = Path.Combine(Path.GetTempPath(), $"query-history-{DateTime.Now:yyyyMMdd}.json");
        
        await _exportService.ExportToJsonAsync(history, tempPath);
        
        var fileBytes = await System.IO.File.ReadAllBytesAsync(tempPath);
        System.IO.File.Delete(tempPath);
        
        return File(fileBytes, "application/json", $"query-history-{DateTime.Now:yyyyMMdd}.json");
    }

    [HttpGet("markdown")]
    public async Task<IActionResult> ExportMarkdown()
    {
        var history = await _historyService.GetHistoryAsync();
        var tempPath = Path.Combine(Path.GetTempPath(), $"query-history-{DateTime.Now:yyyyMMdd}.md");
        
        await _exportService.ExportToMarkdownAsync(history, tempPath);
        
        var fileBytes = await System.IO.File.ReadAllBytesAsync(tempPath);
        System.IO.File.Delete(tempPath);
        
        return File(fileBytes, "text/markdown", $"query-history-{DateTime.Now:yyyyMMdd}.md");
    }
}
