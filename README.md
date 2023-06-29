# DataHub using SignalR

- SignalR server with a javascript and a dotnet client
- JwtBearer token authorization
- Simulates a `send -> soft ack -> processing -> hard ack -> receive more data` similar to how a consumer of a Kafka topic behaves (but there is no Kafka here - just a stream of data generator)
