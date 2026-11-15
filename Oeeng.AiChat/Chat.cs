using Ini.Serializer;
using Ini.Serializer.Converters;
using Ini.Serializer.Extensions;
using Ini.Serializer.Services;
using LLama;
using LLama.Common;
using Oeeng.AiChat.Configurations;
using Oeeng.AiChat.Helper;
using Oeeng.AiChat.Service;
using System.Runtime.CompilerServices;

namespace Oeeng.AiChat;

public class Chat : IDisposable
{
    private readonly LLamaWeights _weights;
    private readonly LLamaContext _context;
    private readonly ChatSession _session;
    private readonly InferenceParams _inference;

    private Chat(
        LLamaWeights weights,
        LLamaContext context,
        ChatSession session, Configuration config)
    {
        _weights = weights;
        _context = context;
        _session = session;

        _inference = new InferenceParams
        {
            MaxTokens = config.InferenceParams.MaxTokens,
            AntiPrompts = config.InferenceParams.AntiPrompts
        };
    }

    public static async Task<Chat> CreateAsync(
        string? configDirectory = null,
        CancellationToken cancellationToken = default)
    {
        Configuration config = new();
        string configBasePath = AppContext.BaseDirectory;

        if (configDirectory != null)
        {
            var fileName = nameof(Configuration) + ".ini";
            var filePath = Path.Combine(configDirectory, fileName);
            if (File.Exists(filePath))
            {
                var iniFileStr = await File.ReadAllTextAsync(filePath, cancellationToken);

                config = Ini.Serializer.Ini.Deserialize<Configuration>(iniFileStr, serializer =>
                    serializer.RegisterConverter(new BracketCollectionConverter()));
                configBasePath = configDirectory;
            }
        }

        if(File.Exists(Path.Combine(AppContext.BaseDirectory, nameof(Configuration) + ".ini")))
        {
            var iniFileStr = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, nameof(Configuration) + ".ini"), cancellationToken);
            config = Ini.Serializer.Ini.Deserialize<Configuration>(iniFileStr, serializer =>
                serializer.RegisterConverter(new BracketCollectionConverter()));
            configBasePath = AppContext.BaseDirectory;
        }
        else
        {
            var iniFileStr = Ini.Serializer.Ini.Serialize(config, serializer =>
                serializer.RegisterConverter(new BracketCollectionConverter()));
            File.WriteAllText(Path.Combine(AppContext.BaseDirectory, nameof(Configuration) + ".ini"), iniFileStr);
        }


        string path = await ModelInstaller.EnsureModelAsync(
            config.Base,
            configBasePath,
            cancellationToken);

        var parameters = new ModelParams(path)
        {
            ContextSize = config.ModelParams.ContextSize,
            GpuLayerCount = config.ModelParams.GpuLayerCount,
            Threads = config.ModelParams.Threads,
            BatchSize = config.ModelParams.BatchSize
        };

        var weights = LLamaWeights.LoadFromFile(parameters);

        LLamaContext? context = null;

        try
        {
            context = weights.CreateContext(parameters);

            var executor = new InteractiveExecutor(context);

            var session = new ChatSession(executor)
                .WithHistoryTransform(
                    new ChatHistoryTransformer());

            var skill = string.Empty;
            if(File.Exists(config.Base.SkillDirectory))
            {
                skill = await File.ReadAllTextAsync(config.Base.SkillDirectory, cancellationToken);
            }

            if(skill == string.Empty && File.Exists(Path.Combine(AppContext.BaseDirectory, "default.skill")))
            {
                skill = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "default.skill"), cancellationToken);
            }




            session.History.AddMessage(
                AuthorRole.System,
                skill
            );

            return new Chat(
                weights,
                context,
                session,
                config);
        }
        catch
        {
            context?.Dispose();
            weights.Dispose();
            throw;
        }
    }

    public async IAsyncEnumerable<string> StreamAsync(
        string question,
        [EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            throw new ArgumentException(
                "Question must not be empty.",
                nameof(question));
        }

        var message = new ChatHistory.Message(
            AuthorRole.User,
            question);

        await foreach (var token in _session.ChatAsync(
            message,
            _inference,
            cancellationToken))
        {
            yield return token;
        }
    }

    public void Dispose()
    {
        _context.Dispose();
        _weights.Dispose();
    }
}