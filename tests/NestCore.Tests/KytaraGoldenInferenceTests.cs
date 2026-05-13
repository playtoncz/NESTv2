using System.IO;
using System.Linq;
using NestCore.Inference;
using NestFormat;

namespace NestCore.Tests;

public class KytaraGoldenInferenceTests
{
    private static string TestDataPath(string name)
    {
        var p = Path.Combine(AppContext.BaseDirectory, "TestData", name);
        if (File.Exists(p))
            return p;
        return ResolveUnderRepo("tests", name);
    }

    private static string ResolveUnderRepo(params string[] segments)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(new[] { dir.FullName }.Concat(segments).ToArray());
            if (File.Exists(candidate))
                return candidate;
            dir = dir.Parent;
        }

        throw new InvalidOperationException("Nepodařilo se najít testovací soubor: " + Path.Combine(segments));
    }

    /// <summary>
    /// Vstupy z tests/kytara-nylon-konzult.txt; referenční váhy cílů ze starého NEST (kytara-all-levels.nkb).
    /// </summary>
    [Fact]
    public void KytaraNylonConsultation_GoalWeights_MatchLegacyNest()
    {
        var kbPath = TestDataPath("kytara-all-levels.xml");
        var answersPath = TestDataPath("kytara-nylon-konzult.txt");

        var kb = new BaseXmlReader().Read(XmlFileEncoding.ReadAllText(kbPath));
        var answers = new AnswersXmlReader().Read(XmlFileEncoding.ReadAllText(answersPath));

        var result = new InferenceEngine().Run(kb, answers);

        double MaxFor(string propId)
        {
            var g = result.Goals.FirstOrDefault(x => x.AttributeId == "kytara" && x.PropositionId == propId);
            Assert.NotNull(g);
            return g!.MaxWeight;
        }

        const double tol = 0.03;
        // Referenční váhy ze starého NEST (export výsledků / tabulka odvozování, kytara-all-levels).
        Assert.Equal(0.615, MaxFor("akustická nylon"), tol);
        Assert.Equal(-0.437, MaxFor("elektroakustická kov"), tol);
        Assert.Equal(-0.474, MaxFor("elektrická kov"), tol);
        Assert.Equal(-0.481, MaxFor("akustická kov"), tol);
    }
}
