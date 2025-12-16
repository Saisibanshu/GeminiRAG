
using GeminiRAG.Core.Models;

namespace GeminiRAG.Core.Interfaces;

/// <summary>
/// Service for managing FileSearch operations
/// </summary>
public interface IFileSearchService
{
    Task<List<StoreInfo>> ListStoresAsync();
    Task<string> CreateStoreAsync(string displayName);
    Task<string> UploadDocumentAsync(string storeName, string filePath);
    Task<List<string>> UploadMultiplePdfsAsync(string storeName, string[] filePaths);
    Task<List<DocumentInfo>> ListFilesAsync(string storeName);
    Task DeleteFileAsync(string fileName);
    Task DeleteStoreAsync(string storeName, bool force = false);
}
