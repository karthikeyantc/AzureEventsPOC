using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using Azure.Messaging.EventGrid;
using Azure;
using System.Linq;
using AzureEvent.Functions;
using Azure.Identity;
using AzureEvent.Function.Services;
using Microsoft.Extensions.Configuration;

namespace AzureEvent.Function
{
    public class events
    {
        public readonly IErrorStorageHandler _errorStorageHandler;
        public readonly IEventModelConverterFactory _eventModelConverterFactory;
        public readonly IConfiguration _configuration;
        public events(IErrorStorageHandler errorStorageHandler, IEventModelConverterFactory eventModelConverterFactory, IConfiguration configuration)
        {
            _errorStorageHandler = errorStorageHandler;
            _eventModelConverterFactory = eventModelConverterFactory;
            _configuration = configuration;
        }
        [FunctionName("events")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "events/{domainName}")] HttpRequest req, string domainName,
            ILogger log)
        {

            using var reader = new StreamReader(req.Body);
            var requestBody = await reader.ReadToEndAsync();
            try
            {
                log.LogInformation("The event mapper function is processing a request.");

                //Event Grid Domain client
                List<EventModel> eventslist = new List<EventModel>();
                var settings = new JsonSerializerSettings();
                var eventModelConverter = _eventModelConverterFactory.Create(domainName);
                settings.Converters.Add(eventModelConverter);

                using (var streamReader = new StringReader(requestBody))
                {
                    using (var jsonReader = new JsonTextReader(streamReader))
                    {
                        var serializer = JsonSerializer.Create(settings);
                        eventslist = serializer.Deserialize<List<EventModel>>(jsonReader);
                    }
                }

                string domainEndpoint = Environment.GetEnvironmentVariable($"DomainEndpoint{domainName.ToLower()}");
                var clientId = _configuration["ManagedIdentityClientId"]; // Replace with your Managed Identity's Client Id
                var options = new DefaultAzureCredentialOptions { ManagedIdentityClientId = clientId };
                var credential = new DefaultAzureCredential(options);
                EventGridPublisherClient client = new EventGridPublisherClient(new Uri(domainEndpoint), credential);

                string json = JsonConvert.SerializeObject(eventslist);
                // log.LogInformation($"Events: {json}");
                BinaryData requestBodyBinary = BinaryData.FromString(json);
                List<EventGridEvent> events = EventGridEvent.ParseMany(requestBodyBinary).ToList();
                Response result = await client.SendEventsAsync(events);
                // throw new Exception("An error occurred while publishing events, Please check the payload and try again.");
                // return the result's response content as a string
                if (result.Status == 200)
                    return new OkObjectResult("Events published successfully");
                else
                {
                    throw new Exception("An error occurred while publishing events, Please check the payload and try again.");
                }
            }
            catch (Exception ex)
            {
                // await SendErrorToStorage(req, domainName, log, requestBody);
                await _errorStorageHandler.SendErrorToStorage(req, domainName, requestBody);
                log.LogError($"An error occurred: {ex.Message}");
                return new BadRequestObjectResult("An error occurred with message: " + ex.Message);
            }
        }
        // private static async Task SendErrorToStorage(HttpRequest req, string domainName, ILogger log, string requestBody)
        // {
        //     try
        //     {
        //         log.LogInformation("The Error is being stored in the Storage Account.");
        //         var clientId = "145a79a6-042d-4be2-9f56-9caf813c3a53"; // Replace with your Managed Identity's Client Id
        //         var options = new DefaultAzureCredentialOptions { ManagedIdentityClientId = clientId };
        //         var credential = new DefaultAzureCredential(options);
        //         var tokenRequestContext = new TokenRequestContext(new[] { "https://storage.azure.com/.default" });
        //         AccessToken token = await credential.GetTokenAsync(tokenRequestContext);

        //         // Create a unique name for the blob
        //         string blobName = req.Headers["API-ID"] + ".txt";
        //         log.LogInformation($"Blob Name: {blobName}");
        //         log.LogInformation($"The Token is : {token.Token}");
        //         // Prepare the 
        //         req.Body.Position = 0;
        //         var request = new HttpRequestMessage(HttpMethod.Put, $"https://azureeventsa.blob.core.windows.net/apimlog/events/{domainName}/{blobName}")
        //         {
        //             Content = new StringContent(requestBody)
        //         };

        //         // Add headers
        //         request.Headers.Add("x-ms-blob-type", "BlockBlob");
        //         request.Headers.Add("x-ms-version", "2019-07-07");
        //         request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

        //         // Send the request
        //         var httpClient = new HttpClient();
        //         var response = await httpClient.SendAsync(request);

        //         // Log the response
        //         log.LogInformation($"Response: {response.StatusCode}");

        //         if (!response.IsSuccessStatusCode)
        //         {
        //             log.LogError($"Failed to upload blob: {response.StatusCode}");
        //         }
        //     }
        //     catch (System.Exception ex)
        //     {
        //         log.LogError($"An error occurred: {ex.Message}");
        //     }
        // }
    }

    // public class EventModelConverter : JsonConverter<EventModel>
    // {
    //     private readonly string _domainName;
    //     public EventModelConverter(string domainName)
    //     {
    //         _domainName = domainName;
    //     }
    //     public override EventModel ReadJson(JsonReader reader, Type objectType, EventModel existingValue, bool hasExistingValue, JsonSerializer serializer)
    //     {
    //         var eventModel = new EventModel();
    //         serializer.Populate(reader, eventModel);
    //         if (string.IsNullOrWhiteSpace(eventModel.Topic)) throw new JsonSerializationException("Topic is required");
    //         // Ensure default values
    //         if (string.IsNullOrWhiteSpace(eventModel.Id)) eventModel.Id = Guid.NewGuid().ToString();
    //         if (string.IsNullOrWhiteSpace(eventModel.Subject)) eventModel.Subject = "EventGridModel";
    //         if (string.IsNullOrWhiteSpace(eventModel.EventType)) eventModel.EventType = "Azure.Sdk.Sample " + _domainName;
    //         if (eventModel.EventTime == default) eventModel.EventTime = DateTimeOffset.UtcNow;
    //         if (string.IsNullOrWhiteSpace(eventModel.DataVersion)) eventModel.DataVersion = "1.0";

    //         return eventModel;
    //     }

    //     public override void WriteJson(JsonWriter writer, EventModel value, JsonSerializer serializer)
    //     {
    //         serializer.Serialize(writer, value);
    //     }
    // }
}
