using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;

string keyFromEnvironment = "cc67ba39c6ad4938ac2b8c689dfc7977";

AzureOpenAIClient azureClient = new(
    new Uri("https://openai-dev-000.openai.azure.com/"),
    new AzureKeyCredential(keyFromEnvironment));
ChatClient chatClient = azureClient.GetChatClient("gpt-35-turbo");

ChatCompletion completion = chatClient.CompleteChat(
    [
        // System messages represent instructions or other guidance about how the assistant should behave
        new UserChatMessage("Generate the Powerpoint slides for a conference session on FinOps for AI on Azure"),
    ]);

Console.WriteLine($"{completion.Role}: {completion.Content[0].Text}");