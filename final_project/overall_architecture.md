# Architecture description

Our architecture is vericaly layered into a presentation, business and persistence layers. It also horizontaly layered based on the teams in the project into the Service Support Application Team, Cloud Platform Team and Machine Learning Engineering Team.

We have two kinds of users, the Devices and the Technicians. Our system also uses the external Technicians API.

The devices access the system throw the Device Access Point implemented with Event hub throw the HTTP protocol. This End point send any data from devices to the Event Consumer Azure Function and to the Anomaly detection Azure Container Apps. The Event Consumer stores it to the Logs Blob-storage, and the Anomaly detection analyzes the devices data fetched from the blob-storage and stores the result of this analyses to the Anomalies W Azure tables database for Anomalies data writes.

For the Service Support Application Team, these is a Single Page App accessed by Technicians. This web app has an App service Backend and a Anomalies R Azure tables dababase. To this database, the mensioned Anomalies W database data are transfered throw the Cloud Platform Team system. These to databases are this way kept in sync and providing decuppeling of the reading and the writing parts of the Anomalies system.

The Cloud Platform Team part of the system provide a Sync Azure Function component which using two Queues Azure Service Buses both between it and Anomalies W and Anomalies R databases synchronizes these databases.

Finally the Backend component needs to fetch information from the unreliable Technicians
API. For that the Cloud Platform Team provides a Technicians Manager App service with a Techn. Cache Table Storage. The Technicians API responces are this way cached and the backend fetches from the Technicians Manager component.
