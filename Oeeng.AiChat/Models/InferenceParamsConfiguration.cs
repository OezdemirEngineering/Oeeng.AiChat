using System;
using System.Collections.Generic;
using System.Text;

namespace Oeeng.AiChat.Models;

internal class InferenceParamsConfiguration
{
    public int MaxTokens { get; set; } = 2048;

    public List<string> AntiPrompts { get; set; } =
            [
                "<|im_end|>",
                "<|im_start|>"
            ];
}
