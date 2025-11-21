using GeminiRAG.Configuration;
using GeminiRAG.Models;
using GeminiRAG.Services;
using GeminiRAG.UI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GeminiRAG;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            ConsoleUI.WriteHeader("🤖 Gemini RAG - PDF Question Answering with Strict Grounding");

            // Load configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            // Bind settings
            var geminiSettings = new GeminiSettings
            {
                ApiKey = configuration["GeminiApi:ApiKey"] ?? throw new InvalidOperationException("API Key not configured"),
                FileSearchStoreName = configuration["FileSearchStore:StoreName"] ?? "rag-document-store"
            };

           if (geminiSettings.ApiKey == "YOUR_API_KEY_HERE")
            {
                ConsoleUI.WriteError("API Key not configured!");
                ConsoleUI.WriteInfo("Please update appsettings.json with your Gemini API key.");
                ConsoleUI.WriteInfo("Get your API key from: https://aistudio.google.com/apikey");
                return;
            }

            // Setup Dependency Injection
            var services = new ServiceCollection();
            ConfigureServices(services, geminiSettings);
            var serviceProvider = services.BuildServiceProvider();

            // Get services
            var fileSearchService = serviceProvider.GetRequiredService<IFileSearchService>();
            var queryService = serviceProvider.GetRequiredService<IGeminiQueryService>();
            var historyService = serviceProvider.GetRequiredService<IQueryHistoryService>();

            // List existing stores
            ConsoleUI.WriteInfo("Checking for existing FileSearch stores...");
            var stores = await fileSearchService.ListStoresAsync();

            string? selectedStoreName = null;

            if (stores.Count > 0)
            {
                ConsoleUI.WriteInfo($"\n📦 Found {stores.Count} existing store(s):");
                for (int i = 0; i < stores.Count; i++)
                {
                    var created = stores[i].CreateTime.HasValue 
                        ? stores[i].CreateTime.Value.ToString("yyyy-MM-dd HH:mm")
                        : "unknown";
                    Console.WriteLine($"   {i + 1}. {stores[i].DisplayName} (created: {created})");
                }

                ConsoleUI.WriteInfo("\nOptions:");
                Console.WriteLine("   - Enter store number (1-" + stores.Count + ") to use existing store");
                Console.WriteLine("   - Type 'new' to create a new store");
                
                var choice = ConsoleUI.PromptForInput("Your choice");

                if (int.TryParse(choice, out int storeIndex) && storeIndex > 0 && storeIndex <= stores.Count)
                {
                    selectedStoreName = stores[storeIndex - 1].Name;
                    fileSearchService.UseExistingStore(selectedStoreName);
                }
                else if (choice?.Trim().ToLower() == "new")
                {
                    selectedStoreName = await fileSearchService.CreateStoreAsync(geminiSettings.FileSearchStoreName);
                }
                else
                {
                    ConsoleUI.WriteError("Invalid choice. Exiting.");
                    return;
                }
            }
            else
            {
                ConsoleUI.WriteInfo("No existing stores found. Creating new store...");
                selectedStoreName = await fileSearchService.CreateStoreAsync(geminiSettings.FileSearchStoreName);
            }

            // Check for existing files in selected store
            var existingFiles = await fileSearchService.ListFilesAsync();
            
            if (existingFiles.Count > 0)
            {
                ConsoleUI.WriteInfo($"\n📁 Found {existingFiles.Count} file(s) in this store:");
                for (int i = 0; i < existingFiles.Count; i++)
                {
                    var uploadDate = existingFiles[i].UploadDate.HasValue 
                        ? existingFiles[i].UploadDate.Value.ToString("yyyy-MM-dd HH:mm")
                        : "unknown";
                    Console.WriteLine($"   {i + 1}. {existingFiles[i].DisplayName} (uploaded: {uploadDate})");
                }
                Console.WriteLine();

                ConsoleUI.WriteInfo("What would you like to do?");
                Console.WriteLine("   1. Use existing files (start asking questions)");
                Console.WriteLine("   2. Upload additional PDF(s) to this store");
                Console.WriteLine("   3. Go back and select a different store");
                
                var userChoice = ConsoleUI.PromptForInput("Your choice (1-3)");

                if (userChoice == "1")
                {
                    ConsoleUI.WriteSuccess($"✓ Ready to query {existingFiles.Count} document(s)!");
                }
                else if (userChoice == "2")
                {
                    // Upload new files to existing store
                    var pdfPath = ConsoleUI.PromptForInput("\nPDF file path to upload");
                    if (!string.IsNullOrWhiteSpace(pdfPath))
                    {
                        await fileSearchService.UploadPdfAsync(pdfPath);
                    }
                }
                else if (userChoice == "3")
                {
                    ConsoleUI.WriteInfo("Please restart the application to select a different store.");
                    return;
                }
                else
                {
                    ConsoleUI.WriteError("Invalid choice. Using existing files by default.");
                }
            }
            else
            {
                // Empty store - must upload
                ConsoleUI.WriteInfo("\n📁 This store is empty. Let's upload a PDF.");
                var pdfPath = ConsoleUI.PromptForInput("PDF file path");

                if (string.IsNullOrWhiteSpace(pdfPath))
                {
                    ConsoleUI.WriteError("No PDF path provided.");
                    return;
                }

                await fileSearchService.UploadPdfAsync(pdfPath);
            }

            ConsoleUI.WriteSuccess("\n✓ Setup complete! You can now ask questions about your PDF(s).");
            ConsoleUI.WriteInfo("The system will ONLY answer from the PDF content (strict grounding).");
            ConsoleUI.WriteInfo("Type 'exit' or 'quit' to end the session.\n");

            // Query loop
            while (true)
            {
                var question = ConsoleUI.PromptForInput("\n❓ Your question");

                if (string.IsNullOrWhiteSpace(question))
                {
                    continue;
                }

                if (question.Trim().ToLower() is "exit" or "quit")
                {
                    break;
                }

                try
                {
                    // Query using service
                    var request = new QueryRequest
                    {
                        Question = question,
                        FileSearchStoreName = fileSearchService.GetStoreName()!
                    };

                    ConsoleUI.WriteInfo("Searching in your PDF...");
                    var response = await queryService.QueryAsync(request);

                    if (!string.IsNullOrEmpty(response.ErrorMessage))
                    {
                        ConsoleUI.WriteError(response.ErrorMessage);
                        continue;
                    }

                    // Display citations
                    if (response.Citations.Count > 0)
                    {
                        ConsoleUI.WriteCitations(response.Citations);
                    }

                    // Display answer
                    if (!response.IsFound)
                    {
                        ConsoleUI.WriteNotFound();
                    }
                    else
                    {
                        ConsoleUI.WriteAnswer(response.Answer);
                    }

                    // Add to history
                    historyService.AddQuery(new QueryHistory
                    {
                        Timestamp = DateTime.UtcNow,
                        Question = question,
                        Answer = response.Answer,
                        Citations = response.Citations,
                        ResponseTime = response.ResponseTime,
                        IsFound = response.IsFound
                    });
                }
                catch (Exception ex)
                {
                    ConsoleUI.WriteError($"Query failed: {ex.Message}");
                }
            }

            // Show history summary
            var history = historyService.GetHistory();
            ConsoleUI.WriteInfo($"\n📊 Session Summary: {history.Count} questions asked");

            // Export option
            if (history.Count > 0)
            {
                var exportChoice = ConsoleUI.PromptForInput("\nExport query history? (json/markdown/no)");
                var exportService = serviceProvider.GetRequiredService<IExportService>();

                if (exportChoice?.Trim().ToLower() == "json")
                {
                    var exportPath = $"query-history-{DateTime.Now:yyyy MMddHHmmss}.json";
                    await exportService.ExportToJsonAsync(history, exportPath);
                    ConsoleUI.WriteSuccess($"✓ Exported to: {exportPath}");
                }
                else if (exportChoice?.Trim().ToLower() == "markdown")
                {
                    var exportPath = $"query-history-{DateTime.Now:yyyyMMddHHmmss}.md";
                    await exportService.ExportToMarkdownAsync(history, exportPath);
                    ConsoleUI.WriteSuccess($"✓ Exported to: {exportPath}");
                }
            }

            // Cleanup
            ConsoleUI.WriteInfo("\nCleaning up...");
            var cleanup = ConsoleUI.PromptForInput("Delete File Search store? (y/n)");

            if (cleanup?.Trim().ToLower() == "y")
            {
                await fileSearchService.DeleteStoreAsync();
            }

            ConsoleUI.WriteSuccess("\n👋 Thank you for using Gemini RAG!");
        }
        catch (Exception ex)
        {
            ConsoleUI.WriteError($"Application error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }

    private static void ConfigureServices(IServiceCollection services, GeminiSettings settings)
    {
        // Register services
        services.AddSingleton<IFileSearchService>(sp => new FileSearchService(settings.ApiKey));
        services.AddSingleton<IGeminiQueryService>(sp => new GeminiQueryService(settings.ApiKey));
        services.AddSingleton<IQueryHistoryService, QueryHistoryService>();
        services.AddSingleton<IExportService, ExportService>();
    }
}
