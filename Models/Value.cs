namespace EasySmhiRestApi.Models
{
    using EasySmhiRestApi.Utils;
    using System.Text.Json.Serialization;

    public class Value
    {
        [JsonPropertyName("date")]
        [JsonConverter(typeof(JsonUnixTimeConverter))]
        public DateTime? UtcDateTime { get; set; }

        [JsonPropertyName("value")]
        public string? DataValue { get; set; }

        [JsonPropertyName("quality")]
        public string? Quality { get; set; }
    }
}
