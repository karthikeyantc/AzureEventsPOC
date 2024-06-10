using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace AzureEvent.Functions
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class EventModel
    {
        [JsonProperty(Required = Required.Always)]
        public string Topic { get; set; }
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Subject { get; set; } = "EventGridModel";
        public string EventType { get; set; } = "Azure.Sdk.Sample";
        public object Data { get; set; }
        public string DataVersion { get; set; } = "1.0";
        public DateTimeOffset EventTime { get; set; } = DateTimeOffset.UtcNow;

    }
}