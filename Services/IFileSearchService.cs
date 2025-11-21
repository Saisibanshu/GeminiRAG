namespace GeminiRAG.Services;

/// <summary>
/// Service for managing FileSearch operations
/// </summary>
public interface IFileSearchService
{
    Task<string> CreateStoreAsync(string displayName);
    Task<string> UploadPdfAsync(string filePath);
    Task DeleteStoreAsync();
    string? GetStoreName();
}
