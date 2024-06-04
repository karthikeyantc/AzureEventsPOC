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

                //Event Grid Domain client
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                List<EventModel> eventslist = new List<EventModel>();
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new EventModelConverter());
                if (requestBody.TrimStart().StartsWith("["))
                {
                    eventslist = JsonConvert.DeserializeObject<List<EventModel>>(requestBody, settings);
                    // Process the list
                }
                else
                {
                    eventslist = new List<EventModel> { JsonConvert.DeserializeObject<EventModel>(requestBody, settings) };
                    // Process the single object
                }
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
                return new BadRequestObjectResult("An error occurred with message: " + ex.Message);
            }
        }
    }
    public class EventModelConverter : JsonConverter<EventModel>
    {
        public override EventModel ReadJson(JsonReader reader, Type objectType, EventModel existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var eventModel = new EventModel();
            serializer.Populate(reader, eventModel);

            // Ensure default values
            if (string.IsNullOrWhiteSpace(eventModel.id)) eventModel.id = Guid.NewGuid().ToString();
            if (string.IsNullOrWhiteSpace(eventModel.subject)) eventModel.subject = "EventGridModel";
            if (string.IsNullOrWhiteSpace(eventModel.eventType)) eventModel.eventType = "Azure.Sdk.Sample";
            if (eventModel.eventTime == default) eventModel.eventTime = DateTimeOffset.UtcNow;
            if (string.IsNullOrWhiteSpace(eventModel.dataVersion)) eventModel.dataVersion = "1.0";

            return eventModel;
        }

        public override void WriteJson(JsonWriter writer, EventModel value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
