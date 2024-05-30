using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.EventGrid;
using System.Collections.Generic;

namespace AzureEvent.Function
{
    public static class APIMTrigger
    {
        [FunctionName("APIMTrigger")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";
            string topicEndpoint = Environment.GetEnvironmentVariable($"TopicEndpoint{data?.topic}");
            string topicKey = Environment.GetEnvironmentVariable($"TopicKey{data?.topic}");

            TopicCredentials topicCredentials = new TopicCredentials(topicKey);
            EventGridClient client = new EventGridClient(topicCredentials);

            List<EventGridEvent> events = new()
            {
                new() {
                    Id = Guid.NewGuid().ToString(),
                    EventType = "AzureEvent.APIMTrigger.EventPublished", // replace with your event type
                    Data = name,
                    EventTime = DateTime.Now,
                    Subject = "New event from "+ name, // replace with your subject
                    DataVersion = "1.0"
                }
            };

            await client.PublishEventsAsync(new Uri(topicEndpoint).Host, events);

            return new OkObjectResult(responseMessage);
        }
    }
}
