namespace GeminiRAG.Core.Configuration;

/// <summary>
/// Strongly-typed configuration for Gemini API
/// </summary>
public class GeminiSettings
{
    public required string ApiKey { get; set; }
    public string FileSearchStoreName { get; set; } = "rag-document-store";
}
