# Gemini RAG - PDF Question Answering

A .NET 9.0 console application that uses Google Gemini API's File Search tool for Retrieval Augmented Generation (RAG). Ask questions about your PDF documents with **strict grounding** - the system only answers from document content, never from the model's general knowledge.

## 🏗️ Architecture

This application uses a **hybrid approach** for maximum compatibility and easy migration:

- **Official Google.GenAI SDK**: For client initialization and future FileSearch integration when supported
- **REST API**: Direct calls to Gemini API's FileSearch endpoints (create store, upload PDF, query with grounding)
- **Easy Migration Path**: When Google.GenAI SDK adds FileSearch support, minimal code changes needed

## ✨ Features

- 📄 Upload large PDF documents for indexing
- 🔍 Semantic search across your documents using FileSearch tool
- 🎯 **Strict grounding**: Only answers from PDF content (no hallucination)
- ❌ Polite "not found" responses when information isn't in the document
- 📚 Citation support showing which document parts were used
- 🎨 Beautiful console UI with colors
- 🔄 Easy migration when official SDK adds FileSearch

## 🚀 Prerequisites

1. **.NET 9.0 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/9.0)
2. **Gemini API Key** - [Get one here](https://aistudio.google.com/apikey)

## 📦 Installation

1. **Clone or download this project**

2. **Configure your API key**:
   Edit `appsettings.json` and replace `YOUR_API_KEY_HERE` with your actual Gemini API key:
   ```json
   {
     "GeminiApi": {
       "ApiKey": "your-actual-api-key-here"
     }
   }
   ```

3. **Restore packages**:
   ```bash
   dotnet restore
   ```

## 🎮 Usage

1. **Run the application**:
   ```bash
   dotnet run
   ```

2. **Provide your PDF path** when prompted:
   ```
   PDF file path: C:\Documents\my-document.pdf
   ```

3. **Ask questions** about your PDF:
   ```
   ❓ Your question: What is the main topic of this document?
   ```

4. **Exit** by typing `exit` or `quit`

## 📖 Example Session

```
═══════════════════════════════════════════════════════════════
  🤖 Gemini RAG - PDF Question Answering with Strict Grounding
═══════════════════════════════════════════════════════════════

ℹ Creating File Search store: rag-document-store...
✓ File Search store created: fileSearchStores/abc123

ℹ Please provide the path to your PDF file.
PDF file path: technical-manual.pdf

ℹ Uploading PDF: technical-manual.pdf...
ℹ This may take a while for large files...
ℹ Upload initiated. Waiting for indexing to complete...
.........
✓ PDF uploaded and indexed successfully!

✓ Setup complete! You can now ask questions about your PDF.
ℹ The system will ONLY answer from the PDF content.
ℹ Type 'exit' or 'quit' to end the session.

❓ Your question: How do I configure the system?

ℹ Searching in your PDF...

┌─ Answer ────────────────────────────────────────────────────┐
  To configure the system, you need to edit the config.ini 
  file and set the following parameters: host, port, and 
  database_name as specified in Section 3.2.
└─────────────────────────────────────────────────────────────┘

📚 Sources:
   • Section 3.2 - Configuration Instructions...

❓ Your question: What is the capital of France?

ℹ Searching in your PDF...

╔═══════════════════════════════════════════════════════════╗
║  Sorry, I couldn't find that information in the          ║
║  uploaded documents.                                      ║
╚═══════════════════════════════════════════════════════════╝
```

## 🔒 Strict Grounding Explained

This application is configured to **prevent hallucination**:

- ✅ **Only uses PDF content**: Answers are sourced exclusively from your uploaded document
- ❌ **No general knowledge**: Won't answer questions outside your PDF's scope
- 🎯 **Factual accuracy**: Temperature set to 0.0 for deterministic responses
- 📝 **Citations**: Shows which parts of the document were used

## 🛠️ Technical Architecture

### Hybrid Approach

1. **Official Google.GenAI SDK** (`Google.GenAI` NuGet package)
   - Client initialization
   - Future FileSearch integration (when SDK adds support)
   - Ensures compatibility with official Google tooling

2. **REST API** (Direct HTTP calls)
   - FileSearchStore operations (create, delete)
   - PDF upload to FileSearchStore
   - Query with FileSearch tool and strict grounding
   - Used until official SDK adds FileSearch support

### Components

1. **FileSearchManager.cs**
   - REST API integration for FileSearchStore operations
   - PDF upload with multipart form data
   - Async operation status polling
   - Error handling and cleanup

2. **Program.cs**
   - Main application flow and UI
   - Official Google.GenAI SDK client initialization
   - REST API calls for FileSearch queries
   - Strict grounding configuration (temperature=0, system instructions)
   - Citation extraction from grounding metadata

3. **ConsoleHelper.cs**
   - Beautiful colored console output
   - User input prompts
   - Citation and error display

### API Integration

**REST API Endpoints Used:**
- `POST /v1beta/fileSearchStores` - Create File Search store
- `POST /upload/v1beta/{storeName}:uploadToFileSearchStore` - Upload PDF
- `GET /v1beta/{operationName}` - Check operation status
- `POST /v1beta/models/gemini-2.0-flash:generateContent` - Query with FileSearch tool
- `DELETE /v1beta/{storeName}` - Delete store

**SDK Used:**
- `Google.GenAI` - Official Google Gen AI .NET SDK for client management

## ⚙️ Configuration

Edit `appsettings.json`:

```json
{
  "GeminiApi": {
    "ApiKey": "YOUR_API_KEY_HERE"
  },
  "FileSearchStore": {
    "StoreName": "my-rag-store"
  }
}
```

## 🐛 Troubleshooting

**"API Key not configured!"**
- Make sure you've replaced `YOUR_API_KEY_HERE` in `appsettings.json`

**"File not found"**
- Provide the full absolute path to your PDF
- Example: `C:\Users\YourName\Documents\file.pdf` (Windows)

**"Upload operation timed out"**
- Very large PDFs (100+ MB) may take longer to index
- Try a smaller document first to test

**"Failed to create File Search store"**
- Check your API key is valid
- Ensure you have internet connectivity
- Verify the Gemini API is accessible from your network

## 📝 Notes

- PDF files are indexed using semantic chunking for optimal retrieval
- The File Search store persists until you explicitly delete it
- You can upload multiple PDFs to the same store (modify code accordingly)
- Maximum recommended PDF size: 200 MB

## 🔗 Resources

- [Gemini API Documentation](https://ai.google.dev/gemini-api/docs)
- [File Search Guide](https://ai.google.dev/gemini-api/docs/file-search)
- [Google_GenerativeAI SDK](https://github.com/gunpal5/Google_GenerativeAI)

## 📄 License

This project is provided as-is for educational and development purposes.

---

**Built with ❤️ using .NET 9.0 and Google Gemini API**
