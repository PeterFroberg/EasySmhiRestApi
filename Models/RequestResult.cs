namespace EasySmhiRestApi.Models
{
    public class RequestResult
    {
        public int RequestedStations { get; set; }
        
        public bool AllRequestedStationsReturnedData { get; set; }

        public List<WindAndTempratureResult> WindAndTempratureResults {get; set;}
    }
}
