**EasySmhiRestApi**
- To request a specific station and to use daily averages
    - Use a small jsonobject in the body of the request
        ```json
        {
            "stationId": "183750",
            "RequestType": "day"
        }
        ```
     - To get latesthour use requesttype hour
    ```json
        {
            "stationId": "183750",
            "RequestType": "hour"
        }
    ```
    - If no json body is used allstations will be returnd with last hour data
    
**Process**
- Discovery of the Api
    - How is data structured
    - How is Datadelivered
    - Find suitable enpoints to get correct data
    - using Postman to access the SMHI API and test endpoints
- Create resultmodels for easy delivery through REST API
- "Secure" API access with API key
    - For easy demonstration API key is stored in appsetting.json
    - Clients add the API-key in the header
        - Return a unauthorized response if API i missing or invalid
- Create Single staion request
    - Minimize response time 
        - Get data in parallell and build the response
            - Make building process reusable for an allstation response as well
- Create all staions response
    - Minimize response time 
        - Get data in parallell and build the response
- Create Unit Tests