using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using GeminiRAG.Core.Models;
using GeminiRAG.Core.Interfaces;


namespace GeminiRAG.Infrastructure.Services;

/// <summary>
/// Service for managing FileSearch operations using Gemini API REST endpoints
/// </summary>
public class FileSearchService : IFileSearchService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _baseUrl = "https://generativelanguage.googleapis.com/v1beta";


    public FileSearchService(string apiKey)
    {
        _apiKey = apiKey;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("x-goog-api-key", apiKey);
    }

    public async Task<List<StoreInfo>> ListStoresAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/fileSearchStores");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[WARN] {responseContent}");
                return new List<StoreInfo>();
            }

            var result = JsonSerializer.Deserialize<StoreListResponse>(responseContent);
            return result?.FileSearchStores?.Select(s => new StoreInfo
            {
                Name = s.Name ?? "",
                DisplayName = s.DisplayName ?? "Unnamed Store",
                CreateTime = DateTime.TryParse(s.CreateTime, out var create) ? create : null,
                UpdateTime = DateTime.TryParse(s.UpdateTime, out var update) ? update : null
            }).ToList() ?? new List<StoreInfo>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WARN] Error listing stores: {ex.Message}");
            return new List<StoreInfo>();
        }
    }



    public async Task<string> CreateStoreAsync(string displayName)
    {
        Console.WriteLine($"[INFO] Creating File Search store: {displayName}...");

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
        Console.WriteLine($"[SUCCESS] ✓ File Search store created: {result?.Name}");
        return result?.Name ?? throw new Exception("Store name not returned");
    }

    public async Task<string> UploadPdfAsync(string storeName, string filePath)
    {
        if (string.IsNullOrEmpty(storeName))
        {
            throw new ArgumentException("Store name cannot be empty", nameof(storeName));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        Console.WriteLine($"[INFO] Uploading PDF: {Path.GetFileName(filePath)}...");
Console.WriteLine("[INFO] This may take a while for large files...");

        var fileBytes = await File.ReadAllBytesAsync(filePath);
        var mimeType = "application/pdf";
        var numBytes = fileBytes.Length;

        // Step 1: Initiate resumable upload
        Console.WriteLine("[INFO] Initiating resumable upload...");
        
        var initiateUrl = $"https://generativelanguage.googleapis.com/upload/v1beta/{storeName}:uploadToFileSearchStore?key={_apiKey}";
        
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
        Console.WriteLine("[INFO] Upload URL obtained. Uploading file bytes...");

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

        Console.WriteLine("[INFO] Upload complete. Waiting for indexing...");

        // Step 3: Wait for operation to complete
        await WaitForOperationAsync(operationName);

        Console.WriteLine($"[SUCCESS] ✓ PDF uploaded and indexed successfully!");
        return operationName;
    }

    public async Task<List<string>> UploadMultiplePdfsAsync(string storeName, string[] filePaths)
    {
        var operationNames = new List<string>();
        
        Console.WriteLine($"[INFO] Uploading {filePaths.Length} PDF files...");
        
        foreach (var filePath in filePaths)
        {
            try
            {
                var operationName = await UploadPdfAsync(storeName, filePath);
                operationNames.Add(operationName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to upload {Path.GetFileName(filePath)}: {ex.Message}");
            }
        }
        
        Console.WriteLine($"[SUCCESS] ✓ Uploaded {operationNames.Count}/{filePaths.Length} files successfully!");
        return operationNames;
    }

    public async Task<List<DocumentInfo>> ListFilesAsync(string storeName)
    {
        if (string.IsNullOrEmpty(storeName))
        {
            return new List<DocumentInfo>();
        }

        try
        {
            // Correct endpoint: {storeName}/documents not {storeName}/files
            var response = await _httpClient.GetAsync($"{_baseUrl}/{storeName}/documents");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[WARN] Could not list files: {responseContent}");
                return new List<DocumentInfo>();
            }

            // Parse document list response
            var result = JsonSerializer.Deserialize<DocumentListResponse>(responseContent);
            return result?.Documents?.Select(d => new DocumentInfo
            {
                Name = d.Name ?? "",
                DisplayName = d.DisplayName ?? Path.GetFileName(d.Name ?? ""),
                MimeType = d.MimeType ?? "application/pdf",
                UploadDate = DateTime.TryParse(d.CreateTime, out var date) ? date : null,
                Status = "Active"
            }).ToList() ?? new List<DocumentInfo>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WARN] Error listing files: {ex.Message}");
            return new List<DocumentInfo>();
        }
    }

    public async Task DeleteStoreAsync(string storeName)
    {
        if (string.IsNullOrEmpty(storeName))
        {
            return;
        }

        Console.WriteLine($"[INFO] Deleting File Search store: {storeName}...");

        var response = await _httpClient.DeleteAsync($"{_baseUrl}/{storeName}");

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("[SUCCESS] ✓ File Search store deleted");
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[ERROR] Failed to delete store: {error}");
            throw new Exception(error); // Propagate error to controller
        }
    }



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

internal class DocumentListResponse
{
    [JsonPropertyName("documents")]
    public List<FileSearchDocument>? Documents { get; set; }
}

internal class FileSearchDocument
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("mimeType")]
    public string? MimeType { get; set; }

    [JsonPropertyName("createTime")]
    public string? CreateTime { get; set; }
}

internal class StoreListResponse
{
    [JsonPropertyName("fileSearchStores")]
    public List<FileSearchStoreResponse>? FileSearchStores { get; set; }
}
