namespace GeminiRAG.Core.Models;

/// <summary>
/// Request for querying the RAG system
/// </summary>
public class QueryRequest
{
    public required string Question { get; set; }
    public required string FileSearchStoreName { get; set; }
    public float Temperature { get; set; } = 0.0f; // Strict grounding by default
    public int MaxOutputTokens { get; set; } = 2048;
}
