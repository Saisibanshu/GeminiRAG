using GeminiRAG.Models;
using System.Text;
using System.Text.Json;

namespace GeminiRAG.Services;

/// <summary>
/// Service for exporting query history to different formats
/// </summary>
public class ExportService : IExportService
{
    public async Task ExportToJsonAsync(List<QueryHistory> history, string filePath)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(new
        {
            exportDate = DateTime.UtcNow,
            totalQueries = history.Count,
            queries = history
        }, options);

        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task ExportToMarkdownAsync(List<QueryHistory> history, string filePath)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("# Query History Export");
        sb.AppendLine();
        sb.AppendLine($"**Export Date:** {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"**Total Queries:** {history.Count}");
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();

        foreach (var entry in history)
        {
            sb.AppendLine($"## Query {history.IndexOf(entry) + 1}");
            sb.AppendLine();
            sb.AppendLine($"**Time:** {entry.Timestamp:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"**Response Time:** {entry.ResponseTime.TotalMilliseconds:F0}ms");
            sb.AppendLine($"**Found:** {(entry.IsFound ? "✅ Yes" : "❌ No")}");
            sb.AppendLine();
            sb.AppendLine($"**Question:**");
            sb.AppendLine($"> {entry.Question}");
            sb.AppendLine();

            if (entry.IsFound && !string.IsNullOrEmpty(entry.Answer))
            {
                sb.AppendLine($"**Answer:**");
                sb.AppendLine(entry.Answer);
                sb.AppendLine();

                if (entry.Citations.Count > 0)
                {
                    sb.AppendLine($"**Citations:**");
                    foreach (var citation in entry.Citations)
                    {
                        var display = string.IsNullOrEmpty(citation.Preview)
                            ? citation.Source
                            : $"{citation.Source}: {citation.Preview}";
                        sb.AppendLine($"- {display}");
                    }
                    sb.AppendLine();
                }
            }

            sb.AppendLine("---");
            sb.AppendLine();
        }

        await File.WriteAllTextAsync(filePath, sb.ToString());
    }
}
