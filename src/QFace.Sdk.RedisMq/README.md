# QFace.Sdk.RedisMq

Redis pub/sub **publisher** helpers for .NET (Akka actor).

> **Migration note:** Message-consumer hosts (`UseRedisMqInConsumer`, `[Consumer]`, consumer samples)
> were removed. QimERP cross-module sync uses **Temporal workers hosted in WebApi** processes.
> Do not add new RedisMq consumer projects.

## Install

```bash
dotnet add package QFace.Sdk.RedisMq
```

## Publisher usage

```csharp
builder.Services.AddRedisMqProducer(builder.Configuration);
// ...
app.UseRedisMqInApi();
```

Config section: `RedisMq`.
