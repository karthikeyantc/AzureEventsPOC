// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Azure.Messaging.EventGrid;

namespace AzureEvent.Function
{
    public static class BSystemConsumer
    {
        [FunctionName("BSystemConsumer")]
        public static void Run([EventGridTrigger] EventGridEvent eventGridEvent, ILogger log)
        {
            log.LogInformation(eventGridEvent.Data.ToString());
            // log all the events that are consumed by the function app with the Id, Topic, and the event type.
            log.LogInformation($"Event Id: {eventGridEvent.Id}");
            log.LogInformation($"Event Topic: {eventGridEvent.Topic}");
            log.LogInformation($"Event Type: {eventGridEvent.EventType}");

        }
    }
}
