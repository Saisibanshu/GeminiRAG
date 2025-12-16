namespace GeminiRAG.Core.Interfaces;

/// <summary>
/// Service for validating files against Google File Search supported formats
/// and preventing extension spoofing attacks
/// </summary>
public interface IFileValidationService
{
    /// <summary>
    /// Validates if a file is supported by Google File Search
    /// Checks both extension and actual file content
    /// </summary>
    Task<FileValidationResult> ValidateFileAsync(Stream fileStream, string fileName);
    
    /// <summary>
    /// Gets list of all supported file extensions
    /// </summary>
    List<string> GetSupportedExtensions();
}

public class FileValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public string? DetectedMimeType { get; set; }
    public string? Extension { get; set; }
    public bool IsPotentiallySpoofed { get; set; }
}
