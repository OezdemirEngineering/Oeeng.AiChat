using LLama.Abstractions;
using LLama.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Oeeng.AiChat.Service;

internal class ChatHistoryTransformer : IHistoryTransform
{
    /// <summary>
    /// Converts chat history into the model-specific prompt format.
    /// </summary>
    /// <param name="history">The chat history to transform.</param>
    /// <returns>A formatted prompt string.</returns>
    public string HistoryToText(ChatHistory history)
    {
        var builder = new StringBuilder();

        foreach (var message in history.Messages)
        {
            string role = message.AuthorRole switch
            {
                AuthorRole.System => "system",
                AuthorRole.User => "user",
                AuthorRole.Assistant => "assistant",
                _ => "user"
            };

            builder.Append("<|im_start|>");
            builder.Append(role);
            builder.Append('\n');
            builder.Append(message.Content);
            builder.Append("<|im_end|>\n");
        }

        builder.Append("<|im_start|>assistant\n");

        return builder.ToString();
    }

    /// <summary>
    /// Converts generated text into a chat history object.
    /// </summary>
    /// <param name="role">The role associated with the generated text.</param>
    /// <param name="text">The generated text.</param>
    /// <returns>A chat history containing the generated message.</returns>
    public ChatHistory TextToHistory(AuthorRole role, string text)
    {
        var history = new ChatHistory();

        history.AddMessage(role, text);

        return history;
    }

    /// <summary>
    /// Creates a new instance of the same history transformer.
    /// </summary>
    /// <returns>A cloned history transformer.</returns>
    public IHistoryTransform Clone()
    {
        return new ChatHistoryTransformer();
    }
}
