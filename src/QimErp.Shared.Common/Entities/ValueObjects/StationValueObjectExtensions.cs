using QimErp.Shared.Common.Entities;

namespace QimErp.Shared.Common.Entities.ValueObjects;

public static class StationValueObjectExtensions
{
    /// <summary>
    /// Converts a StationBase to StationValueObject
    /// </summary>
    public static StationValueObject ToValueObject(this StationBase station)
    {
        if (station == null)
            throw new ArgumentNullException(nameof(station));

        return new StationValueObject(
            station.Id,
            station.Name,
            station.Code);
    }

    /// <summary>
    /// Creates a StationValueObject from individual properties
    /// </summary>
    public static StationValueObject ToValueObject(
        Guid id,
        string name,
        string? code = null)
    {
        return new StationValueObject(
            id,
            name,
            code);
    }
}
