namespace GeminiRAG.Models;

/// <summary>
/// Query history entry for tracking
/// </summary>
public class QueryHistory
{
    public DateTime Timestamp { get; set; }
    public required string Question { get; set; }
    public string Answer { get; set; } = string.Empty;
    public List<Citation> Citations { get; set; } = new();
    public TimeSpan ResponseTime { get; set; }
    public bool IsFound { get; set; }
}
