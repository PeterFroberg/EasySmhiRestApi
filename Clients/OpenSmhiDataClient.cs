namespace EasySmhiRestApi.Clients
{
    using EasySmhiRestApi.Enum;
    using EasySmhiRestApi.Models;
    using EasySmhiRestApi.Utils;
    using System.Collections.Concurrent;
    using System.Globalization;

    public class OpenSmhiDataClient : IOpenSmhiDataClient
    {
        private readonly HttpClient _httpClient;
        private readonly IJsonUtils _jsonUtils;

        public OpenSmhiDataClient(HttpClient httpClient, IJsonUtils jsonUtils)
        {
            _httpClient = httpClient;
            _jsonUtils = jsonUtils;
        }

        public async Task<(bool IsSuccesfull, string JsonResult)?> GetTempratureForAllStaionLatestHour()
        {
            var jsonResponse = await _httpClient.GetStringAsync(SmhiEndpoint.GetTemperatureForAllStationsLastestHour);

            if (_jsonUtils.ValidateJsonSchema<WeatherStation>(jsonResponse))
            {
                return (true, jsonResponse);
            }

            return (false, string.Empty);
        }

        public async Task<ConcurrentDictionary<string, WindAndTempratureResult>> GetTempAndWindForStationById(string stationId, string requestType, string smhiTempratureUrl, string smhiWindUrl)
        {
            ConcurrentDictionary<string, WindAndTempratureResult> resultDictionary = new ConcurrentDictionary<string, WindAndTempratureResult>();

            async Task FetchSmhiTempratureData()
            {
                var smhiResponseForTemprature = await GetDataFromSmhi(smhiTempratureUrl.Replace("--STATION_ID--", stationId));
                if (!string.IsNullOrEmpty(smhiResponseForTemprature))
                {
                    var weatherStation = await ParseSingelStationData(smhiResponseForTemprature);
                    if (weatherStation != null)
                    {
                        resultDictionary.AddOrUpdate(
                            weatherStation.Key,
                            _ = new WindAndTempratureResult
                            {
                                WeatherStation = weatherStation,
                                AirTemprature = weatherStation?.Values?.Count == 0 ? null : weatherStation?.Values?.First()
                            },
                            (_, existing) =>
                            {
                                existing.AirTemprature = weatherStation?.Values?.Count == 0
                                 ? null
                                 : weatherStation?.Values?.First();
                                return existing;
                            }
                        );
                    }
                }
            }

            async Task FetchSmhiGustWindDataForHour()
            {
                var smhiResponseForGustWind = await GetDataFromSmhi(smhiWindUrl.Replace("--STATION_ID--", stationId));
                if (!string.IsNullOrEmpty(smhiResponseForGustWind))
                {
                    var weatherStation = await ParseSingelStationData(smhiResponseForGustWind);
                    if (weatherStation != null)
                    {
                        resultDictionary.AddOrUpdate(
                             weatherStation.Key,
                             _ = new WindAndTempratureResult
                             {
                                 WeatherStation = weatherStation,
                                 GustWind = weatherStation?.Values?.Count == 0 ? null : weatherStation?.Values?.First()
                             },
                             (_, existing) =>
                             {
                                 existing.GustWind = weatherStation?.Values?.Count == 0
                                  ? null
                                  : weatherStation?.Values?.First();
                                 return existing;
                             }
                        );
                    }
                }
            }

            async Task FetchSmhiGustWindDataForDay()
            {
                var smhiResponseForGustWind = await GetDataFromSmhi(smhiWindUrl.Replace("--STATION_ID--", stationId));
                if (!string.IsNullOrEmpty(smhiResponseForGustWind))
                {
                    var weatherStation = await ParseSingelStationData(smhiResponseForGustWind);
                    if (weatherStation != null)
                    {
                        var windValues = weatherStation.Values
                        .Select(value =>
                        {
                            bool success = double.TryParse(value.DataValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double result);
                            return new { success, result };
                        })
                        .Where(x => x.success)
                        .Select(x => x.result).ToList();

                        var averageWind = windValues.Any() ? Math.Round(windValues.Average(), 1) : double.NaN;

                        resultDictionary.AddOrUpdate(
                            weatherStation.Key,
                            _ = new WindAndTempratureResult
                            {
                                WeatherStation = weatherStation,
                                GustWind = new Value()
                                {
                                    DataValue = averageWind.ToString()
                                }
                            },
                            (_, existing) =>
                            {
                                existing.GustWind = new Value()
                                {
                                    DataValue = averageWind.ToString(CultureInfo.InvariantCulture)
                                };
                                existing.WeatherStation.Values = null;

                                return existing;
                            }
                        );
                    }
                }
            }
            await Task.WhenAll(
                FetchSmhiTempratureData(),
                requestType == "hour" ? FetchSmhiGustWindDataForHour() : FetchSmhiGustWindDataForDay()
                );

            return resultDictionary;
        }



        public async Task<ConcurrentDictionary<string, WindAndTempratureResult>> GetTempAndWindForAllStations()
        {
            ConcurrentDictionary<string, WindAndTempratureResult> resultDictionary = new ConcurrentDictionary<string, WindAndTempratureResult>();

            async Task FetchSmhiTempratureData()
            {
                var smhiResponseForTemprature = await GetDataFromSmhi(SmhiEndpoint.GetTemperatureForAllStationsLastestHour);
                if (!string.IsNullOrEmpty(smhiResponseForTemprature))
                {
                    var stationAndTemp = await ParseMultipleStationsData(smhiResponseForTemprature);

                    foreach (var weatherStation in stationAndTemp)
                    {
                        resultDictionary.AddOrUpdate(
                            weatherStation.Key,
                            _ = new WindAndTempratureResult
                            {
                                WeatherStation = weatherStation.Value,
                                AirTemprature = weatherStation.Value?.Values.Count == 0 ? null : weatherStation.Value.Values.First()
                            },
                            (_, existing) =>
                            {
                                existing.AirTemprature = weatherStation.Value?.Values.Count == 0
                                 ? null
                                 : weatherStation.Value.Values.First();

                                return existing;
                            }
                        );
                    }
                }
            }

            async Task FetchSmhiGustWindData()
            {

                var smhiResponseForGustWind = await GetDataFromSmhi(SmhiEndpoint.GetGustWindForAllStationsLatestHour);
                if (!string.IsNullOrEmpty(smhiResponseForGustWind))
                {
                    var SationsWithWind = await ParseMultipleStationsData(smhiResponseForGustWind);
                    foreach (var weatherStation in SationsWithWind)
                    {
                        resultDictionary.AddOrUpdate(
                            weatherStation.Key,
                            _ = new WindAndTempratureResult
                            {
                                WeatherStation = weatherStation.Value,
                                GustWind = weatherStation.Value?.Values.Count == 0 ? null : weatherStation.Value.Values.First()
                            },
                            (_, existing) =>
                            {
                                existing.GustWind = weatherStation.Value?.Values.Count == 0
                                 ? null
                                 : weatherStation.Value.Values.First();

                                return existing;
                            }
                        );
                    }
                }
            }
            await Task.WhenAll(
                FetchSmhiTempratureData(),
                FetchSmhiGustWindData()
                );

            return resultDictionary;
        }

        protected virtual async Task<WeatherStation>? ParseSingelStationData(string smhiResultJson)
        {
            var station = _jsonUtils.ParseJsonProperty<WeatherStation>(smhiResultJson, SmhiJsonElement.station.ToString());
            station.Values = _jsonUtils.ParseArrayToList<Value>(smhiResultJson, SmhiJsonElement.value.ToString());

            return station;
        }

        protected virtual async Task<Dictionary<string, WeatherStation>?> ParseMultipleStationsData(string SmhiResultAsJson)
        {
            var stationsJson = _jsonUtils.GetPartialJson(SmhiResultAsJson, SmhiJsonElement.station.ToString());
            var ListOfStationsWithData = _jsonUtils.ParseArray<WeatherStation>(stationsJson, SmhiJsonElement.station.ToString());

            return ListOfStationsWithData;
        }

        private async Task<string> GetDataFromSmhi(string url)
        {
            var httpTempratureResponse = await _httpClient.GetAsync(url);
            if (httpTempratureResponse.IsSuccessStatusCode)
            {
                return await httpTempratureResponse.Content.ReadAsStringAsync();
            }

            return string.Empty;
        }
    }
}
