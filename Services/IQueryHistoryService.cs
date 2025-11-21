using GeminiRAG.Models;

namespace GeminiRAG.Services;

/// <summary>
/// Service for tracking query history
/// </summary>
public interface IQueryHistoryService
{
    void AddQuery(QueryHistory entry);
    List<QueryHistory> GetHistory();
    void ClearHistory();
}
