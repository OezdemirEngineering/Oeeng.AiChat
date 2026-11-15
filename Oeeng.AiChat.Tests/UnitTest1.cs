using Oeeng.AiChat.Helper;
using Oeeng.AiChat.Models;
using Shouldly;

namespace Oeeng.AiChat.Tests;

public class ChatTests
{
    [Fact]
    public async Task CreateAsync_GeneralQuestion_ShouldContainParis()
    {
        // Arrange
        var chat = await Chat.CreateAsync();

        // Act
        var response = string.Empty;
        await foreach (var token in chat.StreamAsync("What is the capital of France?"))
        {
            response += token;
        }

        // Assert
        response.ShouldContain("Paris", Case.Insensitive);
    }

    [Fact]
    public void ResolveModelDirectory_RelativePath_ShouldUseBasePath()
    {
        // Arrange
        const string basePath = @"D:\Temp\Config";
        const string configuredPath = @".\LLModels";

        // Act
        var resolvedPath = ModelInstaller.ResolveModelDirectory(configuredPath, basePath);

        // Assert
        resolvedPath.ShouldBe(Path.GetFullPath(Path.Combine(basePath, configuredPath)));
    }

    [Fact]
    public void ResolveModelDirectory_AbsolutePath_ShouldStayUnchanged()
    {
        // Arrange
        const string basePath = @"D:\Temp\Config";
        const string configuredPath = @"D:\Data\Models";

        // Act
        var resolvedPath = ModelInstaller.ResolveModelDirectory(configuredPath, basePath);

        // Assert
        resolvedPath.ShouldBe(configuredPath);
    }

    [Fact]
    public void BaseConfiguration_DefaultModel_ShouldBeSupportedTestModel()
    {
        // Arrange
        var configuration = new BaseConfiguration();

        // Act
        var modelFile = configuration.ModelFilename;

        // Assert
        modelFile.ShouldBe("Qwen3.5-0.8B-Q4_K_M.gguf");
    }
}
