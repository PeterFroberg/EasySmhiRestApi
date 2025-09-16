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
    

