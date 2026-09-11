using Carter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using QimErp.Shared.Common.Entities;
using QimErp.Shared.Common.Services;
using QimErp.Shared.Common.Services.Auth;

namespace QimErp.Shared.Common.Features.EntityCodes;

public sealed class EntityCodeConfigResponse
{
    public string EntityType { get; set; } = string.Empty;
    /// <summary>Module that conceptually owns this entity type's numbering (e.g. "Payroll", "Inventory").</summary>
    public string Module { get; set; } = string.Empty;
    public string Prefix { get; set; } = string.Empty;
    public string Separator { get; set; } = string.Empty;
    public bool IncludeYear { get; set; }
    public int PaddingWidth { get; set; }
    public string Mode { get; set; } = string.Empty;
    public string ResetPeriod { get; set; } = string.Empty;
    /// <summary>Next code as it would be formatted right now — NOT reserved, advisory only.</summary>
    public string Preview { get; set; } = string.Empty;
}

public sealed class UpdateEntityCodeConfigRequest
{
    public string Prefix { get; set; } = string.Empty;
    public string Separator { get; set; } = "-";
    public bool IncludeYear { get; set; } = true;
    public int PaddingWidth { get; set; } = 4;
    public string Mode { get; set; } = nameof(CodeGenerationMode.Auto);
    public string ResetPeriod { get; set; } = nameof(CodeResetPeriod.Never);
}

/// <summary>
/// Tenant-facing settings surface over <see cref="IEntityCodeService"/> — lets an admin see and
/// edit the numbering rule (prefix/padding/reset behavior) for every entity type a module's own
/// <see cref="IEntityCodeService"/> registration knows about.
///
/// Discovered automatically by Carter's assembly scan in every consuming module (no manual
/// registration needed). Each module resolves its own concrete <see cref="IEntityCodeService"/>
/// from DI, so the same route on two different module APIs returns two different entity lists.
/// </summary>
public sealed class EntityCodeSettingsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/entity-codes", [Authorize] async (
            IEntityCodeService codeService,
            ICurrentUserService currentUserService,
            CancellationToken ct) =>
        {
            var tenantId = currentUserService.GetTenantId() ?? string.Empty;
            var configs = await codeService.GetAllConfigsAsync(tenantId, ct);
            var response = configs
                .Select(c => ToResponse(c, codeService))
                .OrderBy(c => c.EntityType, StringComparer.OrdinalIgnoreCase)
                .ToList();
            return Result.WithSuccess(response).ToIResult();
        })
        .WithTags("EntityCodes")
        .WithSummary("List this module's entity numbering configs for the current tenant");

        app.MapPut("/api/entity-codes/{entityType}", [Authorize] async (
            string entityType,
            UpdateEntityCodeConfigRequest request,
            IEntityCodeService codeService,
            ICurrentUserService currentUserService,
            CancellationToken ct) =>
        {
            if (!Enum.TryParse<CodeGenerationMode>(request.Mode, true, out var mode))
            {
                return Result.WithFailure<EntityCodeConfigResponse>(
                    new Error("EntityCodes.InvalidMode", $"Unknown mode '{request.Mode}'.")).ToIResult();
            }

            if (!Enum.TryParse<CodeResetPeriod>(request.ResetPeriod, true, out var resetPeriod))
            {
                return Result.WithFailure<EntityCodeConfigResponse>(
                    new Error("EntityCodes.InvalidResetPeriod", $"Unknown reset period '{request.ResetPeriod}'.")).ToIResult();
            }

            if (request.PaddingWidth is < 1 or > 20)
            {
                return Result.WithFailure<EntityCodeConfigResponse>(
                    new Error("EntityCodes.InvalidPaddingWidth", "PaddingWidth must be between 1 and 20.")).ToIResult();
            }

            var tenantId = currentUserService.GetTenantId() ?? string.Empty;
            if (!codeService.GetKnownEntityTypes().Contains(entityType, StringComparer.OrdinalIgnoreCase))
            {
                return Result.WithNotFound<EntityCodeConfigResponse>(
                    new Error("EntityCodes.UnknownEntityType", $"'{entityType}' is not a known entity type for this module.")).ToIResult();
            }

            await codeService.UpsertConfigAsync(
                tenantId, entityType,
                request.Prefix, request.Separator, request.IncludeYear, request.PaddingWidth,
                mode, resetPeriod, ct);

            var updated = await codeService.GetConfigAsync(tenantId, entityType, ct);
            return Result.WithSuccess(ToResponse(updated!, codeService)).ToIResult();
        })
        .WithTags("EntityCodes")
        .WithSummary("Update the numbering rule for one entity type");

        app.MapPost("/api/entity-codes/{entityType}/suggest", [Authorize] async (
            string entityType,
            IEntityCodeService codeService,
            ICurrentUserService currentUserService,
            CancellationToken ct) =>
        {
            if (!codeService.GetKnownEntityTypes().Contains(entityType, StringComparer.OrdinalIgnoreCase))
            {
                return Result.WithNotFound<EntityCodeSuggestResponse>(
                    new Error("EntityCodes.UnknownEntityType", $"'{entityType}' is not a known entity type for this module.")).ToIResult();
            }

            var tenantId = currentUserService.GetTenantId() ?? string.Empty;
            var code = await codeService.SuggestAsync(tenantId, entityType, ct);
            return Result.WithSuccess(new EntityCodeSuggestResponse { Code = code }).ToIResult();
        })
        .WithTags("EntityCodes")
        .WithSummary("Suggest the next entity code without reserving it");

        app.MapPost("/api/entity-codes/{entityType}/validate", [Authorize] async (
            string entityType,
            ValidateEntityCodeRequest request,
            IEntityCodeService codeService,
            ICurrentUserService currentUserService,
            CancellationToken ct) =>
        {
            if (!codeService.GetKnownEntityTypes().Contains(entityType, StringComparer.OrdinalIgnoreCase))
            {
                return Result.WithNotFound<EntityCodeValidationResponse>(
                    new Error("EntityCodes.UnknownEntityType", $"'{entityType}' is not a known entity type for this module.")).ToIResult();
            }

            var tenantId = currentUserService.GetTenantId() ?? string.Empty;
            var (isValid, error) = await codeService.ValidateManualAsync(tenantId, entityType, request.Code, ct);
            return Result.WithSuccess(new EntityCodeValidationResponse
            {
                IsValid = isValid,
                Error = error,
            }).ToIResult();
        })
        .WithTags("EntityCodes")
        .WithSummary("Validate a manually entered entity code against format rules");

        app.MapPost("/api/entity-codes/{entityType}/reserve", [Authorize] async (
            string entityType,
            ReserveEntityCodeRequest request,
            IEntityCodeService codeService,
            ICurrentUserService currentUserService,
            CancellationToken ct) =>
        {
            var tenantId = currentUserService.GetTenantId();
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return Result.WithSuccess(new EntityCodeReserveResponse
                {
                    Reserved = false,
                    Error = "Tenant context is required.",
                }).ToIResult();
            }

            var ttl = request.TtlSeconds is > 0
                ? TimeSpan.FromSeconds(Math.Min(request.TtlSeconds.Value, (int)EntityCodeReservation.MaxTtl.TotalSeconds))
                : EntityCodeReservation.DefaultTtl;
            var (reserved, error) = await codeService.TryReserveManualAsync(
                tenantId, entityType, request.Code, request.ReservationToken, ttl,
                validateFormat: true, metadata: null, ct);
            return Result.WithSuccess(new EntityCodeReserveResponse
            {
                Reserved = reserved,
                Error = error,
            }).ToIResult();
        })
        .WithTags("EntityCodes")
        .WithSummary("Reserve a manually entered entity code for a short TTL");

        app.MapPost("/api/entity-codes/{entityType}/release", [Authorize] async (
            string entityType,
            ReleaseEntityCodeRequest request,
            IEntityCodeService codeService,
            ICurrentUserService currentUserService,
            CancellationToken ct) =>
        {
            var tenantId = currentUserService.GetTenantId() ?? string.Empty;
            await codeService.ReleaseReservationAsync(
                tenantId, entityType, request.Code, request.ReservationToken, ct);
            return Result.WithSuccess(true).ToIResult();
        })
        .WithTags("EntityCodes")
        .WithSummary("Release a previously reserved entity code");
    }

    private static EntityCodeConfigResponse ToResponse(EntityCodeConfig config, IEntityCodeService codeService) => new()
    {
        EntityType = config.EntityType,
        Module = codeService.GetModuleFor(config.EntityType),
        Prefix = config.Prefix,
        Separator = config.Separator,
        IncludeYear = config.IncludeYear,
        PaddingWidth = config.PaddingWidth,
        Mode = config.Mode.ToString(),
        ResetPeriod = config.ResetPeriod.ToString(),
        Preview = config.FormatCode(config.LastSequence + 1),
    };
}

public sealed class ValidateEntityCodeRequest
{
    public string Code { get; set; } = string.Empty;
}

public sealed class ReserveEntityCodeRequest
{
    public string Code { get; set; } = string.Empty;
    public string ReservationToken { get; set; } = string.Empty;
    public int? TtlSeconds { get; set; }
}

public sealed class ReleaseEntityCodeRequest
{
    public string Code { get; set; } = string.Empty;
    public string ReservationToken { get; set; } = string.Empty;
}

public sealed class EntityCodeValidationResponse
{
    public bool IsValid { get; set; }
    public string? Error { get; set; }
}

public sealed class EntityCodeReserveResponse
{
    public bool Reserved { get; set; }
    public string? Error { get; set; }
}

public sealed class EntityCodeSuggestResponse
{
    public string Code { get; set; } = string.Empty;
}
