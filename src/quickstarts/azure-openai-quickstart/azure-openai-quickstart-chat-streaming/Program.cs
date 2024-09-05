using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.ClientModel;

string keyFromEnvironment = "cc67ba39c6ad4938ac2b8c689dfc7977";

AzureOpenAIClient azureClient = new(
    new Uri("https://openai-dev-000.openai.azure.com/"),
    new AzureKeyCredential(keyFromEnvironment));
ChatClient chatClient = azureClient.GetChatClient("gpt-35-turbo");

ResultCollection<StreamingChatCompletionUpdate> completionUpdates = chatClient.CompleteChatStreaming(
    [
        new UserChatMessage("Generate the Powerpoint slides for a conference session on FinOps for AI on Azure"),
    ]);

foreach (StreamingChatCompletionUpdate completionUpdate in completionUpdates)
{
    foreach (ChatMessageContentPart contentPart in completionUpdate.ContentUpdate)
    {
        Console.Write(contentPart.Text);
    }
}