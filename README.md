# Oeeng.AiChat

A lightweight .NET 10 chat library on top of LLamaSharp for local GGUF models.

## Installation

```bash
dotnet add package Oeeng.AiChat
```

## Usage

```csharp
using Oeeng.AiChat;

using var chat = await Chat.CreateAsync();

await foreach (var token in chat.StreamAsync("What is the capital of France?"))
{
    Console.Write(token);
}
```

### Configuration

`Configuration.ini` is loaded from:
1. the optional `configDirectory` passed to `Chat.CreateAsync(...)`
2. `AppContext.BaseDirectory` (takes precedence when present)

Relative paths such as `./LLModels` or `.\\LLModels` are resolved relative to the directory of the active `Configuration.ini` file.

## Tested Models

The library has been tested with the following models:

```bash
curl -L -C - -o Qwen3.5-0.8B-Q4_K_M.gguf https://huggingface.co/unsloth/Qwen3.5-0.8B-GGUF/resolve/main/Qwen3.5-0.8B-Q4_K_M.gguf

curl -L -C - -o Qwen3.5-4B-Q4_K_M.gguf https://huggingface.co/unsloth/Qwen3.5-4B-GGUF/resolve/main/Qwen3.5-4B-Q4_K_M.gguf

curl -L -C - -o Qwen3.5-9B-Q4_K_M.gguf https://huggingface.co/unsloth/Qwen3.5-9B-GGUF/resolve/main/Qwen3.5-9B-Q4_K_M.gguf
```