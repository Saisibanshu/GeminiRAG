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

            // Create File Search Store
            await fileSearchService.CreateStoreAsync(geminiSettings.FileSearchStoreName);

            // Get PDF file path from user
            ConsoleUI.WriteInfo("Please provide the path to your PDF file.");
            var pdfPath = ConsoleUI.PromptForInput("PDF file path");


            if (string.IsNullOrWhiteSpace(pdfPath))
            {
                ConsoleUI.WriteError("No PDF path provided.");
                return;
            }

            // Upload PDF
            await fileSearchService.UploadPdfAsync(pdfPath);

            ConsoleUI.WriteSuccess("\n✓ Setup complete! You can now ask questions about your PDF.");
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
    }
}
