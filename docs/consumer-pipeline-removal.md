# Consumer pipeline removal

Standalone Kafka/RedisMq **message consumer** hosts are no longer part of QimERP.

## What changed

- Removed `Demo.Kafka.Consumer` and `Demo.RedisMq.Consumer` samples.
- Removed Kafka/RedisMq consumer discovery, hosted services, consumer actors, and `Use*InConsumer` entry points.
- Producer APIs remain (`AddKafkaProducer` / `AddRedisMqProducer` + `Use*InApi`).
- `AddDbContextWithOutboxConsumer` is obsolete; prefer Temporal workers in WebApi + `ConsumerUserContextService` for activity audit context.

## Replacement

Cross-module async work uses **Temporal** workers registered in each module's WebApi (`AddTemporalWorker`). There is no separate `.Consumer` project per module.
