Oeeng.AiChat

Local LLMs. Local Agents. Your Data, Your Control.

Wouldn’t it be great to have your own AI agents running locally on your machine, without constantly relying on cloud services or exposing your data to external providers?

That’s the idea behind Oeeng.AiChat.

This is a small open-source project where I’m exploring how to build an independent local AI orchestration platform using .NET.

The goal is to go beyond traditional chatbot interactions and eventually enable local LLMs to act as autonomous agents — executing tasks, interacting with applications, and collaborating within a controlled environment.

Rather than depending entirely on external AI services, the idea is to keep as much processing as possible on your own hardware.

Oeeng.AiChat is the first building block of that vision: a lightweight .NET 10 chat library built on top of LLamaSharp, allowing applications to interact with local GGUF models through a simple API.

The project is still in its early stages, with more capabilities planned as the architecture evolves.

Installation

dotnet add package Oeeng.AiChat

Usage

using Oeeng.AiChat;
using var chat = await Chat.CreateAsync();
await foreach (var token in chat.StreamAsync("What is the capital of France?"))
{
    Console.Write(token);
}

Configuration

Configuration.ini is loaded from:

1. The optional configDirectory passed to Chat.CreateAsync(...)
2. AppContext.BaseDirectory (takes precedence when present)

Relative paths such as ./LLModels or .\\LLModels are resolved relative to the directory of the active Configuration.ini file.

Tested Models

The library has been tested with the following models:

Qwen3.5 – 0.8B

curl -L -C - -o Qwen3.5-0.8B-Q4_K_M.gguf https://huggingface.co/unsloth/Qwen3.5-0.8B-GGUF/resolve/main/Qwen3.5-0.8B-Q4_K_M.gguf

Qwen3.5 – 4B

curl -L -C - -o Qwen3.5-4B-Q4_K_M.gguf https://huggingface.co/unsloth/Qwen3.5-4B-GGUF/resolve/main/Qwen3.5-4B-Q4_K_M.gguf

Qwen3.5 – 9B

curl -L -C - -o Qwen3.5-9B-Q4_K_M.gguf https://huggingface.co/unsloth/Qwen3.5-9B-GGUF/resolve/main/Qwen3.5-9B-Q4_K_M.gguf

Project Vision

The long-term vision is to explore a modular architecture where local LLMs can become more than conversational interfaces.

Potential directions include:

* Local AI Agents – Execute tasks and interact with applications.
* Agent Orchestration – Coordinate multiple specialized agents.
* Hybrid AI Execution – Use local models by default and external models only when necessary.
* Privacy & Control – Keep sensitive information and processing within the local environment whenever possible.

The focus is on experimenting with practical AI integration in .NET applications while keeping the architecture lightweight and extensible.

⸻

Developed by OEENG – Oezdemir Engineering