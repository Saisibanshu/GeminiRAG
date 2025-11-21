namespace GeminiRAG.Models;

/// <summary>
/// Response from querying the RAG system
/// </summary>
public class QueryResponse
{
    public string Answer { get; set; } = string.Empty;
    public List<Citation> Citations { get; set; } = new();
    public bool IsFound { get; set; }
    public TimeSpan ResponseTime { get; set; }
    public string? ErrorMessage { get; set; }
}
