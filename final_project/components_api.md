# Components

The communication on the edges of the teams systems uses HTTP API.

## Device Access Point

**Request Method:** `POST`

**Request Query Parameters:** None

**Request Body:**

```json
{
  "deviceId": "device1",
  "timestamp": "2026-05-20T15:27:00.123456Z",
  "location": "c05a3ec5-7460-4f84-ba80-55ce62bf0e95",
  "logsSize": 5242880
}
```

and the zipped logs data stream. The timestamp format is `YYYY-MM-DDTHH:mm:ss.SSSSSSZ`. The `logsSize` has type int and denotes number of bytes of the payload.

**Response Code:**

- `201 Created` - Log was successfully stored.
- `400 Bad Request` - Body is not in the correct form.

**Response Body:** None

## Get Anomalies

Done by Technicians from their mobile app.

**Request Method:** `GET`

**Request Query Parameters:**

- `technicianId` - id of the technician (example: "0c8af4d3-3d3b-4f43-b188-47910f3f00f0")

**Request Body:** None

**Response Code:**

- `200 OK` - Anomalies fetched
- `204 No Content` - No Anomalies for this technician found
- `400 Bad Request` - Technician ID not found

**Response Body:**

```json
{
  "responsibleTechnicianId": "0c8af4d3-3d3b-4f43-b188-47910f3f00f0",
  "anomalies": [ // list of anomalies
        {
            "deviceId": "device1",
            "eventId": "2d1a3311-6677-4b53-b09b-626ba8ba484b",
            "timestamp": "2026-05-20T15:27:00.123456Z",
            "location": "c05a3ec5-7460-4f84-ba80-55ce62bf0e95",
            "anomalyDescription": "..."
        }
  ]
}
```

## Finish Anomaly

**Request Method:** `POST`

**Request Query Parameters:**

- `deviceId` - id of the device (example: "device1")
- `eventId` - id of the event which created this anomaly (example: "2d1a3311-6677-4b53-b09b-626ba8ba484b")
- `timestamp` - timestamp of the anomaly creation (example: "2026-05-20T15:27:00.123456Z")

**Request Body:** None

**Response Code:**

- `200 OK` - Anomaly status updated
- `400 Bad Request` - Anomaly not found

**Response Body:** None

## Fetch Technicians (Private)

Used by the Service Support Application Team Backend to fetch the Technicians information from the Technicians Manager.

**Request Method:** `GET`

**Request Query Parameters:**

- `technicianId` - id of the technician (example: "0c8af4d3-3d3b-4f43-b188-47910f3f00f0")

**Request Body:** None

**Response Code:**

- `200 OK` - Technicians fetched
- `400 Bad Request` - Technician ID not found

**Response Body:**

```json
{"deviceId": "device1", "location": "c05a3ec5-7460-4f84-ba80-55ce62bf0e95"}
{"deviceId": "device2", "location": "c05a3ec5-7460-4f84-ba80-55ce62bf0e95"}
{"deviceId": "device3", "location": "eb7a4b73-1451-4575-95a6-381b4cf90942"}
{"deviceId": "device4", "location": "c05a3ec5-7460-4f84-ba80-55ce62bf0e95"}
{"deviceId": "device5", "location": "e88ace7d-d2c2-4a82-b966-8009ccd7afa5"}
{"deviceId": "device6", "location": "eb7a4b73-1451-4575-95a6-381b4cf90942"}
```

## Fetch device Info (Private)

Used by the Service Support Application Team Backend to fetch the device information from the Technicians Manager.

**Request Method:** `GET`

**Request Query Parameters:**

- `deviceId:` - id of the device (example: "device1")

**Request Body:** None

**Response Code:**

- `200 OK` - Device fetched
- `400 Bad Request` - Device ID not found

**Response Body:**

```json
{
  "deviceId": "device1",
  "responsibleTechnicianId": "0c8af4d3-3d3b-4f43-b188-47910f3f00f0",
  "location": "c05a3ec5-7460-4f84-ba80-55ce62bf0e95"
}
```
