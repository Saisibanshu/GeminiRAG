using GeminiRAG.Core.Interfaces;
using GeminiRAG.Core.Models;
using GeminiRAG.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeminiRAG.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class StoresController : ControllerBase
{
    private readonly IFileSearchService _fileSearchService;
    private readonly IStoreRepository _storeRepository;
    private readonly IUserContextService _userContext;

    public StoresController(
        IFileSearchService fileSearchService,
        IStoreRepository storeRepository,
        IUserContextService userContext)
    {
        _fileSearchService = fileSearchService;
        _storeRepository = storeRepository;
        _userContext = userContext;
    }

    [HttpGet]
    public async Task<ActionResult<List<StoreInfo>>> GetStores()
    {
        var userId = _userContext.GetCurrentUserId();
        
        // Get user's stores from database
        var dbStores = await _storeRepository.GetStoresByUserIdAsync(userId);
        
        // Also get FileSearch stores for verification (these should match)
        var fileSearchStores = await _fileSearchService.ListStoresAsync();
        
        // Convert to StoreInfo
        var stores = dbStores.Select(s => new StoreInfo
        {
            Name = s.Name,
            DisplayName = s.DisplayName,
            CreateTime = s.CreatedAt
        }).ToList();

        return Ok(stores);
    }

    [HttpPost]
    public async Task<ActionResult<string>> CreateStore([FromBody] CreateStoreRequest request)
    {
        var userId = _userContext.GetCurrentUserId();

        // Check if user already has a store with this name
        var existingStore = await _storeRepository.GetStoreByNameAsync(userId, request.DisplayName);
        if (existingStore != null)
        {
            return Conflict(new { message = "Store with this name already exists" });
        }

        // Create in FileSearch
        var storeName = await _fileSearchService.CreateStoreAsync(request.DisplayName);

        // Save to database with user association
        var store = new Core.Entities.Store
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = storeName,
            DisplayName = request.DisplayName,
            CreatedAt = DateTime.UtcNow
        };

        await _storeRepository.CreateStoreAsync(store);

        return Ok(new { name = storeName, id = store.Id });
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteStore([FromQuery] string storeName, [FromQuery] bool force = false)
    {
        try
        {
            var userId = _userContext.GetCurrentUserId();

            // Find store in database and verify ownership
            var dbStore = await _storeRepository.GetStoreByNameAsync(userId, storeName);
            if (dbStore == null)
            {
                return NotFound(new { message = "Store not found or you don't have permission to delete it" });
            }

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

            // Delete from FileSearch
            await _fileSearchService.DeleteStoreAsync(storeName, force);

            // Delete from database
            await _storeRepository.DeleteStoreAsync(dbStore.Id);

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
