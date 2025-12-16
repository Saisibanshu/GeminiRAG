using GeminiRAG.Core.Interfaces;
using GeminiRAG.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeminiRAG.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IFileSearchService _fileSearchService;
    private readonly IFileValidationService _fileValidation;

    public DocumentsController(IFileSearchService fileSearchService, IFileValidationService fileValidation)
    {
        _fileSearchService = fileSearchService;
        _fileValidation = fileValidation;
    }

    [HttpGet]
    public async Task<ActionResult<List<DocumentInfo>>> GetDocuments([FromQuery] string storeName)
    {
        var documents = await _fileSearchService.ListFilesAsync(storeName);
        return Ok(documents);
    }
    
    [HttpGet("supported-types")]
    public ActionResult<List<string>> GetSupportedFileTypes()
    {
        var extensions = _fileValidation.GetSupportedExtensions();
        return Ok(new { 
            extensions = extensions,
            count = extensions.Count,
            message = "Supported file types for Google File Search"
        });
    }

    [HttpPost("upload")]
    public async Task<ActionResult<string>> UploadDocument([FromQuery] string storeName, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        // Validate file before processing
        using (var validationStream = file.OpenReadStream())
        {
            var validation = await _fileValidation.ValidateFileAsync(validationStream, file.FileName);
            
            if (!validation.IsValid)
            {
                return BadRequest(new
                {
                    error = validation.ErrorMessage,
                    fileName = file.FileName,
                    extension = validation.Extension,
                    isSpoofed = validation.IsPotentiallySpoofed
                });
            }
        }

        // Create a temp file to store the upload
        var tempPath = Path.GetTempFileName();
        var originalExt = Path.GetExtension(file.FileName);
        var newPath = Path.ChangeExtension(tempPath, originalExt);
        
        // Move temp file to have correct extension (important for mime type detection)
        System.IO.File.Move(tempPath, newPath, overwrite: true);
        tempPath = newPath;

        try
        {
            using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var operationName = await _fileSearchService.UploadDocumentAsync(storeName, tempPath);
            return Ok(new { 
                operation = operationName,
                fileName = file.FileName,
                size = file.Length,
                message = "File uploaded and validated successfully"
            });
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
