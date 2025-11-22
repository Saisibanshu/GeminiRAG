using GeminiRAG.Core.Interfaces;
using GeminiRAG.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace GeminiRAG.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QueryController : ControllerBase
{
    private readonly IGeminiQueryService _queryService;
    private readonly IQueryHistoryService _historyService;

    public QueryController(IGeminiQueryService queryService, IQueryHistoryService historyService)
    {
        _queryService = queryService;
        _historyService = historyService;
    }

    [HttpPost]
    public async Task<ActionResult<QueryResponse>> Query([FromBody] QueryRequest request)
    {
        var response = await _queryService.QueryAsync(request);

        // Add to history
        _historyService.AddQuery(new QueryHistory
        {
            Timestamp = DateTime.UtcNow,
            Question = request.Question,
            Answer = response.Answer,
            Citations = response.Citations,
            ResponseTime = response.ResponseTime,
            IsFound = response.IsFound
        });

        return Ok(response);
    }
}
