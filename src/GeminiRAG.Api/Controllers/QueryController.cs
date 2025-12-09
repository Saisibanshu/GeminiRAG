using GeminiRAG.Core.Interfaces;
using GeminiRAG.Core.Models;
using GeminiRAG.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeminiRAG.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class QueryController : ControllerBase
{
    private readonly IGeminiQueryService _queryService;
    private readonly IQueryHistoryService _historyService;
    private readonly IUserContextService _userContext;

    public QueryController(
        IGeminiQueryService queryService, 
        IQueryHistoryService historyService,
        IUserContextService userContext)
    {
        _queryService = queryService;
        _historyService = historyService;
        _userContext = userContext;
    }

    [HttpPost]
    public async Task<ActionResult<QueryResponse>> Query([FromBody] QueryRequest request)
    {
        var userId = _userContext.GetCurrentUserId();
        var response = await _queryService.QueryAsync(request);

        // Add to history with user ID and store ID
        await _historyService.AddQueryAsync(userId, request.StoreId, new QueryHistory
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
