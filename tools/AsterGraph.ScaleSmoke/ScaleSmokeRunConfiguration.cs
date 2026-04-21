namespace AsterGraph.ScaleSmoke;

public sealed record ScaleSmokeRunConfiguration(
    ScaleSmokeTier Tier,
    int Samples)
{
    public static ScaleSmokeRunConfiguration Parse(string[] args)
    {
        var tier = ScaleSmokeTier.Parse(args);
        var samples = 1;

        for (var index = 0; index < args.Length; index++)
        {
            if (!string.Equals(args[index], "--samples", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (index + 1 >= args.Length || !int.TryParse(args[index + 1], out samples) || samples <= 0)
            {
                throw new ArgumentException("The '--samples' argument requires a positive integer value.");
            }

            index++;
        }

        return new ScaleSmokeRunConfiguration(tier, samples);
    }
}
