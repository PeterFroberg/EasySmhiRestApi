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
            var results = new ConcurrentDictionary<string, WindAndTempratureResult>();

            var tempratureTask = FetchSmhiTemperatureDataHourAsync(stationId, smhiTempratureUrl, results);
            var gustWindTask = requestType == "hour"
                ? FetchSmhiGustWindDataHourAsync(stationId, smhiWindUrl, results)
                : FetchSmhiGustWindDataDayAsync(stationId, smhiWindUrl, results);

            await Task.WhenAll(tempratureTask, gustWindTask);

            return results;
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
                            _ =>
                            {
                                var firstValue = weatherStation.Value?.Values?.FirstOrDefault();
                                var newResult = new WindAndTempratureResult
                                {
                                    WeatherStation = weatherStation.Value,
                                    AirTemprature = firstValue
                                };

                                if (weatherStation.Value != null)
                                {
                                    weatherStation.Value.Values = null;
                                }

                                return newResult;
                            },
                        (_, existing) =>
                        {
                            var firstValue = weatherStation.Value?.Values?.FirstOrDefault();
                            existing.AirTemprature = firstValue;

                            if (weatherStation.Value != null)
                            {
                                weatherStation.Value.Values = null;
                            }

                            return existing;
                        });
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
                            _ =>
                            {
                                var firstValue = weatherStation.Value?.Values?.FirstOrDefault();

                                var newResult = new WindAndTempratureResult
                                {
                                    WeatherStation = weatherStation.Value,
                                    GustWind = firstValue
                                };

                                if (weatherStation.Value != null)
                                {
                                    weatherStation.Value.Values = null;
                                }

                                return newResult;
                            },
                            (_, existing) =>
                            {
                                var firstValue = weatherStation.Value?.Values?.FirstOrDefault();
                                existing.GustWind = firstValue;

                                if (weatherStation.Value != null)
                                {
                                    weatherStation.Value.Values = null;
                                }

                                return existing;
                            });
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

        private async Task FetchSmhiTemperatureDataHourAsync(
            string stationId,
            string smhiTemperatureUrl,
            ConcurrentDictionary<string, WindAndTempratureResult> results)
        {
            var json = await GetDataFromSmhi(smhiTemperatureUrl.Replace("--STATION_ID--", stationId));
            if (string.IsNullOrEmpty(json)) return;

            var weatherStation = await ParseSingelStationData(json);
            if (weatherStation == null) return;

            results.AddOrUpdate(
                weatherStation.Key,
                _ =>
                {
                    var firstValue = weatherStation.Values?.Count == 0 ? null : weatherStation.Values.First();
                    var newReult = new WindAndTempratureResult
                    {
                        WeatherStation = weatherStation,
                        AirTemprature = firstValue
                    };

                    weatherStation.Values = null;

                    return newReult;
                },
                (_, existing) =>
                {
                    existing.AirTemprature = weatherStation.Values?.Count == 0 ? null : weatherStation.Values.First();
                    return existing;
                });
        }

        private async Task FetchSmhiGustWindDataHourAsync(
            string stationId,
            string smhiWindUrl,
            ConcurrentDictionary<string, WindAndTempratureResult> results)
        {
            var json = await GetDataFromSmhi(smhiWindUrl.Replace("--STATION_ID--", stationId));
            if (string.IsNullOrEmpty(json)) return;

            var weatherStation = await ParseSingelStationData(json);
            if (weatherStation == null) return;

            results.AddOrUpdate(
                weatherStation.Key,
                _ => new WindAndTempratureResult
                {
                    WeatherStation = weatherStation,
                    GustWind = weatherStation.Values?.Count == 0 ? null : weatherStation.Values.First()
                },
                (_, existing) =>
                {
                    existing.GustWind = weatherStation.Values?.Count == 0 ? null : weatherStation.Values.First();
                    return existing;
                });
        }

        private async Task FetchSmhiGustWindDataDayAsync(
            string stationId,
            string smhiWindUrl,
            ConcurrentDictionary<string, WindAndTempratureResult> results)
        {
            var json = await GetDataFromSmhi(smhiWindUrl.Replace("--STATION_ID--", stationId));
            if (string.IsNullOrEmpty(json)) return;

            var weatherStation = await ParseSingelStationData(json);
            if (weatherStation == null) return;

            var windValues = weatherStation.Values
                .Select(v => double.TryParse(v.DataValue, NumberStyles.Any,
                                             CultureInfo.InvariantCulture, out var d)
                                ? (double?)d : null)
                .Where(d => d.HasValue)
                .Select(d => d.Value)
                .ToList();

            var averageWind = windValues.Any() ? Math.Round(windValues.Average(), 1) : double.NaN;
            weatherStation.Values = null;

            results.AddOrUpdate(
                weatherStation.Key,
                _ => new WindAndTempratureResult
                {
                    WeatherStation = weatherStation,
                    GustWind = new Value { DataValue = averageWind.ToString(CultureInfo.InvariantCulture) }
                },
                (_, existing) =>
                {
                    existing.GustWind = new Value { DataValue = averageWind.ToString(CultureInfo.InvariantCulture) };

                    return existing;
                });
        }

    }
}
