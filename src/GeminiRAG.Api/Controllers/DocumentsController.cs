using GeminiRAG.Core.Interfaces;
using GeminiRAG.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace GeminiRAG.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IFileSearchService _fileSearchService;

    public DocumentsController(IFileSearchService fileSearchService)
    {
        _fileSearchService = fileSearchService;
    }

    [HttpGet]
    public async Task<ActionResult<List<DocumentInfo>>> GetDocuments([FromQuery] string storeName)
    {
        var documents = await _fileSearchService.ListFilesAsync(storeName);
        return Ok(documents);
    }

    [HttpPost("upload")]
    public async Task<ActionResult<string>> UploadDocument([FromQuery] string storeName, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        // Create a temp file to store the upload
        var tempPath = Path.GetTempFileName();
        var originalExt = Path.GetExtension(file.FileName);
        var newPath = Path.ChangeExtension(tempPath, originalExt);
        
        // Move temp file to have correct extension (important for mime type detection)
        System.IO.File.Move(tempPath, newPath);
        tempPath = newPath;

        try
        {
            using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var operationName = await _fileSearchService.UploadPdfAsync(storeName, tempPath);
            return Ok(new { operation = operationName });
        }
        finally
        {
            if (System.IO.File.Exists(tempPath))
                System.IO.File.Delete(tempPath);
        }
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteDocument([FromQuery] string fileName)
    {
        try
        {
            await _fileSearchService.DeleteFileAsync(fileName);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
