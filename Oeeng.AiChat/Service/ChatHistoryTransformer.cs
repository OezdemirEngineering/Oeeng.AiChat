using LLama.Abstractions;
using LLama.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Oeeng.AiChat.Service;

internal class ChatHistoryTransformer : IHistoryTransform
{
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

    public ChatHistory TextToHistory(AuthorRole role, string text)
    {
        var history = new ChatHistory();

        history.AddMessage(role, text);

        return history;
    }

    public IHistoryTransform Clone()
    {
        return new ChatHistoryTransformer();
    }
}
