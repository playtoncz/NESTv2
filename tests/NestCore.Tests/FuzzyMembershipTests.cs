using NestCore.Inference;
using NestCore.Model;

namespace NestCore.Tests;

public class FuzzyMembershipTests
{
    [Fact]
    public void ComputeWeight_InsideCrispRange_ReturnsOne()
    {
        var bounds = new FuzzyBounds { FuzzyLower = 0, CrispLower = 10, CrispUpper = 20, FuzzyUpper = 30 };
        Assert.Equal(1, FuzzyMembership.ComputeWeight(10, bounds));
        Assert.Equal(1, FuzzyMembership.ComputeWeight(15, bounds));
        Assert.Equal(1, FuzzyMembership.ComputeWeight(20, bounds));
    }

    [Fact]
    public void ComputeWeight_BelowFuzzyLower_ReturnsMinusOne()
    {
        var bounds = new FuzzyBounds { FuzzyLower = 0, CrispLower = 10, CrispUpper = 20, FuzzyUpper = 30 };
        Assert.Equal(-1, FuzzyMembership.ComputeWeight(-1, bounds));
        Assert.Equal(-1, FuzzyMembership.ComputeWeight(0, bounds));
    }

    [Fact]
    public void ComputeWeight_AboveFuzzyUpper_ReturnsMinusOne()
    {
        var bounds = new FuzzyBounds { FuzzyLower = 0, CrispLower = 10, CrispUpper = 20, FuzzyUpper = 30 };
        Assert.Equal(-1, FuzzyMembership.ComputeWeight(30, bounds));
        Assert.Equal(-1, FuzzyMembership.ComputeWeight(31, bounds));
    }

    [Fact]
    public void ComputeWeight_InRampBelowCrisp_ReturnsLinear()
    {
        // fl=0, cl=10: at 5 weight = (2*5/10)-1 = 0
        var bounds = new FuzzyBounds { FuzzyLower = 0, CrispLower = 10, CrispUpper = 20, FuzzyUpper = 30 };
        Assert.Equal(0, FuzzyMembership.ComputeWeight(5, bounds));
        // at 0 -> (0)-1 = -1, at 10 -> 1
        Assert.Equal(-1, FuzzyMembership.ComputeWeight(0, bounds));
        Assert.Equal(1, FuzzyMembership.ComputeWeight(10, bounds));
    }

    [Fact]
    public void ComputeWeight_InRampAboveCrisp_ReturnsLinear()
    {
        // cu=20, fu=30: at 25 weight = (2*(30-25)/(30-20))-1 = 1-1 = 0
        var bounds = new FuzzyBounds { FuzzyLower = 0, CrispLower = 10, CrispUpper = 20, FuzzyUpper = 30 };
        Assert.Equal(0, FuzzyMembership.ComputeWeight(25, bounds));
        Assert.Equal(1, FuzzyMembership.ComputeWeight(20, bounds));
        Assert.Equal(-1, FuzzyMembership.ComputeWeight(30, bounds));
    }
}
