using GeminiRAG.Core.Models;

namespace GeminiRAG.Core.Interfaces;

/// <summary>
/// Service for tracking query history
/// </summary>
public interface IQueryHistoryService
{
    void AddQuery(QueryHistory entry);
    List<QueryHistory> GetHistory();
    void ClearHistory();
}
