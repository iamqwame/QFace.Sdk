namespace QimErp.Shared.Common.Outbox;

/// <summary>Resolves a stable <see cref="Guid"/> EntityId for outbox ordering / hashing.</summary>
public static class OutboxEntityId
{
    public static Guid FromEntity(object entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        var idProp = entity.GetType().GetProperty("Id");
        if (idProp is null)
            return Guid.CreateVersion7();

        return FromValue(idProp.GetValue(entity));
    }

    public static Guid FromValue(object? id) => id switch
    {
        Guid g => g,
        string s when Guid.TryParse(s, out var parsed) => parsed,
        long l => FromInt64(l),
        int i => FromInt64(i),
        _ => Guid.CreateVersion7(),
    };

    public static long ReadVersion(object entity)
    {
        var versionProp = entity.GetType().GetProperty("Version")
                          ?? entity.GetType().GetProperty("SourceVersion");
        if (versionProp is null) return 0;
        var value = versionProp.GetValue(entity);
        return value switch
        {
            long l => l,
            int i => i,
            _ => 0,
        };
    }

    private static Guid FromInt64(long value)
    {
        Span<byte> bytes = stackalloc byte[16];
        BitConverter.TryWriteBytes(bytes[..8], value);
        // Namespace-ish high bytes so long-derived Guids don't collide with random UUIDv7s casually.
        bytes[8] = 0x4F; // 'O'
        bytes[9] = 0x42; // 'B'
        bytes[10] = 0x58; // 'X'
        bytes[11] = 0x01;
        return new Guid(bytes);
    }
}
