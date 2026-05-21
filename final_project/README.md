#  Anomaly detection System

This project descries an Azure cloud system for detecting anomalies in log dumps of IoT Devices. Aldough in this specification, the domains of Service Support Application and Machine Learning Engineering Teams is also described, this specification is mainly written for the purposes of the Cloud Platform Team which is responcible for the Azure cloud integration.

## File structure

- `overall_architecture.md` - describes the whole architecture from the high level
  - `overall_architecture.excalidraw` - provides a visual representation to this description
- `components.md` - describes each component in detail providing information about the hosting environment, configuration, metrics and discussion about tradeoffs
- `components_api.md` - describes the REST API for those components providing it
- `log_upload_trace.md` - describes the trace of the POST operation on the Device Access Point
