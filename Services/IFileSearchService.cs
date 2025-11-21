using GeminiRAG.Models;

namespace GeminiRAG.Services;

/// <summary>
/// Service for managing FileSearch operations
/// </summary>
public interface IFileSearchService
{
    Task<List<StoreInfo>> ListStoresAsync();
    Task<string> CreateStoreAsync(string displayName);
    void UseExistingStore(string storeName);
    Task<string> UploadPdfAsync(string filePath);
    Task<List<string>> UploadMultiplePdfsAsync(string[] filePaths);
    Task<List<DocumentInfo>> ListFilesAsync();
    Task DeleteStoreAsync();
    string? GetStoreName();
}
