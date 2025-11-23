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
    public async Task<IActionResult> DeleteStore([FromQuery] string storeName, [FromQuery] bool force = false)
    {
        try
        {
            // If not forcing, check if store has files first
            if (!force)
            {
                var files = await _fileSearchService.ListFilesAsync(storeName);
                if (files != null && files.Count > 0)
                {
                    // Return 409 Conflict with file list
                    return Conflict(new
                    {
                        message = "Store is not empty",
                        fileCount = files.Count,
                        files = files.Select(f => new { f.Name, f.DisplayName }).ToList()
                    });
                }
            }

            await _fileSearchService.DeleteStoreAsync(storeName, force);
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
