using Oeeng.AiChat.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Oeeng.AiChat.Helper;


internal static class ModelInstaller
{

    public static async Task<string> EnsureModelAsync(
        BaseConfiguration baseConfiguration,
        string? basePath = null,
        CancellationToken cancellationToken = default)
    {
        string rootPath = string.IsNullOrWhiteSpace(basePath)
            ? AppContext.BaseDirectory
            : basePath;

        string configuredDirectory = baseConfiguration.ModelDirectory;

        string directory = string.IsNullOrWhiteSpace(configuredDirectory)
            ? Path.Combine(rootPath, "LLModels")
            : Path.IsPathRooted(configuredDirectory)
                ? configuredDirectory
                : Path.GetFullPath(Path.Combine(rootPath, configuredDirectory));

        Directory.CreateDirectory(directory);

        string destination = Path.Combine(directory, baseConfiguration.ModelFilename);

        // Modell bereits vorhanden
        if (File.Exists(destination) &&
            new FileInfo(destination).Length > 10_000_000)
        {
            Console.WriteLine($"Model already exists: {destination}");
            return destination;
        }

        string temporary = destination + ".download";

        using var client = new HttpClient(
            new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.All
            })
        {
            Timeout = TimeSpan.FromHours(4)
        };

        using var response = await client.GetAsync(
            baseConfiguration.ModelUrl,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        Console.WriteLine($"Downloading {baseConfiguration.ModelFilename} model...");

        try
        {
            await using (var input =
                await response.Content.ReadAsStreamAsync(cancellationToken))
            await using (var output = new FileStream(
                temporary,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                1024 * 1024,
                true))
            {
                byte[] buffer = new byte[1024 * 1024];

                long bytes = 0;
                int count;

                while ((count = await input.ReadAsync(
                    buffer,
                    cancellationToken)) != 0)
                {
                    await output.WriteAsync(
                        buffer.AsMemory(0, count),
                        cancellationToken);

                    bytes += count;

                    if (bytes % (20L * 1024 * 1024) < count)
                    {
                        Console.Write(
                            $"\rDownloaded {bytes / (1024 * 1024)} MiB...");
                    }
                }
            }

            if (new FileInfo(temporary).Length < 10_000_000)
            {
                throw new InvalidDataException(
                    "Model download was unexpectedly small.");
            }

            File.Move(temporary, destination, true);

            Console.WriteLine("\nModel ready.");
        }
        catch
        {
            if (File.Exists(temporary))
                File.Delete(temporary);

            throw;
        }

        return destination;
    }
}
