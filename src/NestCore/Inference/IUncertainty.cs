using System.Collections.Generic;
using NestCore.Model;

namespace NestCore.Inference;

/// <summary>Rozhraní pro různé typy neurčitosti (standardní, logická, neuronová atd.).</summary>
public interface IUncertainty
{
    Interval CTR(Interval a, Interval w);
    Interval CONJ(Interval a, Interval b);
    Interval DISJ(Interval a, Interval b);
    Interval NEG(Interval a);
    Interval NORM(Interval a);
    Interval GLOB(IReadOnlyList<Interval> intervals);
}

