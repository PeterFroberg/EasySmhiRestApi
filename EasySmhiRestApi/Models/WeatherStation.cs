namespace EasySmhiRestApi.Models
{
    using System.Text.Json.Serialization;

    public class WeatherStation
    {
        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("updated")]
        public long? Updated { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("summary")]
        public string? Summary { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("owner")]
        public string? Owner { get; set; }

        [JsonPropertyName("ownerCategory")]
        public string? OwnerCategory { get; set; }

        [JsonPropertyName("measuringStations")]
        public string? MeasuringStations { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("height")]
        public double Height { get; set; }

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("active")]
        public bool Active { get; set; }

        [JsonPropertyName("from")]
        public long From { get; set; }

        [JsonPropertyName("to")]
        public long To { get; set; }

        [JsonPropertyName("value")]
        public List<Value>? Values { get; set; }
    }
}
