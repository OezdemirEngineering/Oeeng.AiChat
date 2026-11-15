using System;
using System.Collections.Generic;
using System.Text;

namespace Oeeng.AiChat.Models;

internal class ModelParamsConfiguration
{
    public uint ContextSize { get; set; } = 8192;

    public int GpuLayerCount { get; set; } = 0;

    public int Threads { get; set; } = 8;

    public uint BatchSize { get; set; } = 512;
}
