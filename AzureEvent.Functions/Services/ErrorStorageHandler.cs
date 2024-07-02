using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AzureEvent.Function.Services
{
    public class ErrorStorageHandler : IErrorStorageHandler
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;
        public ErrorStorageHandler(IConfiguration configuration, ILogger<ErrorStorageHandler> logger, HttpClient httpClient)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
        }
        public async Task SendErrorToStorage(HttpRequest req, string domainName, string requestBody)
        {
            try
            {
                _logger.LogInformation("The Error is being stored in the Storage Account.");
                var clientId = _configuration["ManagedIdentityClientId"]; // Replace with your Managed Identity's Client Id
                var options = new DefaultAzureCredentialOptions { ManagedIdentityClientId = clientId };
                var credential = new DefaultAzureCredential(options);
                var tokenRequestContext = new TokenRequestContext(new[] { "https://storage.azure.com/.default" });
                AccessToken token = await credential.GetTokenAsync(tokenRequestContext);

                // Create a unique name for the blob
                string blobName = req.Headers["API-ID"] + ".txt";
                _logger.LogInformation($"Blob Name: {blobName}");
                _logger.LogInformation($"The Token is : {token.Token}");
                // Prepare the 
                req.Body.Position = 0;
                var request = new HttpRequestMessage(HttpMethod.Put, $"https://azureeventsa.blob.core.windows.net/apimlog/events/{domainName}/{blobName}")
                {
                    Content = new StringContent(requestBody)
                };

                // Add headers
                request.Headers.Add("x-ms-blob-type", "BlockBlob");
                request.Headers.Add("x-ms-version", "2019-07-07");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

                // Send the request
                var response = await _httpClient.SendAsync(request);

                // Log the response
                _logger.LogInformation($"Response: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to upload blob: {response.StatusCode}");
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"An error occurred: {ex.Message}");
            }
        }
    }
}