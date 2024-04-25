namespace LocalUtilities.GdiUtilities;

public class RandomUniformPointsGeneration : RandomPointsGeneration
{
    protected override double GetNextRandomValue(Random random, double min, double max)
    {
        double value;
        do
        {
            value = min + random.NextDouble() * (max - min);
        } while (!(value > min && value < max));
        return value;
    }
}