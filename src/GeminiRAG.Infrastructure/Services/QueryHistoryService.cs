using GeminiRAG.Core.Models;
using GeminiRAG.Core.Interfaces;

namespace GeminiRAG.Infrastructure.Services;

/// <summary>
/// Service for tracking query history
/// </summary>
public class QueryHistoryService : IQueryHistoryService
{
    private readonly List<QueryHistory> _history = new();

    public Task AddQueryAsync(QueryHistory entry)
    {
        _history.Add(entry);
        return Task.CompletedTask;
    }

    public Task<List<QueryHistory>> GetHistoryAsync()
    {
        return Task.FromResult(_history.ToList()); // Return copy
    }

    public Task ClearHistoryAsync()
    {
        _history.Clear();
        return Task.CompletedTask;
    }
}
