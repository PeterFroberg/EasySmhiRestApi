namespace EasySmhiRestApi.Clients
{
    using EasySmhiRestApi.Models;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;

    public interface IOpenSmhiDataClient
    {
        Task<ConcurrentDictionary<string, WindAndTempratureResult>> GetTempAndWindForStationById(string stationId, string requestType, string smhiTempratureUrl, string smhiWindUrl);

        Task<ConcurrentDictionary<string, WindAndTempratureResult>> GetTempAndWindForAllStations();
    }
}