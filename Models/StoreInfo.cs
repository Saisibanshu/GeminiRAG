namespace GeminiRAG.Models;

/// <summary>
/// Information about a FileSearch store
/// </summary>
public class StoreInfo
{
    public required string Name { get; set; } // Full name like "fileSearchStores/abc123"
    public required string DisplayName { get; set; }
    public DateTime? CreateTime { get; set; }
    public DateTime? UpdateTime { get; set; }
}
