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
