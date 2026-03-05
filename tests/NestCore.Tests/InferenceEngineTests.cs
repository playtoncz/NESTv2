using NestCore.Inference;
using NestCore.Model;

namespace NestCore.Tests;

public class InferenceEngineTests
{
    private static KnowledgeBase MinimalKb()
    {
        var kb = new KnowledgeBase
        {
            Global = new Global { DefaultWeight = DefaultWeightKind.Irrelevant }
        };
        kb.Attributes.Add(new NestCore.Model.Attribute
        {
            Id = "a",
            Type = AttributeType.Binary,
            Propositions = new List<Proposition>()
        });
        kb.Attributes.Add(new NestCore.Model.Attribute
        {
            Id = "goal",
            Type = AttributeType.Single,
            Propositions = new List<Proposition> { new() { Id = "G", Name = "Goal" } }
        });
        kb.CompositionalRules.Add(new CompositionalRule
        {
            Id = "r1",
            Condition = new Condition
            {
                Conjunctions = new List<Conjunction>
                {
                    new() { Literals = new List<Literal> { new() { AttributeId = "a", PropositionId = null, Negation = false } } }
                }
            },
            Conclusions = new List<Conclusion> { new() { AttributeId = "goal", PropositionId = "G", Weight = 1 } }
        });
        return kb;
    }

    [Fact]
    public void Run_SingleRuleWithBinaryCondition_FiresAndUpdatesGoal()
    {
        var kb = MinimalKb();
        var answers = new AnswerSet
        {
            Attributes = new List<AttributeAnswer>
            {
                new() { Id = "a", Type = AttributeType.Binary, Answers = new List<Answer> { new() { Value = "yes", Weight = 1 } } }
            }
        };

        var engine = new InferenceEngine();
        var result = engine.Run(kb, answers);

        var g = result.Goals.FirstOrDefault(x => x.AttributeId == "goal" && x.PropositionId == "G");
        Assert.NotNull(g);
        Assert.True(g.MaxWeight > 0);
        Assert.Single(result.FiredRules);
        Assert.Equal("r1", result.FiredRules[0].RuleId);
    }

    [Fact]
    public void Run_EmptyAnswers_GoalHasDefaultScore()
    {
        var kb = MinimalKb();
        var answers = new AnswerSet();
        kb.Global.DefaultWeight = DefaultWeightKind.Irrelevant; // 0,0

        var engine = new InferenceEngine();
        var result = engine.Run(kb, answers);

        Assert.NotNull(result.Goals);
        var g = result.Goals.FirstOrDefault(x => x.AttributeId == "goal" && x.PropositionId == "G");
        Assert.NotNull(g);
        Assert.True(g.MaxWeight >= -1 && g.MaxWeight <= 1);
    }

    [Fact]
    public void Run_TwoLiteralConjunction_BothMustBeSatisfied()
    {
        var kb = new KnowledgeBase { Global = new Global { DefaultWeight = DefaultWeightKind.Irrelevant } };
        kb.Attributes.Add(new NestCore.Model.Attribute { Id = "a", Type = AttributeType.Binary, Propositions = new List<Proposition>() });
        kb.Attributes.Add(new NestCore.Model.Attribute { Id = "b", Type = AttributeType.Binary, Propositions = new List<Proposition>() });
        kb.Attributes.Add(new NestCore.Model.Attribute
        {
            Id = "goal",
            Type = AttributeType.Single,
            Propositions = new List<Proposition> { new() { Id = "G" } }
        });
        kb.CompositionalRules.Add(new CompositionalRule
        {
            Id = "r1",
            Condition = new Condition
            {
                Conjunctions = new List<Conjunction>
                {
                    new()
                    {
                        Literals = new List<Literal>
                        {
                            new() { AttributeId = "a", Negation = false },
                            new() { AttributeId = "b", Negation = false }
                        }
                    }
                }
            },
            Conclusions = new List<Conclusion> { new() { AttributeId = "goal", PropositionId = "G", Weight = 1 } }
        });

        var engine = new InferenceEngine();
        var onlyA = new AnswerSet
        {
            Attributes = new List<AttributeAnswer> { new() { Id = "a", Type = AttributeType.Binary, Answers = new List<Answer> { new() { Value = "yes", Weight = 1 } } } }
        };
        var resultOnlyA = engine.Run(kb, onlyA);
        var gOnlyA = resultOnlyA.Goals.FirstOrDefault(x => x.AttributeId == "goal");
        Assert.NotNull(gOnlyA);
        Assert.True(gOnlyA.MaxWeight <= 0, "Only 'a' set: condition (a AND b) should not fire");

        var both = new AnswerSet
        {
            Attributes = new List<AttributeAnswer>
            {
                new() { Id = "a", Type = AttributeType.Binary, Answers = new List<Answer> { new() { Value = "yes", Weight = 1 } } },
                new() { Id = "b", Type = AttributeType.Binary, Answers = new List<Answer> { new() { Value = "yes", Weight = 1 } } }
            }
        };
        var resultBoth = engine.Run(kb, both);
        var gBoth = resultBoth.Goals.FirstOrDefault(x => x.AttributeId == "goal" && x.PropositionId == "G");
        Assert.NotNull(gBoth);
        Assert.True(gBoth.MaxWeight > 0);
        Assert.Single(resultBoth.FiredRules);
    }

    [Fact]
    public void Run_NumericAttributeWithFuzzy_SpreadsWeightToPropositions()
    {
        var kb = new KnowledgeBase { Global = new Global { DefaultWeight = DefaultWeightKind.Irrelevant } };
        kb.Attributes.Add(new NestCore.Model.Attribute
        {
            Id = "temp",
            Type = AttributeType.Numeric,
            Propositions = new List<Proposition>
            {
                new() { Id = "low", WeightFunction = new FuzzyBounds { FuzzyLower = 0, CrispLower = 10, CrispUpper = 20, FuzzyUpper = 30 } },
                new() { Id = "high", WeightFunction = new FuzzyBounds { FuzzyLower = 20, CrispLower = 30, CrispUpper = 40, FuzzyUpper = 50 } }
            }
        });
        kb.Attributes.Add(new NestCore.Model.Attribute
        {
            Id = "goal",
            Type = AttributeType.Single,
            Propositions = new List<Proposition> { new() { Id = "G" } }
        });
        kb.CompositionalRules.Add(new CompositionalRule
        {
            Id = "r1",
            Condition = new Condition
            {
                Conjunctions = new List<Conjunction>
                {
                    new() { Literals = new List<Literal> { new() { AttributeId = "temp", PropositionId = "low", Negation = false } } }
                }
            },
            Conclusions = new List<Conclusion> { new() { AttributeId = "goal", PropositionId = "G", Weight = 1 } }
        });

        var answers = new AnswerSet
        {
            Attributes = new List<AttributeAnswer>
            {
                new() { Id = "temp", Type = AttributeType.Numeric, Answers = new List<Answer> { new() { Value = "15", Weight = 1 } } }
            }
        };
        var engine = new InferenceEngine();
        var result = engine.Run(kb, answers);
        var g = result.Goals.FirstOrDefault(x => x.AttributeId == "goal" && x.PropositionId == "G");
        Assert.NotNull(g);
        Assert.True(g.MaxWeight > 0);
        Assert.Single(result.FiredRules);
    }
}
