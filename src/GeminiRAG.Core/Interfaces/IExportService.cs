using GeminiRAG.Core.Models;

namespace GeminiRAG.Core.Interfaces;

/// <summary>
/// Service for exporting query history
/// </summary>
public interface IExportService
{
    Task ExportToJsonAsync(List<QueryHistory> history, string filePath);
    Task ExportToMarkdownAsync(List<QueryHistory> history, string filePath);
}
