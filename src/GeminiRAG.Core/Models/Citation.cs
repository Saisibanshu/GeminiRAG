namespace GeminiRAG.Core.Models;

/// <summary>
/// Citation from document chunks
/// </summary>
public class Citation
{
    public string Source { get; set; } = string.Empty;
    public string? Preview { get; set; }
}
