# Components descriptions

## Device Access Point

**Type:** Event hub

**Configuration:**

- Uses 64–256 partitions
- Partitioning by device ID (PartitionKey = DeviceId)
- Ensures that for each device, its cooresponding log events are ordered correctally by the timestamp from the post request
- Ensures that the data from one device will be processed by the same instance of the Anomaly Detection algorithm
- Consumer groups:
  - `cg-archive`
    - for raw log storage
  - `cg-anomaly`
    - for anomaly detection pipeline
    - selds only metadata of the logs blob
    - Strict ordering requirement (True)
- for both:
  - (eventId, deviceId, timestamp) events must be uniquely identify a logical event
  - Retry semantics: at-least-once delivery
  - Poison event behavior: malformed event skipped
  - Checkpointing rule: blob archival success

**autoscaling:**

- autoscaling metric hierarchy: 1. Partition lag time 2. Partition lag count 3. CPU 4. Memory

1. Trigger 1 - Lag count

| Metric           |            Suggested Value |
| :--------------- | -------------------------: |
| target lag       | 10k–50k events per replica |
| polling interval |                     15–30s |
| cooldown         |                    2–5 min |

2. Trigger 2 - Lag time

| Lag Time |    Action |
| :------- | --------: |
| <2 min   |   healthy |
| 5 min    | scale out |
| 15 min   |   warning |
| 30 min   |  critical |

**Metrics:**

We use the `Asynchronous Gauge` to meassure the Event Hub lag count since it is also important for scalability of our system. The unit is `{events}` since we measure how many events behind are we.

## Logs database

**Type:** Blob storage

**Configuration:**

- The naming convention for the blob storage files is `/{year}/{month}/{day}/{deviceId}/{timestamp}.log.zip`
- As metadata with each blob, we store deviceId, timestamp, payloadSize, checksumSha256. The checksum is for integrity.
- Blob retention set for 90 days

## Anomalies W

**Type:** Azure Table Storage

**Configuration:**

- Anomalies W is write-optimized anomaly persistence store.
  - append-oriented
  - entities are immutable after creation except for status fields
- The exact schema of the table depends on the resutls of the Anomaly detection algorithm. However the table uses the deviceId, eventId and timestamp as a row key.
  - there is also synced column which denotes if the row is synced with the Anomalies W database (default False)
- DeviceId is the partition key

**Synchronization:**

- Anomalies W is the source of truth
- Anomalies R is eventually consistent with Anomalies W
- The Get Anomalies API operates on eventually consistent data
- Recently detected anomalies may not immediately appear in the read model

## Anomalies R

**Type:** Azure Table Storage

**Configuration:**

- Anomalies R is the read-optimized projection of anomaly data used exclusively by the Service Support Application
  - Anomalies R is a materialized view of Anomalies W
  - All mutations to Anomalies R originate exclusively from the Sync Azure Function
- The exact schema of the table depends on the resutls of the Anomaly detection algorithm. However the table uses the deviceId, eventId, resolved and timestamp as a row key.
  - resolved column denotes if the given anomaly was resolved by Technician (defalut false)
  - All fields except 'resolved' are immutable
- Retention of active anomalies is indefinite, for resolved anomalies it is 90 days
- DeviceId is the partition key

**Synchronization:**

- Sync Function MUST use UPSERT (InsertOrReplace) semantics.

## Sync

**Type:** Azure Function

Takes any new row in the Anomalies W database, removes the `synced` column, add the `resolved` column and pushes this to the Anomalies R database. After this is sucessufly done, mark the original row in the Anomalies W as synced by setting the `synced` column to True.

**Configuration:**

Trigger sources are the anomaly-write-events and anomaly-update-events.

**Tradeoffs:**

Since this mechanism is stateless and trigered by an event, Azure Function is perfect chois.

Reguarding the syncing it self, this mechanism introduces complexity and is little bit more costly compared to a single Anomaly database. However this is outweighed by the decuppeling of the layers of each team speeding up the developement and a better scalability and availability of each subsystem.

**Metrics:**

We measurre sync_latency_ms (in ms) by using the Histogram (with double) counter.

## Anomaly detection

**Type:** Azure Container Apps

**Configuration:**

- It is a partition-aware, stateful stream processing system that consumes Event Hub metadata events and performs anomaly inference over log payloads stored in Blob Storage
- It processes metadata events and fetches blobs only when needed from Logs database
- Each Container App replica owns one or more Event Hub partitions and process events sequentially per partition
- State is externalized to Table storage to survive scaling and restarts

**Tradeoffs:**

Aldough Azure Container Apps is more complex and expensive than a simple App Service, the potentialy huge burst load on the system makes this necessary.

## Technicians Manager

**Type:** Azure App Service (HTTP server)

The Technicians Manager component acts as caching layer in front of the external Technicians API. It is a C# HTTP server providing REST API to Backend component.

**Configuration:**

## Technicians Cache

**Type:** Table Storage

A cache for Technicians Manager component. Used for caching responces of the Technicians API external system.

**Configuration:**

- Read-through cache
- Cache entries expire after 1 hour
- The technician cache is eventually consistent with the external Technicians API
- Contains two caches, one for device info and one for technitians info

**Metrics:**

We track the technician_cache_misses_total and external_api_requests_total to see the percentage of cache misses. So the units are respectivelly. The basic Counter (with long) is used.

## Backend

**Type:** App service

The Backend App Service provides the read-oriented API layer for the Service Support Application. It is stateless and horizontally scalable. The Service Support Application Team can simply load theyr web app to this backend.

**Configuration:**

- All backend anomaly queries SHALL target a single partition key
- The backend queries Anomalies R exclusively and does not directly access Anomalies W
- Backend expects the Technicians Manager provided by Cloud Platform Team as a caching layer to be available
  - In case of its unavailability, Backend tries to access the Technicians API dirrectally.

**Tradeoffs:**

Since we can expect at maximum tousends of technitians. A simply App service is enough. Since the load on this part of the system is not so large, the Service Support Application Team is free to implement a requests caching inside their environment without an external Azure database.
