namespace EasySmhiRestApi.tests
{
    using System.Net.Http;
    using EasySmhiRestApi.Clients;
    using EasySmhiRestApi.Models;
    using EasySmhiRestApi.Utils;
    using Moq;

    public class OpenSmhiDataClientTests
    {
        [Theory]
        [InlineData("183750", "12.5", "5.4", "hour")]
        [InlineData("183750", "12.5", "5.4", "day")]
        public async Task GetTempAndWindForStationById_AddsTemperatureAndWindValues(string stationId, string tempratur, string gustWind, string requestType)
        {
            //Arrange
            var tempratureStation = new WeatherStation
            {
                Key = stationId,
                Values = new() { new Value { DataValue = tempratur } }
            };

            var gustWindStation = new WeatherStation
            {
                Key = stationId,
                Values = new() { new Value { DataValue = gustWind } }
            };

            var httpClient = HttpClientTestHelpers.CreateFakeHttpClient(req =>
                req.RequestUri!.ToString().Contains("wind")
                    ? "{ \"station\": \"wind-json\" }"
                    : "{ \"station\": \"temp-json\" }");

            var jsonUtils = new Mock<JsonUtils>();

            var openSmhiDataClient = new TestableOpenSmhiDataClient(httpClient, jsonUtils.Object, tempratureStation, gustWindStation);

            //Act
            var result = await openSmhiDataClient.GetTempAndWindForStationById(
                stationId,
                requestType,
                smhiTempratureUrl: "https://api/temp/--STATION_ID--",
                smhiWindUrl: "https://api/wind/--STATION_ID--"
                );

            //Assert
            Assert.True(result.ContainsKey(stationId));
            var windAndTempratureResult = result[stationId];
            Assert.Equal(tempratur, windAndTempratureResult.AirTemprature.DataValue);
            Assert.Equal(gustWind, windAndTempratureResult.GustWind.DataValue);

        }

        [Theory]
        [InlineData("183750", "12.5", "5.4", "183751", "12.5", "5.4", "hour", 2)]
        public async Task GetTempAndWindForAllStations_MergesData_ForTwoStations(string stationId1, string tempratur1, string gustWind1, string stationId2, string tempratur2, string gustWind2, string requestType, int expectedCount)
        {
            //Arrange
            var tempratureDictionary = BuildStations((stationId1, tempratur1), (stationId2, tempratur2));
            var gustWindDictionary = BuildStations((stationId1, gustWind1), (stationId2, gustWind2));

            var httpClient = HttpClientTestHelpers.CreateFakeHttpClient(req =>
                req.RequestUri!.ToString().Contains("wind")
                    ? "{ \"station\": \"wind-json\" }"
                    : "{ \"station\": \"temp-json\" }");

            var jsonUtils = new Mock<JsonUtils>();

            var openSmhiDataClient = new TestableOpenSmhiDataClientForAll(httpClient, jsonUtils.Object, tempratureDictionary, gustWindDictionary);

            //Act
            var result = await openSmhiDataClient.GetTempAndWindForAllStations();

            //Assert
            Assert.Equal(expectedCount, result.Count);
            Assert.Equal(tempratur1, result[stationId1].AirTemprature.DataValue);
            Assert.Equal(gustWind1, result[stationId1].GustWind.DataValue);
            Assert.Equal(tempratur2, result[stationId2].AirTemprature.DataValue);
            Assert.Equal(gustWind2, result[stationId2].GustWind.DataValue);

        }

        private static Dictionary<string, WeatherStation> BuildStations(params (string key, string value)[] data)
        {
            var dict = new Dictionary<string, WeatherStation>();
            foreach (var (stationId, StationValue) in data)
            {
                dict[stationId] = new WeatherStation
                {
                    Key = stationId,
                    Values = new() { new Value { DataValue = StationValue } }
                };
            }
            return dict;
        }

        private class TestableOpenSmhiDataClient : OpenSmhiDataClient
        {
            private readonly WeatherStation _tempratureStation;
            private readonly WeatherStation _gustWindStation;

            public TestableOpenSmhiDataClient(HttpClient httpClient, IJsonUtils jsonUtils, WeatherStation tempratureStation, WeatherStation gustWindStation)
                : base(httpClient, jsonUtils)
            {
                _tempratureStation = tempratureStation;
                _gustWindStation = gustWindStation;
            }

            protected override Task<WeatherStation> ParseSingelStationData(string _)
            {
                if (_.Contains("wind"))
                {
                    return Task.FromResult(_gustWindStation);
                }

                return Task.FromResult(_tempratureStation);
            }
        }

        private class TestableOpenSmhiDataClientForAll : OpenSmhiDataClient
        {
            private readonly Dictionary<string, WeatherStation> _temp;
            private readonly Dictionary<string, WeatherStation> _wind;
            private bool _tempReturned;

            public TestableOpenSmhiDataClientForAll(HttpClient httpClient,
                IJsonUtils jsonUtils,
                Dictionary<string, WeatherStation> temp,
                Dictionary<string, WeatherStation> wind)
                : base(httpClient, jsonUtils)
            {
                _temp = temp;
                _wind = wind;
            }

            protected override Task<Dictionary<string, WeatherStation>?> ParseMultipleStationsData(string _)
            {
                var result = !_tempReturned ? _temp : _wind;
                _tempReturned = true;
                return Task.FromResult<Dictionary<string, WeatherStation>?>(result);
            }
        }

    }
}