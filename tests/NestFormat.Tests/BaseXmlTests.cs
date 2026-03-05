using NestCore.Model;
using NestFormat;

namespace NestFormat.Tests;

public class BaseXmlTests
{
    private static string GetNemoceXmlPath()
    {
        var dir = AppContext.BaseDirectory;
        var path = Path.Combine(dir, "TestData", "Nemoce.xml");
        if (!File.Exists(path))
            path = Path.Combine(dir, "..", "..", "..", "..", "old", "Nemoce.xml");
        return path;
    }

    [Fact]
    public void Read_NemoceXml_LoadsKnowledgeBase()
    {
        var path = GetNemoceXmlPath();
        if (!File.Exists(path))
        {
            // Skip if repo layout doesn't have old/Nemoce.xml
            return;
        }
        var xml = File.ReadAllText(path);
        var reader = new BaseXmlReader();
        var kb = reader.Read(xml);

        Assert.NotNull(kb.Global);
        Assert.Equal("standard", kb.Global.InferenceMechanism);
        Assert.Equal(3, kb.Global.WeightRange);
        Assert.Equal(DefaultWeightKind.Irrelevant, kb.Global.DefaultWeight);

        Assert.Equal(7, kb.Attributes.Count);
        var diagnoza = kb.Attributes.FirstOrDefault(a => a.Id == "diagnoza");
        Assert.NotNull(diagnoza);
        Assert.Equal(AttributeType.Multiple, diagnoza.Type);
        Assert.Equal(3, diagnoza.Propositions.Count);

        var teplota = kb.Attributes.FirstOrDefault(a => a.Id == "teplota");
        Assert.NotNull(teplota);
        Assert.Equal(AttributeType.Numeric, teplota.Type);
        Assert.NotNull(teplota.LegalValues);
        Assert.Equal(35, teplota.LegalValues.LowerBound);
        Assert.Equal(42, teplota.LegalValues.UpperBound);
        Assert.Equal(2, teplota.Propositions.Count);
        var normalni = teplota.Propositions.FirstOrDefault(p => p.Id == "normalni");
        Assert.NotNull(normalni?.WeightFunction);
        Assert.Equal(37.5, normalni.WeightFunction.FuzzyUpper);

        Assert.Equal(14, kb.CompositionalRules.Count);
        var c1 = kb.CompositionalRules.FirstOrDefault(r => r.Id == "c1");
        Assert.NotNull(c1);
        Assert.Single(c1.Condition.Conjunctions);
        Assert.Single(c1.Condition.Conjunctions[0].Literals);
        Assert.Equal("bledost", c1.Condition.Conjunctions[0].Literals[0].AttributeId);
        Assert.Single(c1.Conclusions);
        Assert.Equal("diagnoza", c1.Conclusions[0].AttributeId);
        Assert.Equal("TBC", c1.Conclusions[0].PropositionId);
        Assert.Equal(1, c1.Conclusions[0].Weight);
    }

    [Fact]
    public void Roundtrip_NemoceXml_PreservesData()
    {
        var path = GetNemoceXmlPath();
        if (!File.Exists(path))
            return;
        var xml = File.ReadAllText(path);
        var reader = new BaseXmlReader();
        var writer = new BaseXmlWriter();
        var kb = reader.Read(xml);
        var xml2 = writer.Write(kb);
        var kb2 = reader.Read(xml2);

        Assert.Equal(kb.Attributes.Count, kb2.Attributes.Count);
        Assert.Equal(kb.CompositionalRules.Count, kb2.CompositionalRules.Count);
        Assert.Equal(kb.Global.InferenceMechanism, kb2.Global.InferenceMechanism);
        var d2 = kb2.Attributes.FirstOrDefault(a => a.Id == "diagnoza");
        Assert.NotNull(d2);
        Assert.Equal(AttributeType.Multiple, d2.Type);
        Assert.Equal(3, d2.Propositions.Count);
    }
}
