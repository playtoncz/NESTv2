namespace NestCore.Model;

/// <summary>Interval hodnot (min, max) pro váhy a neurčitost.</summary>
public sealed class Interval
{
    public double Min { get; set; }
    public double Max { get; set; }

    public Interval() { }

    public Interval(double min, double max)
    {
        Min = min;
        Max = max;
    }

    public Interval(Interval other)
    {
        Min = other.Min;
        Max = other.Max;
    }

    public void Set(double min, double max)
    {
        Min = min;
        Max = max;
    }

    public void Set(Interval other)
    {
        Min = other.Min;
        Max = other.Max;
    }
}
