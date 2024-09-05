using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Azure.Search.Documents.Indexes.Models;
using System.Threading;

class Program
{
    private static readonly string searchServiceName = "search-25am53ujh3y74";
    private static readonly string searchApiKey = "JPGih0cbsxcMrvdsVIHTUePamj2Ri8CfoEy6LyZLDIAzSeDnoFBU";
    private static readonly string indexName = "cosmosdb-index-001";
    private static readonly string openAiEndpoint = "https://openai-dev-000.openai.azure.com";
    private static readonly string openAiApiKey = "cc67ba39c6ad4938ac2b8c689dfc7977";
    private static readonly string openAiDeploymentName = "text-embedding-3-small";

    static async Task Main(string[] args)
    {
        var endpoint = $"https://{searchServiceName}.search.windows.net";
        var searchClient = new SearchClient(new Uri(endpoint), indexName, new AzureKeyCredential(searchApiKey));
        int batchSize = 100; // Adjust batch size if needed
        int skip = 0;
        bool hasMoreDocuments = true;

        while (hasMoreDocuments)
        {
            var searchResults = await searchClient.SearchAsync<SearchDocument>("*", new SearchOptions
            {
                Size = batchSize,
                Skip = skip
            });

            var documentsToUpdate = new List<SearchDocument>();

            await foreach (var result in searchResults.Value.GetResultsAsync())
            {
                try
                {
                    var documentId = result.Document["id"].ToString();
                    var biography = GetBiography(result.Document);

                    if (biography != null)
                    {
                        var embedding = await GenerateEmbedding(biography);
                        result.Document["content_vector"] = embedding.Select(f => f.ToString()).ToArray();
                        documentsToUpdate.Add(result.Document);

                        // Process documents in parallel
                        if (documentsToUpdate.Count >= batchSize)
                        {
                            await UpdateDocumentsInIndex(searchClient, documentsToUpdate);
                            documentsToUpdate.Clear();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing document: {ex.Message}");
                }
            }

            if (documentsToUpdate.Count > 0)
            {
                await UpdateDocumentsInIndex(searchClient, documentsToUpdate);
            }

            skip += batchSize;
            hasMoreDocuments = searchResults.Value.TotalCount > skip;
        }

        Console.WriteLine("All documents processed.");
    }

    static string GetBiography(SearchDocument document)
    {
        if (document.TryGetValue("payload", out var payloadValue) && payloadValue is SearchDocument payload)
        {
            if (payload.TryGetValue("data", out var dataValue) && dataValue is SearchDocument data)
            {
                if (data.TryGetValue("user", out var userValue) && userValue is SearchDocument user)
                {
                    if (user.TryGetValue("biography", out var biographyValue) && biographyValue is string biography)
                    {
                        return biography;
                    }
                }
            }
        }

        return null;
    }

    static async Task<float[]> GenerateEmbedding(string text)
    {
        using (var httpClient = new HttpClient())
        {
            // Add API Key to the headers
            httpClient.DefaultRequestHeaders.Add("api-key", openAiApiKey);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Define the request body
            var requestBody = new
            {
                input = text
            };

            // Create JSON content
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            // Construct the API URL
            string apiUrl = $"{openAiEndpoint}/openai/deployments/{openAiDeploymentName}/embeddings?api-version=2023-05-15";

            // Send the request to the Azure OpenAI API
            var response = await httpClient.PostAsync(apiUrl, content);

            // Ensure the request was successful
            response.EnsureSuccessStatusCode();

            // Read and parse the response
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var embeddingResponse = JsonSerializer.Deserialize<Rootobject>(jsonResponse);

            // Return the embedding array
            return embeddingResponse.data[0].embedding;
        }
    }

    static async Task UpdateDocumentsInIndex(SearchClient searchClient, List<SearchDocument> documents)
    {
        var batch = IndexDocumentsBatch.MergeOrUpload(documents);
        var options = new IndexDocumentsOptions { ThrowOnAnyError = true };

        try
        {
            await searchClient.IndexDocumentsAsync(batch, options);
            Console.WriteLine($"Updated {documents.Count} documents.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating documents in index: {ex.Message}");
        }
    }

    public class Rootobject
    {
        public string _object { get; set; }
        public Datum[] data { get; set; }
        public string model { get; set; }
        public Usage usage { get; set; }
    }

    public class Usage
    {
        public int prompt_tokens { get; set; }
        public int total_tokens { get; set; }
    }

    public class Datum
    {
        public string _object { get; set; }
        public int index { get; set; }
        public float[] embedding { get; set; }
    }
}
