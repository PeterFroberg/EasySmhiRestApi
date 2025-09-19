namespace EasySmhiRestApi.Utils
{
    using EasySmhiRestApi.Models;
    using System.Collections.Generic;

    public interface IJsonUtils
    {
        bool ValidateJsonSchema<T>(string jsonString);
        string GetPartialJson(string jsonString, string propertyName);
        Dictionary<string, WeatherStation>? ParseArray<T>(string jsonString, string propertyName);
        List<T>? ParseArrayToList<T>(string jsonstring, string propertyname);
        T ParseJsonProperty<T>(string jsonString, string propertyName);
    }
}