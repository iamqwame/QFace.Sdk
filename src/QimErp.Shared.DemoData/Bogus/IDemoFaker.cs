using Bogus;

namespace QimErp.Shared.DemoData.Bogus;

/// <summary>
/// Facade over a <see cref="Faker"/> instance with a deterministic seed. Hides the raw
/// Bogus type from consumers so we can layer our own Ghana extensions on top and swap
/// the engine without churning the surface.
/// </summary>
public interface IDemoFaker
{
    /// <summary>The underlying Bogus Faker — exposed for consumers needing rule-binding via <see cref="Faker{T}"/>.</summary>
    Faker Faker { get; }

    /// <summary>The seed used to construct this faker — round-tripped to consumers for reproducible runs.</summary>
    int Seed { get; }
}

public sealed class DemoFaker : IDemoFaker
{
    public DemoFaker(int seed)
    {
        Seed = seed;
        Faker = new Faker { Random = new Randomizer(seed) };
    }

    public Faker Faker { get; }
    public int Seed { get; }
}
