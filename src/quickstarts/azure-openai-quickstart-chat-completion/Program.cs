using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;

// Fetch the API key and endpoint from environment variables
string azureApiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY");
string azureApiEndpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");

if (string.IsNullOrEmpty(azureApiKey) || string.IsNullOrEmpty(azureApiEndpoint))
{
	Console.WriteLine("Please make sure AZURE_OPENAI_KEY and AZURE_OPENAI_ENDPOINT environment variables are set.");
	return;
}

// Initialize AzureOpenAIClient with credentials from environment variables
AzureOpenAIClient azureClient = new AzureOpenAIClient(new Uri(azureApiEndpoint), new AzureKeyCredential(azureApiKey));
ChatClient chatClient = azureClient.GetChatClient("gpt-35-turbo");

// Creating a user prompt message
ChatCompletion completion = chatClient.CompleteChat(
	new ChatMessage[]
	{
		new UserChatMessage("Generate the Powerpoint slides for a conference session on FinOps for AI on Azure"),
	});

// Output the completion result
Console.WriteLine($"{completion.Role}: {completion.Content[0].Text}");