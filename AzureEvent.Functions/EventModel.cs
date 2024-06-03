using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AzureEvent.Functions
{
    public class EventModel
    {
        public string topic { get; set; }
        public string id { get; set; } = Guid.NewGuid().ToString();
        public string subject { get; set; } = "EventGridModel";
        public string eventType { get; set; } = "Azure.Sdk.Sample";
        public JToken data { get; set; }
        public string dataVersion { get; set; } = "1.0";
        public DateTimeOffset eventTime { get; set; } = DateTimeOffset.UtcNow;

    }
}