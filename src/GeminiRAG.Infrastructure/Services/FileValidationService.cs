using GeminiRAG.Core.Interfaces;

namespace GeminiRAG.Infrastructure.Services;

/// <summary>
/// Implements file validation with content-based verification to prevent spoofing
/// Based on Google File Search supported formats
/// </summary>
public class FileValidationService : IFileValidationService
{
    // Google File Search supported extensions (based on official documentation)
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        // Documents
        ".pdf", ".doc", ".docx", ".docm", ".odt", ".rtf",
        
        // Spreadsheets
        ".xls", ".xlsx", ".xlsm", ".csv", ".tsv",
        
        // Presentations
        ".ppt", ".pptx",
        
        // Text files
        ".txt", ".md", ".markdown", ".rst", ".tex", ".latex",
        
        // Code files
        ".js", ".jsx", ".ts", ".tsx", ".py", ".java", ".c", ".cpp", ".h", ".hpp",
        ".cs", ".go", ".rs", ".rb", ".php", ".swift", ".kt", ".scala", ".r",
        ".dart", ".lua", ".pl", ".sh", ".bash", ".zsh", ".ps1", ".sql",
        ".xml", ".json", ".yaml", ".yml", ".toml", ".ini", ".cfg",
        
        // Markup
        ".html", ".htm", ".css", ".scss", ".sass", ".less",
        
        // Other
        ".hwp", ".zip"
    };

    // Magic bytes for common file types to detect spoofing
    private static readonly Dictionary<string, byte[][]> FileMagicBytes = new()
    {
        { ".pdf", new[] { new byte[] { 0x25, 0x50, 0x44, 0x46 } } }, // %PDF
        { ".zip", new[] { 
            new byte[] { 0x50, 0x4B, 0x03, 0x04 }, // PK..
            new byte[] { 0x50, 0x4B, 0x05, 0x06 }, // PK..
            new byte[] { 0x50, 0x4B, 0x07, 0x08 }  // PK..
        }},
        { ".docx", new[] { new byte[] { 0x50, 0x4B, 0x03, 0x04 } } }, // DOCX is ZIP
        { ".xlsx", new[] { new byte[] { 0x50, 0x4B, 0x03, 0x04 } } }, // XLSX is ZIP
        { ".pptx", new[] { new byte[] { 0x50, 0x4B, 0x03, 0x04 } } }, // PPTX is ZIP
        { ".exe", new[] { new byte[] { 0x4D, 0x5A } } }, // MZ (to detect and block)
        { ".dll", new[] { new byte[] { 0x4D, 0x5A } } }, // MZ (to detect and block)
        { ".png", new[] { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
        { ".jpg", new[] { 
            new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
            new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 },
            new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 }
        }},
        { ".gif", new[] { 
            new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 }, // GIF87a
            new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }  // GIF89a
        }}
    };

    // Blocked extensions (executable, scripts that shouldn't be uploaded)
    private static readonly HashSet<string> BlockedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".exe", ".dll", ".bat", ".cmd", ".com", ".scr", ".vbs", ".vbe",
        ".msi", ".app", ".deb", ".rpm", ".dmg", ".pkg", ".apk", ".ipa"
    };

    public async Task<FileValidationResult> ValidateFileAsync(Stream fileStream, string fileName)
    {
        var result = new FileValidationResult
        {
            Extension = Path.GetExtension(fileName).ToLowerInvariant()
        };

        // Check if extension is explicitly blocked
        if (BlockedExtensions.Contains(result.Extension))
        {
            result.IsValid = false;
            result.ErrorMessage = $"File type '{result.Extension}' is not allowed for security reasons.";
            return result;
        }

        // Check if extension is in supported list
        if (!SupportedExtensions.Contains(result.Extension))
        {
            result.IsValid = false;
            result.ErrorMessage = $"File type '{result.Extension}' is not supported by Google File Search. " +
                                $"Supported formats include: PDF, DOCX, TXT, MD, code files, and more.";
            return result;
        }

        // Read first 8 bytes for magic number detection
        var buffer = new byte[8];
        fileStream.Position = 0;
        var bytesRead = await fileStream.ReadAsync(buffer, 0, 8);
        fileStream.Position = 0; // Reset for later use

        // Check for executable magic bytes (security check)
        if (IsExecutable(buffer))
        {
            result.IsValid = false;
            result.IsPotentiallySpoofed = true;
            result.ErrorMessage = "File appears to be an executable disguised with a different extension. Upload blocked for security.";
            return result;
        }

        // Verify file content matches extension for binary formats
        if (FileMagicBytes.ContainsKey(result.Extension))
        {
            if (!MatchesMagicBytes(buffer, result.Extension))
            {
                result.IsPotentiallySpoofed = true;
                result.ErrorMessage = $"File content doesn't match the '{result.Extension}' extension. This may be a renamed file.";
                result.IsValid = false;
                return result;
            }
        }

        // Additional validation for text-based files
        if (IsTextBasedExtension(result.Extension))
        {
            // Text files should be valid UTF-8 or ASCII
            try
            {
                var textBuffer = new byte[1024];
                fileStream.Position = 0;
                var textBytesRead = await fileStream.ReadAsync(textBuffer, 0, textBuffer.Length);
                fileStream.Position = 0;

                // Check if it's valid text (not binary garbage)
                if (!IsValidTextContent(textBuffer, textBytesRead))
                {
                    result.IsValid = false;
                    result.IsPotentiallySpoofed = true;
                    result.ErrorMessage = "File appears to contain binary data but has a text file extension.";
                    return result;
                }
            }
            catch
            {
                // If we can't read it as text, it's suspicious
                result.IsValid = false;
                result.ErrorMessage = "Unable to validate text file content.";
                return result;
            }
        }

        result.IsValid = true;
        return result;
    }

    public List<string> GetSupportedExtensions()
    {
        return SupportedExtensions.OrderBy(e => e).ToList();
    }

    private bool IsExecutable(byte[] buffer)
    {
        // Check for Windows executable (MZ)
        if (buffer.Length >= 2 && buffer[0] == 0x4D && buffer[1] == 0x5A)
            return true;

        // Check for ELF (Linux executable) 
        if (buffer.Length >= 4 && buffer[0] == 0x7F && buffer[1] == 0x45 && 
            buffer[2] == 0x4C && buffer[3] == 0x46)
            return true;

        // Check for Mach-O (macOS executable)
        if (buffer.Length >= 4)
        {
            var magic = BitConverter.ToUInt32(buffer, 0);
            if (magic == 0xFEEDFACE || magic == 0xFEEDFACF || 
                magic == 0xCEFAEDFE || magic == 0xCFFAEDFE)
                return true;
        }

        return false;
    }

    private bool MatchesMagicBytes(byte[] buffer, string extension)
    {
        if (!FileMagicBytes.ContainsKey(extension))
            return true; // No magic bytes defined, assume valid

        var magicBytesList = FileMagicBytes[extension];
        
        foreach (var magicBytes in magicBytesList)
        {
            if (buffer.Length < magicBytes.Length)
                continue;

            bool matches = true;
            for (int i = 0; i < magicBytes.Length; i++)
            {
                if (buffer[i] != magicBytes[i])
                {
                    matches = false;
                    break;
                }
            }

            if (matches)
                return true;
        }

        return false;
    }

    private bool IsTextBasedExtension(string extension)
    {
        var textExtensions = new[]
        {
            ".txt", ".md", ".markdown", ".rst", ".csv", ".tsv", ".json", ".xml",
            ".html", ".htm", ".css", ".js", ".ts", ".py", ".java", ".c", ".cpp",
            ".h", ".cs", ".go", ".rs", ".rb", ".php", ".sh", ".sql", ".yaml", ".yml"
        };

        return textExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }

    private bool IsValidTextContent(byte[] buffer, int length)
    {
        int nullBytes = 0;
        int controlChars = 0;

        for (int i = 0; i < length; i++)
        {
            byte b = buffer[i];

            // Count null bytes
            if (b == 0)
                nullBytes++;

            // Count non-printable control characters (excluding common ones like \n, \r, \t)
            if (b < 32 && b != 9 && b != 10 && b != 13)
                controlChars++;
        }

        // If more than 5% null bytes or 10% control chars, probably binary
        double nullRatio = (double)nullBytes / length;
        double controlRatio = (double)controlChars / length;

        return nullRatio < 0.05 && controlRatio < 0.10;
    }
}
