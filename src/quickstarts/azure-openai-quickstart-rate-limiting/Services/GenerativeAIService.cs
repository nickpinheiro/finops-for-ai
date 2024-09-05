using Azure;
using Azure.AI.OpenAI;
using Microsoft.AspNetCore.RateLimiting;
using OpenAI.Chat;

namespace azure_openai_quickstart_rate_limiting.Services
{
    public class GenerativeAIService
	{
		private readonly AzureOpenAIClient _azureClient;
		private readonly ChatClient _chatClient;

		public GenerativeAIService(string azureOpenAIKey, Uri endpoint)
		{
			AzureKeyCredential credential = new AzureKeyCredential(azureOpenAIKey);
			_azureClient = new AzureOpenAIClient(endpoint, credential);
			_chatClient = _azureClient.GetChatClient("gpt-35-turbo");
		}

		public async Task<string> GenerateBioAsync(string userName)
		{
			string prompt = $"Generate a compelling Instagram profile bio for a user, using no more than 150 characters. Do not include the username or use quotes in the bio. Here are the details to use: Name: {userName}.";
			//$"Interests: {userBioData.Interests}, " +
			//$"Profession: {userBioData.Profession}, " +
			//$"Hobbies: {userBioData.Hobbies}.";

			ChatCompletion completion = await _chatClient.CompleteChatAsync(
				new ChatMessage[]
				{
					new UserChatMessage(prompt),
				});

			// Remove quotes if they are present
			string generatedBio = completion.Content[0].Text.Trim('\"');

			return generatedBio;
		}
	}

	public class UserBioData
	{
		public string UserName { get; set; }
		public string Interests { get; set; }
		public string Profession { get; set; }
		public string Hobbies { get; set; }
	}
}