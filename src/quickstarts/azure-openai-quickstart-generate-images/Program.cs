// Install the .NET library via NuGet: dotnet add package Azure.AI.OpenAI --prerelease
using Azure.AI.OpenAI;
using OpenAI.Images;
using System.ClientModel;
using static System.Environment;

string endpoint = GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
string key = GetEnvironmentVariable("AZURE_OPENAI_KEY");

ApiKeyCredential credential = new ApiKeyCredential(key);
AzureOpenAIClient azureClient = new AzureOpenAIClient(new Uri(endpoint), credential);
ImageClient client = azureClient.GetImageClient("dall-e-3");

ClientResult<GeneratedImage> imageResult = await client.GenerateImageAsync("An abstract painting of the seattle skyline featuring the space needle and mount rainier", new()
{
	Quality = GeneratedImageQuality.Standard,
	Size = GeneratedImageSize.W1024xH1024,
	ResponseFormat = GeneratedImageFormat.Uri
});

// Image Generations responses provide URLs you can use to retrieve requested images
GeneratedImage image = imageResult.Value;
Console.WriteLine($"Image URL: {image.ImageUri}");