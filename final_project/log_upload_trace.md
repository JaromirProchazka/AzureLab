# Trace of the Device Log Upload

## Flame Graph

```json
{
  "Device_Send_Log_HTTP": {
    "EventHub_Ingress": {
      "cg-archive": {
        "Archive_Function": "BlobStorage_Write"
      },
      "cg-anomaly": {
        "Anomaly_ContainerApp": [
          "BlobStorage_Read",
          "Anomaly_Inference",
          "State_Table_Write",
          "AnomalyW_Table_Write"
        ]
      }
    }
  }
}
```

Device sends the log POST request. The Event hub first enqueues the event for `cg-archive` group and this event is later solved by the Archive Function which stores the posted data to the blob storage. After this is done, Event hub enqueues to `cg-anomaly` group an event for the logs processing with the logs metadata and blob storage blob address. This is later solved by the Anomaly Container App which updates its state in the State Table and writes the found anomalies to the Anomaly W Table.

## Trace spans

### Device upload

```json
{
  "traceId": "4bf92f3577b34da6a3ce929d0e0e4736",
  "spanId": "span_device_send",
  "parentSpanId": null,
  "name": "Device.SendLogBundle",
  "kind": "CLIENT",
  "startTime": "2026-05-21T10:00:00.000Z",
  "endTime": "2026-05-21T10:00:00.120Z",
  "attributes": {
    "device.id": "device-18422",
    "payload.size.bytes": 4821932,
    "compression": "zip",
    "event.count": 842
  }
}
```

### Event Hub ingress

```json
{
  "traceId": "4bf92f3577b34da6a3ce929d0e0e4736",
  "spanId": "span_eventhub_ingress",
  "parentSpanId": "span_device_send",
  "name": "EventHub.Receive",
  "kind": "SERVER",
  "startTime": "2026-05-21T10:00:00.120Z",
  "endTime": "2026-05-21T10:00:00.180Z",
  "attributes": {
    "eventhub.name": "device-access-point",
    "partition.key": "device-18422",
    "partition.id": "12",
    "delivery.guarantee": "at-least-once"
  }
}
```

### cg-archive

#### Function trigger

```json
{
  "traceId": "4bf92f3577b34da6a3ce929d0e0e4736",
  "spanId": "span_archive_function",
  "parentSpanId": "span_eventhub_ingress",
  "name": "ArchiveFunction.ProcessEvents",
  "kind": "CONSUMER",
  "startTime": "2026-05-21T10:00:00.190Z",
  "endTime": "2026-05-21T10:00:00.260Z",
  "attributes": {
    "consumer.group": "cg-archive",
    "batch.size": 842,
    "checkpoint.strategy": "blob-write-success"
  }
}
```

#### Blob write

```json
{
  "traceId": "4bf92f3577b34da6a3ce929d0e0e4736",
  "spanId": "span_blob_write",
  "parentSpanId": "span_archive_function",
  "name": "BlobStorage.WriteLogArchive",
  "kind": "INTERNAL",
  "startTime": "2026-05-21T10:00:00.200Z",
  "endTime": "2026-05-21T10:00:00.255Z",
  "attributes": {
    "blob.container": "device-logs",
    "blob.path": "/2026/05/21/device-18422/100000.zip",
    "checksum.sha256": "a91f3c2e8d..."
  }
}
```

### cg-anomaly

#### Container App processing

```json
{
  "traceId": "4bf92f3577b34da6a3ce929d0e0e4736",
  "spanId": "span_anomaly_container",
  "parentSpanId": "span_eventhub_ingress",
  "name": "AnomalyDetection.ProcessPartition",
  "kind": "CONSUMER",
  "startTime": "2026-05-21T10:00:00.190Z",
  "endTime": "2026-05-21T10:00:00.600Z",
  "attributes": {
    "consumer.group": "cg-anomaly",
    "partition.id": "12",
    "ordering.guarantee": "sequential-per-partition",
    "state.backend": "azure-table-storage"
  }
}
```

#### Blob fetch

```json
{
  "traceId": "4bf92f3577b34da6a3ce929d0e0e4736",
  "spanId": "span_blob_read",
  "parentSpanId": "span_anomaly_container",
  "name": "BlobStorage.FetchLogs",
  "kind": "INTERNAL",
  "startTime": "2026-05-21T10:00:00.250Z",
  "endTime": "2026-05-21T10:00:00.420Z",
  "attributes": {
    "blob.path": "/2026/05/21/device-18422/100000.zip",
    "cache.hit": false
  }
}
```

#### Anomaly inference

```json
{
  "traceId": "4bf92f3577b34da6a3ce929d0e0e4736",
  "spanId": "span_inference",
  "parentSpanId": "span_anomaly_container",
  "name": "ML.AnomalyInference.Run",
  "kind": "INTERNAL",
  "startTime": "2026-05-21T10:00:00.420Z",
  "endTime": "2026-05-21T10:00:00.560Z",
  "attributes": {
    "model.version": "v3.2.1",
    "input.events": 842,
    "anomalies.detected": 3
  }
}
```

#### State persistence

```json
{
  "traceId": "4bf92f3577b34da6a3ce929d0e0e4736",
  "spanId": "span_state_write",
  "parentSpanId": "span_inference",
  "name": "TableStorage.WriteAnomalyState",
  "kind": "INTERNAL",
  "startTime": "2026-05-21T10:00:00.560Z",
  "endTime": "2026-05-21T10:00:00.590Z",
  "attributes": {
    "table.name": "AnomaliesW",
    "partition.key": "device-18422",
    "rows.inserted": 3
  }
}
```

#### AnomalyW Table Write

```json
{
  "traceId": "4bf92f3577b34da6a3ce929d0e0e4736",
  "spanId": "span_anomaly_w_write",
  "parentSpanId": "span_inference",
  "name": "TableStorage.WriteAnomalyW",
  "kind": "INTERNAL",
  "startTime": "2026-05-21T10:00:00.590Z",
  "endTime": "2026-05-21T10:00:00.620Z",
  "attributes": {
    "table.name": "AnomaliesW",
    "partition.key": "device-18422",
    "operation": "upsert",
    "event.id": "evt-77821",
    "anomaly.count": 3,
    "synced": false,
    "consistency.model": "eventual"
  }
}
```
