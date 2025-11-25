# Gemini RAG - Full Stack PDF Question Answering 🤖

A modern, full-stack **Retrieval Augmented Generation (RAG)** application built with **.NET 9** and **Angular 18**. It leverages Google's **Gemini 1.5 Flash** model to provide strictly grounded answers from your own PDF documents.

## 📜 Table of Contents
- [🏗️ Architecture](#️-architecture)
- [✨ Features](#-features)
- [🚀 Prerequisites](#-prerequisites)
- [📦 Installation & Setup](#-installation--setup)
- [🎮 Usage](#-usage)
- [🔒 Strict Grounding Explained](#-strict-grounding-explained)
- [🛠️ Technical Details](#️-technical-details)
- [🐛 Troubleshooting](#-troubleshooting)

## 🏗️ Architecture

The solution is engineered into a scalable **Clean Architecture** to support enterprise-grade development:

- **`GeminiRAG.Core`** 🧠
  - The domain layer containing pure business logic, interfaces, and models.
  - Zero external dependencies.

- **`GeminiRAG.Infrastructure`** 🔌
  - Implementation layer for external services.
  - Handles Gemini API communication.

- **`GeminiRAG.Api`** 🌐
  - ASP.NET Core Web API serving as the backend.
  - Exposes endpoints for Stores, Documents, Queries, and History.

- **`GeminiRAG.Web`** 🎨
  - Modern Single Page Application (SPA) built with **Angular 18**.
  - Styled with **Angular Material** and **TailwindCSS**.

## ✨ Features

### 🌐 Web Interface
- **Dashboard**: Manage multiple Document Stores (create, delete, switch) dynamically.
- **Document Management**: Upload and list PDF documents with ease.
- **Interactive Chat**: Query your documents with a modern, responsive chat interface.
- **History**: View past queries, answers, and citations.
- **Real-time Feedback**: Toast notifications for actions and errors.

### 🤖 Core Capabilities
- **Strict Grounding**: Answers are sourced *exclusively* from your uploaded PDFs.
- **Zero Hallucination**: Configured to refuse answering if info isn't in the docs.
- **Citation Support**: See exactly which part of the PDF was used for the answer.
- **Dynamic Stores**: Create isolated stores for different projects or topics.

## 🚀 Prerequisites

1. **.NET 9.0 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/9.0)
2. **Node.js (v18+)** - [Download here](https://nodejs.org/)
3. **Gemini API Key** - [Get one here](https://aistudio.google.com/apikey)

## 📦 Installation & Setup

### 1. Configure API Key
Edit `src/GeminiRAG.Api/appsettings.json` and add your key:
```json
{
  "GeminiApi": {
    "ApiKey": "YOUR_API_KEY_HERE"
  }
}
```

### 2. Start the Backend (API)
The API handles all AI operations.
```bash
dotnet run --project src/GeminiRAG.Api/GeminiRAG.Api.csproj
```
*Runs on: `http://localhost:5109`*  
*Swagger UI: `http://localhost:5109/swagger`*

### 3. Start the Frontend (Web)
Open a new terminal window:
```bash
cd src/GeminiRAG.Web
npm install  # First time only
npx ng serve
```
*Runs on: `http://localhost:4200`*

## 🎮 Usage

1. Navigate to `http://localhost:4200`.
2. **Select Store**: Choose an existing store or create a new one (e.g., "Project Alpha").
3. **Upload**: Drag & drop or select PDF files to index them.
4. **Chat**: Type your question. The AI will analyze *only* the documents in the selected store.
5. **Review**: Check the "History" tab for previous interactions.

## 🔒 Strict Grounding Explained

This application is configured to **prevent hallucination**:

- ✅ **Only uses PDF content**: Answers are sourced exclusively from your uploaded document.
- ❌ **No general knowledge**: Won't answer questions outside your PDF's scope (e.g., "What is the capital of France?" will be rejected if not in the PDF).
- 🎯 **Factual accuracy**: Temperature set to `0.0` for deterministic responses.

## 🛠️ Technical Details

### Tech Stack
- **Backend**: C# 12, ASP.NET Core 9.0, Swagger/OpenAPI
- **Frontend**: Angular 18, TypeScript, RxJS, Angular Material, TailwindCSS
- **AI**: Google Gemini 2.5 Flash

## 🐛 Troubleshooting

- **"Connection Refused" in Web UI**: Ensure the API is running on port `5109`.
- **"Quota Exceeded"**: Check your Google AI Studio quota limits.

---
**Built with ❤️ using .NET 9.0 and Google Gemini API**
