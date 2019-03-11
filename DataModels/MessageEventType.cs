using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DataModels
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MessageEventType
    {
        INSERT, REMOVE, MODIFY
    }
}