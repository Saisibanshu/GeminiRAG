using GeminiRAG.Core.Interfaces;
using GeminiRAG.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace GeminiRAG.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExportController : ControllerBase
{
    private readonly IQueryHistoryService _historyService;

    public ExportController(IQueryHistoryService historyService)
    {
        _historyService = historyService;
    }

    [HttpGet("history")]
    public ActionResult<List<QueryHistory>> GetHistory()
    {
        return Ok(_historyService.GetHistory());
    }
}
