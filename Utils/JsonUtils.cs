namespace EasySmhiRestApi.Utils
{
    using EasySmhiRestApi.Models;
    using System.Text.Json;

    public class JsonUtils : IJsonUtils
    {
        private readonly JsonSerializerOptions jsonSerialzerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        public bool ValidateJsonSchema<T>(string jsonString)
        {
            try
            {
                var obj = JsonSerializer.Deserialize<T>(jsonString);
                return obj != null;
            }
            catch
            {
                return false;
            }
        }

        public T ParseJsonProperty<T>(string jsonString, string propertyName)
        {
            using var doc = JsonDocument.Parse(jsonString);

            var jsonProperty = doc.RootElement.GetProperty(propertyName);

            var resultObject = JsonSerializer.Deserialize<T>(
                jsonProperty.GetRawText(),
                jsonSerialzerOptions);

            return resultObject;
        }

        public List<T>? ParseArrayToList<T>(string jsonstring, string propertyname)
        {
            var temp = GetPartialJson(jsonstring, propertyname);
            return JsonSerializer.Deserialize<List<T>>(temp);
        }
        public Dictionary<string, WeatherStation>? ParseArray<T>(string jsonString, string propertyName)
        {
            var resultDictionary = JsonSerializer.Deserialize<List<WeatherStation>>(jsonString, jsonSerialzerOptions).ToDictionary(temp => temp.Key);

            return resultDictionary;
        }

        public string GetPartialJson(string jsonString, string propertyName)
        {
            using var doc = JsonDocument.Parse(jsonString);

            if (!doc.RootElement.TryGetProperty(propertyName, out var arrayElement))
            {
                return string.Empty;
            }

            return arrayElement.GetRawText();
        }
    }


}
