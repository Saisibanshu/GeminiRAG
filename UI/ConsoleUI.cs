using GeminiRAG.Models;

namespace GeminiRAG.UI;

/// <summary>
/// Console user interface helper methods
/// </summary>
public static class ConsoleUI
{
    public static void WriteHeader(string text)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine();
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine($"  {text}");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.ResetColor();
        Console.WriteLine();
    }

    public static void WriteInfo(string text)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"ℹ {text}");
        Console.ResetColor();
    }

    public static void WriteSuccess(string text)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"{text}");
        Console.ResetColor();
    }

    public static void WriteError(string text)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"✗ ERROR: {text}");
        Console.ResetColor();
    }

    public static void WriteWarning(string text)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"⚠ {text}");
        Console.ResetColor();
    }

    public static void WriteAnswer(string answer)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine();
        Console.WriteLine("┌─ Answer ────────────────────────────────────────────────────┐");
        Console.ResetColor();
        Console.WriteLine($"  {answer}");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘");
        Console.ResetColor();
        Console.WriteLine();
    }

    public static void WriteCitations(List<Citation> citations)
    {
        if (citations.Count == 0) return;

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("📚 Sources:");
        foreach (var citation in citations)
        {
            var display = string.IsNullOrEmpty(citation.Preview)
                ? citation.Source
                : $"{citation.Source}: {citation.Preview}";
            Console.WriteLine($"   • {display}");
        }
        Console.ResetColor();
        Console.WriteLine();
    }

    public static string? PromptForInput(string prompt)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"{prompt}: ");
        Console.ResetColor();
        return Console.ReadLine();
    }

    public static void WriteNotFound()
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine();
        Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  Sorry, I couldn't find that information in the          ║");
        Console.WriteLine("║  uploaded documents.                                      ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine();
    }
}
