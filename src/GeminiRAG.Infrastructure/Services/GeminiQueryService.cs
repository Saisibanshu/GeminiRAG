using GeminiRAG.Core.Models;
using GeminiRAG.Core.Interfaces;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace GeminiRAG.Infrastructure.Services;

/// <summary>
/// Service for querying Gemini with FileSearch tool
/// </summary>
public class GeminiQueryService : IGeminiQueryService
{
    private readonly string _apiKey;

    public GeminiQueryService(string apiKey)
    {
        _apiKey = apiKey;
    }

    public async Task<QueryResponse> QueryAsync(QueryRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = new QueryResponse();

        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("x-goog-api-key", _apiKey);

            // Build request for FileSearch tool
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[] { new { text = request.Question } }
                    }
                },
                tools = new[]
                {
                    new
                    {
                        file_search = new
                        {
                            file_search_store_names = new[] { request.FileSearchStoreName }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = request.Temperature,
                    maxOutputTokens = request.MaxOutputTokens
                },
                systemInstruction = new
                {
                    parts = new[]
                    {
                        new
                        {
                            text = @"You are a helpful assistant that answers questions STRICTLY based on the provided documents.
CRITICAL RULES:
1. ONLY use information found in the retrieved document chunks from FileSearch
2. If the answer is not in the documents, say EXACTLY: 'I could not find that information in the uploaded documents.'
3. Do NOT use your general knowledge or training data
4. Do NOT make assumptions or inferences beyond what's explicitly stated
5. Cite the specific parts of the document you're using"
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { WriteIndented = false });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var apiResponse = await httpClient.PostAsync(
                "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent",
                content
            );

            var responseContent = await apiResponse.Content.ReadAsStringAsync();

            if (!apiResponse.IsSuccessStatusCode)
            {
                response.ErrorMessage = $"API Error: {responseContent}";
                response.IsFound = false;
                return response;
            }

            // Parse response
            var result = JsonDocument.Parse(responseContent);
            if (result.RootElement.TryGetProperty("candidates", out var candidates) &&
                candidates.GetArrayLength() > 0)
            {
                var candidate = candidates[0];
                if (candidate.TryGetProperty("content", out var contentObj) &&
                    contentObj.TryGetProperty("parts", out var parts) &&
                    parts.GetArrayLength() > 0)
                {
                    var textPart = parts[0];
                    if (textPart.TryGetProperty("text", out var textValue))
                    {
                        var answer = textValue.GetString() ?? string.Empty;

                        // Check if model couldn't find information
                        if (answer.Contains("could not find", StringComparison.OrdinalIgnoreCase) ||
                            answer.Contains("not in the document", StringComparison.OrdinalIgnoreCase))
                        {
                            response.IsFound = false;
                            response.Answer = string.Empty;
                        }
                        else
                        {
                            response.Answer = answer;
                            response.IsFound = true;

                            // Extract grounding metadata/citations
                            if (candidate.TryGetProperty("groundingMetadata", out var groundingMetadata))
                            {
                                response.Citations = ExtractCitations(groundingMetadata);
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            response.ErrorMessage = $"Query error: {ex.Message}";
            response.IsFound = false;
        }

        stopwatch.Stop();
        response.ResponseTime = stopwatch.Elapsed;
        return response;
    }

    private List<Citation> ExtractCitations(JsonElement groundingMetadata)
    {
        var citations = new List<Citation>();

        try
        {
            if (groundingMetadata.TryGetProperty("groundingChunks", out var chunks))
            {
                foreach (var chunk in chunks.EnumerateArray())
                {
                    if (chunk.TryGetProperty("retrievedContext", out var context))
                    {
                        var citation = new Citation();

                        if (context.TryGetProperty("title", out var title))
                        {
                            citation.Source = title.GetString() ?? "Document chunk";
                        }
                        else if (context.TryGetProperty("text", out var text))
                        {
                            var preview = text.GetString();
                            if (!string.IsNullOrEmpty(preview))
                            {
                                citation.Source = "Chunk";
                                citation.Preview = preview.Length > 100
                                    ? preview.Substring(0, 100) + "..."
                                    : preview;
                            }
                        }

                        citations.Add(citation);
                    }
                }
            }
        }
        catch
        {
            // Ignore citation extraction errors
        }

        return citations;
    }
}
