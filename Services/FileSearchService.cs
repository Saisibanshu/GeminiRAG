using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using GeminiRAG.UI;

namespace GeminiRAG.Services;

/// <summary>
/// Service for managing FileSearch operations using Gemini API REST endpoints
/// </summary>
public class FileSearchService : IFileSearchService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _baseUrl = "https://generativelanguage.googleapis.com/v1beta";
    private string? _storeName;

    public FileSearchService(string apiKey)
    {
        _apiKey = apiKey;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("x-goog-api-key", apiKey);
    }

    public async Task<string> CreateStoreAsync(string displayName)
    {
        ConsoleUI.WriteInfo($"Creating File Search store: {displayName}...");

        var requestBody = new { displayName = displayName };
        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_baseUrl}/fileSearchStores", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to create File Search store: {responseContent}");
        }

        var result = JsonSerializer.Deserialize<FileSearchStoreResponse>(responseContent);
        _storeName = result?.Name ?? throw new Exception("Store name not returned");

        ConsoleUI.WriteSuccess($"✓ File Search store created: {_storeName}");
        return _storeName;
    }

    public async Task<string> UploadPdfAsync(string filePath)
    {
        if (string.IsNullOrEmpty(_storeName))
        {
            throw new InvalidOperationException("Create a store first before uploading files");
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        ConsoleUI.WriteInfo($"Uploading PDF: {Path.GetFileName(filePath)}...");
        ConsoleUI.WriteInfo("This may take a while for large files...");

        var fileBytes = await File.ReadAllBytesAsync(filePath);
        var mimeType = "application/pdf";
        var numBytes = fileBytes.Length;

        // Step 1: Initiate resumable upload
        ConsoleUI.WriteInfo("Initiating resumable upload...");
        
        var initiateUrl = $"https://generativelanguage.googleapis.com/upload/v1beta/{_storeName}:uploadToFileSearchStore?key={_apiKey}";
        
        using var initiateRequest = new HttpRequestMessage(HttpMethod.Post, initiateUrl);
        initiateRequest.Headers.Add("X-Goog-Upload-Protocol", "resumable");
        initiateRequest.Headers.Add("X-Goog-Upload-Command", "start");
        initiateRequest.Headers.Add("X-Goog-Upload-Header-Content-Length", numBytes.ToString());
        initiateRequest.Headers.Add("X-Goog-Upload-Header-Content-Type", mimeType);
        initiateRequest.Content = new StringContent("{}", Encoding.UTF8, "application/json");

        var initiateResponse = await _httpClient.SendAsync(initiateRequest);
        
        if (!initiateResponse.IsSuccessStatusCode)
        {
            var error = await initiateResponse.Content.ReadAsStringAsync();
            throw new Exception($"Failed to initiate upload: {error}");
        }

        // Extract upload URL from response headers
        if (!initiateResponse.Headers.TryGetValues("X-Goog-Upload-URL", out var uploadUrls))
        {
            throw new Exception("Upload URL not returned in response headers");
        }

        var uploadUrl = uploadUrls.First();
        ConsoleUI.WriteInfo("Upload URL obtained. Uploading file bytes...");

        // Step 2: Upload the actual file bytes
        using var uploadRequest = new HttpRequestMessage(HttpMethod.Post, uploadUrl);
        uploadRequest.Headers.Add("X-Goog-Upload-Offset", "0");
        uploadRequest.Headers.Add("X-Goog-Upload-Command", "upload, finalize");
        uploadRequest.Content = new ByteArrayContent(fileBytes);

        var uploadResponse = await _httpClient.SendAsync(uploadRequest);
        var uploadResponseContent = await uploadResponse.Content.ReadAsStringAsync();

        if (!uploadResponse.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to upload file bytes: {uploadResponseContent}");
        }

        // Parse operation response
        var operation = JsonSerializer.Deserialize<OperationResponse>(uploadResponseContent);
        var operationName = operation?.Name ?? throw new Exception("Operation name not returned");

        ConsoleUI.WriteInfo("Upload complete. Waiting for indexing...");

        // Step 3: Wait for operation to complete
        await WaitForOperationAsync(operationName);

        ConsoleUI.WriteSuccess($"✓ PDF uploaded and indexed successfully!");
        return operationName;
    }

    public async Task DeleteStoreAsync()
    {
        if (string.IsNullOrEmpty(_storeName))
        {
            return;
        }

        ConsoleUI.WriteInfo($"Deleting File Search store: {_storeName}...");

        var response = await _httpClient.DeleteAsync($"{_baseUrl}/{_storeName}");

        if (response.IsSuccessStatusCode)
        {
            ConsoleUI.WriteSuccess("✓ File Search store deleted");
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            ConsoleUI.WriteError($"Failed to delete store: {error}");
        }
    }

    public string? GetStoreName() => _storeName;

    private async Task WaitForOperationAsync(string operationName)
    {
        var maxAttempts = 60; // 5 minutes max wait time
        var attempt = 0;

        while (attempt < maxAttempts)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/{operationName}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to check operation status: {responseContent}");
            }

            var operation = JsonSerializer.Deserialize<OperationResponse>(responseContent);

            if (operation?.Done == true)
            {
                if (operation.Error != null)
                {
                    throw new Exception($"Operation failed: {operation.Error.Message}");
                }
                return; // Success
            }

            // Progress indicator
            Console.Write(".");
            await Task.Delay(5000); // Wait 5 seconds between checks
            attempt++;
        }

        throw new TimeoutException("Upload operation timed out");
    }
}

// Internal response models
internal class FileSearchStoreResponse
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("createTime")]
    public string? CreateTime { get; set; }

    [JsonPropertyName("updateTime")]
    public string? UpdateTime { get; set; }
}

internal class OperationResponse
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("done")]
    public bool Done { get; set; }

    [JsonPropertyName("error")]
    public ErrorInfo? Error { get; set; }

    [JsonPropertyName("metadata")]
    public JsonElement? Metadata { get; set; }

    [JsonPropertyName("response")]
    public JsonElement? Response { get; set; }
}

internal class ErrorInfo
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}
