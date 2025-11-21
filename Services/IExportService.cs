using GeminiRAG.Models;

namespace GeminiRAG.Services;

/// <summary>
/// Service for exporting query history
/// </summary>
public interface IExportService
{
    Task ExportToJsonAsync(List<QueryHistory> history, string filePath);
    Task ExportToMarkdownAsync(List<QueryHistory> history, string filePath);
}
