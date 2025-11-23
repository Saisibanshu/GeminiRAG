using GeminiRAG.Core.Models;

namespace GeminiRAG.Core.Interfaces;

/// <summary>
/// Service for tracking query history
/// </summary>
public interface IQueryHistoryService
{
    Task AddQueryAsync(QueryHistory entry);
    Task<List<QueryHistory>> GetHistoryAsync();
    Task ClearHistoryAsync();
}
