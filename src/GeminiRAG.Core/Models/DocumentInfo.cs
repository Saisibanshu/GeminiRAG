namespace GeminiRAG.Core.Models;

/// <summary>
/// Information about an uploaded document
/// </summary>
public class DocumentInfo
{
    public required string Name { get; set; } // File name in store (e.g., "files/abc123")
    public required string DisplayName { get; set; } // Original filename
    public string MimeType { get; set; } = "application/pdf";
    public DateTime? UploadDate { get; set; }
    public string? Status { get; set; }
}
