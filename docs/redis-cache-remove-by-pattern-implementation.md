# Redis `RemoveByPatternAsync` — local SDK wiring (apply manually)

Plan mode blocked direct `.cs` edits. Apply the following in Agent mode, or copy the changes yourself.

## 1) Bump package version

**File:** `QFace.Sdk/src/QFace.Sdk.RedisCache/QFace.Sdk.RedisCache.csproj`

- Change `<Version>1.0.0</Version>` to `<Version>1.0.1</Version>`.

## 2) Use local SDK from `QimErp.Shared.Common` (no stale NuGet)

**File:** `QFace.Sdk/src/QimErp.Shared.Common/QimErp.Shared.Common.csproj`

- Remove: `<PackageReference Include="QFace.Sdk.RedisCache" Version="1.0.0" />`
- Add next to other project refs:

```xml
<ProjectReference Include="..\QFace.Sdk.RedisCache\QFace.Sdk.RedisCache.csproj" />
```

## 3) Call `RemoveByPatternAsync` directly (drop reflection)

**File:** `QFace.Sdk/src/QimErp.Shared.Common/Services/Cache/RedisCacheService.cs`

In `RemoveByPatternAsync(string pattern, string? region = null)`, replace the body that calls `RemoveByPatternWithFallbackAsync` with:

```csharp
var removedCount = await redisCacheService.RemoveByPatternAsync(fullPattern);
```

Delete the entire private method `RemoveByPatternWithFallbackAsync` and its comment block.

## 4) (Optional) Safer pattern delete for StackExchange

**File:** `QFace.Sdk/src/QFace.Sdk.RedisCache/Services/Providers/StackExchangeRedisProvider.cs`

`RemoveByPatternAsync` already exists. Optionally replace `server.Keys(...).ToArray()` with `await foreach` over `server.KeysAsync(_options.Database, pattern, pageSize: 500)` and delete in batches of e.g. 100 keys to reduce memory spikes on large keyspaces.

## 5) Pack and test locally

From repo root `current-sprint`:

```bash
dotnet pack QFace.Sdk/src/QFace.Sdk.RedisCache/QFace.Sdk.RedisCache.csproj -c Release -o ./local-nuget
dotnet build QFace.Sdk/src/QimErp.Shared.Common/QimErp.Shared.Common.csproj -c Release
dotnet build QimErp.CoreHr/src/Modules/People/QimErp.CoreHr.People.WebApi/QimErp.CoreHr.People.WebApi.csproj -c Release
```

If CoreHr still references `QimErp.Shared.Common` as a **NuGet** package, rebuild/pack Shared.Common and bump feed versions, or add a **ProjectReference** from `QimErp.CoreHr.Shared` to the Shared.Common **project** for local work.

## Why this fixes the bug

The published **QFace.Sdk.RedisCache 1.0.0** NuGet likely predates `RemoveByPatternAsync` on the service type, so reflection in `RedisCacheService` returned `null`. Wiring **ProjectReference** to the current SDK source and calling **`IRedisCacheService.RemoveByPatternAsync` directly** ensures pattern purge runs and HR org-unit page cache keys are deleted after mutations.
