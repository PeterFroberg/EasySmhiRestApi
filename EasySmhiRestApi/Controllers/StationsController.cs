namespace EasySmhiRestApi.Controllers
{
    using EasySmhiRestApi.Clients;
    using EasySmhiRestApi.Enum;
    using EasySmhiRestApi.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Concurrent;

    [Authorize(AuthenticationSchemes = "ApiKey")]
    [ApiController]
    [Route("[controller]")]
    public class StationsController : ControllerBase
    {
        private IOpenSmhiDataClient _openSmhiDataClient;

        private readonly ILogger<StationsController> _logger;

        public StationsController(IOpenSmhiDataClient openSmhiDataClient ,ILogger<StationsController> logger)
        {
            _logger = logger;
            _openSmhiDataClient = openSmhiDataClient;
        }

        /// <summary>
        /// Get weather stations Temprature and GustWind.
        /// If no body is present Allstations will be returned. 
        /// If body is included in the request a result will be returnd if station exist otherwise HTTP status 404 will be returned, The RequestType can be either "hour" or "day"
        /// example body:{"stationId": "183750","RequestType": "day"} if hour is used the result will be for the latest hour. 
        /// If day is used an average for the last 24 hour will be returned
        /// </summary>
        /// <returns>A list of stations.</returns>
        [HttpGet("GetStationWindAndTemprature")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetStationWindAndTemprature([FromBody] RequestBody? requestBody)
        {
            ConcurrentDictionary<string, WindAndTempratureResult>? result;
            if (requestBody == null)
            {
                result =_openSmhiDataClient?.GetTempAndWindForAllStations().Result;
                if (result!.IsEmpty)
                {
                    return NotFound();
                }
                else
                {
                    return Ok(result.Values);
                }
            }
            if (requestBody.RequestType == "hour")
            {
                result = _openSmhiDataClient?.GetTempAndWindForStationById(requestBody.StationId, requestBody.RequestType, SmhiEndpoint.GetLastHourTempratureForStationById, SmhiEndpoint.GetGustWindForLastHourForStationById).Result;
               
                return VerifySingleResult(result);
            }
            if (requestBody.RequestType == "day")
            {
                result = _openSmhiDataClient.GetTempAndWindForStationById(requestBody.StationId, requestBody.RequestType, SmhiEndpoint.GetTempratrueForStationLastDay, SmhiEndpoint.GetByVindForCompleteDayForStaionById).Result;
                
                return VerifySingleResult(result);
            }

            return BadRequest("Invalid body");
        }

        private IActionResult VerifySingleResult(ConcurrentDictionary<string, WindAndTempratureResult> result)
        {
            if (result!.IsEmpty)
            {
                return NotFound();
            }
            else
            {
                return Ok(result.Values.First());
            }
        }
    }
}
