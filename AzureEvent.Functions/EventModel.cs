using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureEvent.Functions
{
    public class EventModel
    {
        public string Topic { get; set; }
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Subject { get; set; } = "EventGridModel";
        public string EventType { get; set; } = "Azure.Sdk.Sample";
        public object Data { get; set; } = new object();
        public string DataVersion { get; set; } = "1.0";
        public DateTimeOffset EventTime { get; set; } = DateTimeOffset.UtcNow;

    }
}