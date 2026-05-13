using NestCore.Model;
using Attr = NestCore.Model.Attribute;

namespace NestCore.Tests;

public class AnswerAttributeIdMatchingTests
{
    [Fact]
    public void AnswerIdMatchesKbAttribute_ByCanonicalId()
    {
        var attr = new Attr { Id = "tlusté prsty", Name = "tlustoprst" };
        Assert.True(AnswerAttributeIdMatching.AnswerIdMatchesKbAttribute("tlusté prsty", attr));
    }

    [Fact]
    public void AnswerIdMatchesKbAttribute_ByDisplayName()
    {
        var attr = new Attr { Id = "tlusté prsty", Name = "tlustoprst" };
        Assert.True(AnswerAttributeIdMatching.AnswerIdMatchesKbAttribute("tlustoprst", attr));
        Assert.True(AnswerAttributeIdMatching.AnswerIdMatchesKbAttribute("TLUSTOPRST", attr));
    }

    [Fact]
    public void FindKbAttributeForAnswer_PrefersFirstMatch()
    {
        var list = new List<Attr>
        {
            new() { Id = "other", Name = "x" },
            new() { Id = "tlusté prsty", Name = "tlustoprst" }
        };
        var found = AnswerAttributeIdMatching.FindKbAttributeForAnswer(list, "tlustoprst");
        Assert.NotNull(found);
        Assert.Equal("tlusté prsty", found!.Id);
    }
}
