using GeminiRAG.Core.Models;

namespace GeminiRAG.Core.Interfaces;

/// <summary>
/// Service for tracking query history with user isolation
/// </summary>
public interface IQueryHistoryService
{
    Task AddQueryAsync(Guid userId, Guid? storeId, QueryHistory entry);
    Task<List<QueryHistory>> GetHistoryAsync(Guid userId);
    Task ClearHistoryAsync(Guid userId);
}
