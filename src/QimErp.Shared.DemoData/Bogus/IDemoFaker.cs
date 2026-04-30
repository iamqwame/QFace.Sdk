using Bogus;

namespace QimErp.Shared.DemoData.Bogus;

public interface IDemoFaker
{
    Faker Faker { get; }
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
