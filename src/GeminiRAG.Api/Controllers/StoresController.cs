using GeminiRAG.Core.Interfaces;
using GeminiRAG.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace GeminiRAG.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoresController : ControllerBase
{
    private readonly IFileSearchService _fileSearchService;

    public StoresController(IFileSearchService fileSearchService)
    {
        _fileSearchService = fileSearchService;
    }

    [HttpGet]
    public async Task<ActionResult<List<StoreInfo>>> GetStores()
    {
        var stores = await _fileSearchService.ListStoresAsync();
        return Ok(stores);
    }

    [HttpPost]
    public async Task<ActionResult<string>> CreateStore([FromBody] CreateStoreRequest request)
    {
        var storeName = await _fileSearchService.CreateStoreAsync(request.DisplayName);
        return Ok(new { name = storeName });
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteStore([FromQuery] string storeName)
    {
        try
        {
            await _fileSearchService.DeleteStoreAsync(storeName);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class CreateStoreRequest
{
    public required string DisplayName { get; set; }
}
