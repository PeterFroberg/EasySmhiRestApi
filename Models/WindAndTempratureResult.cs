namespace EasySmhiRestApi.Models
{
    public class WindAndTempratureResult
    {
        public WeatherStation? WeatherStation { get; set; }

        public Value? AirTemprature { get; set; }

        public Value? GustWind { get; set; }
    }
}
