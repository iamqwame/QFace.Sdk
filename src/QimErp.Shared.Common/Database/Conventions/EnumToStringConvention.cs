using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace QimErp.Shared.Common.Database.Conventions;

/// <summary>
/// Stores CLR enums as strings unless a property already has an explicit conversion
/// (e.g. Import.Status → int via ImportConfiguration).
/// </summary>
public sealed class EnumToStringConvention : IModelFinalizingConvention
{
    public void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            foreach (var property in entityType.GetDeclaredProperties())
            {
                var clrType = property.ClrType;
                var enumType = Nullable.GetUnderlyingType(clrType) ?? clrType;
                if (!enumType.IsEnum)
                    continue;

                // Import.Status must stay int (DATA-16 exception) even if a broader
                // Properties<Enum>() string default ran later in the pipeline.
                if (entityType.ClrType == typeof(Import)
                    && property.Name == nameof(Import.Status))
                {
                    property.SetValueConverter(new EnumToNumberConverter<ImportStatus, int>());
                    continue;
                }

                var conventionProperty = (IConventionProperty)property;
                if (conventionProperty.GetValueConverterConfigurationSource()
                    is ConfigurationSource.Explicit)
                {
                    continue;
                }

                var existing = property.GetValueConverter();
                if (existing is not null && existing.ProviderClrType == typeof(string))
                    continue;

                if (existing is not null && existing.ProviderClrType != typeof(string))
                    continue;

                var converterType = typeof(EnumToStringConverter<>).MakeGenericType(enumType);
                var converter = (ValueConverter)Activator.CreateInstance(converterType)!;
                property.SetValueConverter(converter);
            }
        }
    }
}
