using Oeeng.AiChat.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Oeeng.AiChat.Configurations;

internal class Configuration
{
    public BaseConfiguration Base { get; set; } = new();
    public ModelParamsConfiguration ModelParams { get; set; } = new();
    public InferenceParamsConfiguration InferenceParams { get; set; } = new();
}
