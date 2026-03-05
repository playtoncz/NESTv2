using NestCore.Model;
using NestFormat;

namespace NestFormat.Tests;

public class ResultXmlTests
{
    [Fact]
    public void Write_InferenceResult_ProducesValidXmlWithGoals()
    {
        var result = new InferenceResult
        {
            Goals =
            {
                new GoalScore
                {
                    AttributeId = "diagnoza",
                    PropositionId = "TBC",
                    DisplayName = "TBC",
                    MinWeight = 0.5,
                    MaxWeight = 0.8,
                    Status = "evaluated",
                    Type = "goal"
                },
                new GoalScore
                {
                    AttributeId = "teplota",
                    PropositionId = null,
                    DisplayName = "teplota",
                    MinWeight = -0.2,
                    MaxWeight = 0.3,
                    Status = "evaluated",
                    Type = "goal"
                }
            }
        };

        var writer = new ResultXmlWriter();
        var xml = writer.Write(result);

        Assert.Contains("<?xml", xml);
        Assert.Contains("encoding=\"utf-8\"", xml);
        Assert.Contains("<results>", xml);
        Assert.Contains("<goals>", xml);
        Assert.Contains("<goal>", xml);
        Assert.Contains("diagnoza", xml);
        Assert.Contains("TBC", xml);
        Assert.Contains("0.5", xml);
        Assert.Contains("0.8", xml);
        Assert.Contains("</results>", xml);
    }

    [Fact]
    public void Write_InferenceResultWithFiredRules_IncludesExplainability()
    {
        var result = new InferenceResult
        {
            Goals = { new GoalScore { AttributeId = "x", DisplayName = "X", MinWeight = 0.5, MaxWeight = 0.5, Status = "evaluated", Type = "goal" } },
            FiredRules =
            {
                new FiredRuleEntry
                {
                    RuleId = "c1",
                    ConditionScore = 0.9,
                    AppliedConclusions =
                    {
                        new AppliedConclusion { AttributeId = "diagnoza", PropositionId = "TBC", WeightChange = 0.45 }
                    }
                }
            }
        };

        var writer = new ResultXmlWriter();
        var xml = writer.Write(result);

        Assert.Contains("<fired_rules>", xml);
        Assert.Contains("<fired_rule>", xml);
        Assert.Contains("c1", xml);
        Assert.Contains("0.9", xml);
        Assert.Contains("<applied_conclusion>", xml);
        Assert.Contains("weight_change", xml);
        Assert.Contains("0.45", xml);
    }

    [Fact]
    public void Write_EmptyResult_ProducesValidXml()
    {
        var result = new InferenceResult();
        var writer = new ResultXmlWriter();
        var xml = writer.Write(result);

        Assert.Contains("<results>", xml);
        Assert.True(xml.Contains("<goals>") || xml.Contains("<goals />"), "Expected goals element");
        Assert.DoesNotContain("<fired_rules>", xml);
    }
}
