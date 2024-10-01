using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.ClientModel;

// Retrieve the key and endpoint from environment variables
string apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY");
string endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");

if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(endpoint))
{
	throw new Exception("API key or endpoint is missing. Please set the AZURE_OPENAI_KEY and AZURE_OPENAI_ENDPOINT environment variables.");
}

AzureOpenAIClient azureClient = new(
	new Uri(endpoint),
	new AzureKeyCredential(apiKey));

ChatClient chatClient = azureClient.GetChatClient("gpt-35-turbo");

ResultCollection<StreamingChatCompletionUpdate> completionUpdates = chatClient.CompleteChatStreaming(
	new List<ChatMessage>
	{
		new UserChatMessage("Generate the Powerpoint slides for a conference session on FinOps for AI on Azure"),
	});

foreach (StreamingChatCompletionUpdate completionUpdate in completionUpdates)
{
	foreach (ChatMessageContentPart contentPart in completionUpdate.ContentUpdate)
	{
		Console.Write(contentPart.Text);
	}
}