namespace Oeeng.AiChat.Tests;

public class ChatTests
{
    [Fact]
    public async Task CreateAsync_GeneralQuestion()
    {
        var chat = await Chat.CreateAsync();

        var response = string.Empty;
        await foreach (var token in chat.StreamAsync("What is the capital of France?"))
        {
            response += token;
        }

        Assert.Contains("Paris", response, StringComparison.OrdinalIgnoreCase);


    }
}
