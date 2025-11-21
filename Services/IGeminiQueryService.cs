using GeminiRAG.Models;

namespace GeminiRAG.Services;

/// <summary>
/// Service for querying Gemini with FileSearch
/// </summary>
public interface IGeminiQueryService
{
    Task<QueryResponse> QueryAsync(QueryRequest request);
}
