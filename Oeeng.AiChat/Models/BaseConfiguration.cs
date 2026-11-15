using System;
using System.Collections.Generic;
using System.Text;

namespace Oeeng.AiChat.Models;

internal class BaseConfiguration
{
    public string ModelDirectory { get; set; } = @".\LLModels";

    public string ModelFilename { get; set; } = "Qwen3.5-0.8B-Q4_K_M.gguf";

    public string ModelUrl { get; set; } = "https://huggingface.co/unsloth/Qwen3.5-0.8B-GGUF/resolve/main/Qwen3.5-0.8B-Q4_K_M.gguf";

    public string SkillDirectory { get; set; } = @".\Skills";

}
