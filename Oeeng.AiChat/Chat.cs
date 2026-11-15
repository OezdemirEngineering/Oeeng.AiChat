using Oeeng.Ini.Serializer;
using Oeeng.Ini.Serializer.Converters;
using Oeeng.Ini.Serializer.Extensions;
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

    /// <summary>
    /// Creates a new chat instance and initializes the model context from configuration.
    /// </summary>
    /// <param name="configDirectory">Optional directory containing the configuration file.</param>
    /// <param name="cancellationToken">A token used to cancel initialization.</param>
    /// <returns>A fully initialized <see cref="Chat"/> instance.</returns>
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

                config = IniFile.Deserialize<Configuration>(iniFileStr, serializer =>
                    serializer.RegisterConverter(new BracketCollectionConverter()));
                configBasePath = configDirectory;
            }
        }

        if(File.Exists(Path.Combine(AppContext.BaseDirectory, nameof(Configuration) + ".ini")))
        {
            var iniFileStr = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, nameof(Configuration) + ".ini"), cancellationToken);
            config = IniFile.Deserialize<Configuration>(iniFileStr, serializer =>
                serializer.RegisterConverter(new BracketCollectionConverter()));
            configBasePath = AppContext.BaseDirectory;
        }
        else
        {
            var iniFileStr = IniFile.Serialize(config, serializer =>
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

    /// <summary>
    /// Streams a model response token-by-token for the provided user question.
    /// </summary>
    /// <param name="question">The user prompt to send to the model.</param>
    /// <param name="cancellationToken">A token used to cancel streaming.</param>
    /// <returns>An async stream of generated response tokens.</returns>
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

    /// <summary>
    /// Releases all native resources associated with the chat model context.
    /// </summary>
    public void Dispose()
    {
        _context.Dispose();
        _weights.Dispose();
    }
}