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

namespace AzureEvent.Function
{
    public static class events
    {
        [FunctionName("events")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "events/{domainName}")] HttpRequest req, string domainName,
            ILogger log)
        {
            try
            {
                log.LogInformation("The event mapper function is processing a request.");
                // string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                // dynamic data = JsonConvert.DeserializeObject(requestBody);
                //Event Grid Domain client
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                List<EventModel> eventslist = JsonConvert.DeserializeObject<List<EventModel>>(requestBody);
                string domainEndpoint = Environment.GetEnvironmentVariable($"DomainEndpoint{domainName.ToLower()}");
                string domainKey = Environment.GetEnvironmentVariable($"DomainKey{domainName.ToLower()}");
                EventGridPublisherClient client = new EventGridPublisherClient(new Uri(domainEndpoint), new AzureKeyCredential(domainKey));
                BinaryData requestBodyBinary = BinaryData.FromString(JsonConvert.SerializeObject(eventslist));
                List<EventGridEvent> events = EventGridEvent.ParseMany(requestBodyBinary).ToList();
                Response result = await client.SendEventsAsync(events);
                // return the result's response content as a string
                if (result.Status == 200)
                    return new OkObjectResult("Events published successfully");
                else
                    return new BadRequestObjectResult(result.ContentStream);
            }
            catch (Exception ex)
            {
                log.LogError($"An error occurred: {ex.Message}");
                return new BadRequestObjectResult("An error occurred");
            }
        }
    }
}
