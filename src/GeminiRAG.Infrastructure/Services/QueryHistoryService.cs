using GeminiRAG.Core.Models;
using GeminiRAG.Core.Interfaces;

namespace GeminiRAG.Infrastructure.Services;

/// <summary>
/// Service for tracking query history
/// </summary>
public class QueryHistoryService : IQueryHistoryService
{
    private readonly List<QueryHistory> _history = new();

    public void AddQuery(QueryHistory entry)
    {
        _history.Add(entry);
    }

    public List<QueryHistory> GetHistory()
    {
        return _history.ToList(); // Return copy
    }

    public void ClearHistory()
    {
        _history.Clear();
    }
}
