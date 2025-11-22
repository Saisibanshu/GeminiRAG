using GeminiRAG.Core.Models;

namespace GeminiRAG.Core.Interfaces;

/// <summary>
/// Service for querying Gemini with FileSearch
/// </summary>
public interface IGeminiQueryService
{
    Task<QueryResponse> QueryAsync(QueryRequest request);
}
