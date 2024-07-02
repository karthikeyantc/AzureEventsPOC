using System;
using System.Collections.Concurrent;
using AzureEvent.Functions;
using Newtonsoft.Json;

namespace AzureEvent.Function.Services
{
    public interface IEventModelConverterFactory
    {
        JsonConverter<EventModel> Create(string domainName);
        
    }
    public class EventModelConverterFactory : IEventModelConverterFactory
    {
        private readonly ConcurrentDictionary<string, JsonConverter<EventModel>> _converterCache = new ConcurrentDictionary<string, JsonConverter<EventModel>>();

        public JsonConverter<EventModel> Create(string domainName)
        {
            // Ensure EventModelConverter is correctly implementing JsonConverter<EventModel>
            return _converterCache.GetOrAdd(domainName, key => new EventModelConverter(key));
        }
    }
    public class EventModelConverter : JsonConverter<EventModel>
    {
        private readonly string _domainName;
        public EventModelConverter(string domainName)
        {
            _domainName = domainName;
        }
        public override EventModel ReadJson(JsonReader reader, Type objectType, EventModel existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var eventModel = new EventModel();
            serializer.Populate(reader, eventModel);
            if (string.IsNullOrWhiteSpace(eventModel.Topic)) throw new JsonSerializationException("Topic is required");
            // Ensure default values
            if (string.IsNullOrWhiteSpace(eventModel.Id)) eventModel.Id = Guid.NewGuid().ToString();
            if (string.IsNullOrWhiteSpace(eventModel.Subject)) eventModel.Subject = "EventGridModel";
            if (string.IsNullOrWhiteSpace(eventModel.EventType)) eventModel.EventType = "Azure.Sdk.Sample " + _domainName;
            if (eventModel.EventTime == default) eventModel.EventTime = DateTimeOffset.UtcNow;
            if (string.IsNullOrWhiteSpace(eventModel.DataVersion)) eventModel.DataVersion = "1.0";

            return eventModel;
        }

        public override void WriteJson(JsonWriter writer, EventModel value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}