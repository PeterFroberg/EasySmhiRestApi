namespace EasySmhiRestApi.Enum
{
    public static class SmhiEndpoint
    {
        public const string GetTemperatureForAllStationsLastestHour = """https://opendata-download-metobs.smhi.se/api/version/1.0/parameter/1/station-set/all/period/latest-hour/data.json""";
        
        public const string GetLastHourTempratureForStationById = """https://opendata-download-metobs.smhi.se/api/version/1.0/parameter/1/station/--STATION_ID--/period/latest-hour/data.json""";

        public const string GetGustWindForLastHourForStationById = """https://opendata-download-metobs.smhi.se/api/version/1.0/parameter/21/station/--STATION_ID--/period/latest-hour/data.json""";

        public const string GetGustWindForAllStationsLatestHour = """https://opendata-download-metobs.smhi.se/api/version/1.0/parameter/21/station-set/all/period/latest-hour/data.json""";

        public const string GetTempratrueForStationLastDay = """https://opendata-download-metobs.smhi.se/api/version/1.0/parameter/2/station/--STATION_ID--/period/latest-day/data.json""";

        public const string GetByVindForCompleteDayForStaionById = """https://opendata-download-metobs.smhi.se/api/version/1.0/parameter/21/station/--STATION_ID--/period/latest-day/data.json""";
    }
}
