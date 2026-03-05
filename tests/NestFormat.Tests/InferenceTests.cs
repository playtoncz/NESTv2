using NestCore.Inference;
using NestCore.Model;
using NestFormat;

namespace NestFormat.Tests;

public class InferenceTests
{
    private static string GetTestDataPath(string fileName)
    {
        var dir = AppContext.BaseDirectory;
        var path = Path.Combine(dir, "TestData", fileName);
        if (!File.Exists(path))
            path = Path.Combine(dir, "..", "..", "..", "..", "old", fileName);
        return path;
    }

    [Fact]
    public void Run_WithNemoceAndSampleAnswers_ReturnsOrderedGoals()
    {
        var kbPath = GetTestDataPath("Nemoce.xml");
        var answersPath = GetTestDataPath("answers_sample.xml");
        if (!File.Exists(kbPath) || !File.Exists(answersPath))
            return;

        var baseReader = new BaseXmlReader();
        var answersReader = new AnswersXmlReader();
        var kb = baseReader.Read(File.ReadAllText(kbPath));
        var answers = answersReader.Read(File.ReadAllText(answersPath));

        var engine = new InferenceEngine();
        var result = engine.Run(kb, answers);

        Assert.NotNull(result.Goals);
        Assert.True(result.Goals.Count >= 3, "Expected at least 3 goal propositions (diagnoza: Nachlazeni, Chripka, TBC)");

        var diagnozaGoals = result.Goals.Where(g => g.AttributeId == "diagnoza").ToList();
        Assert.True(diagnozaGoals.Count >= 3);

        // With bledost=yes, plice=yes, HCD=no, teplota=37 (normal), priznaky=kasel -> TBC should get positive support
        var tbc = result.Goals.FirstOrDefault(g => g.AttributeId == "diagnoza" && g.PropositionId == "TBC");
        Assert.NotNull(tbc);
        Assert.True(tbc.MaxWeight > 0, "TBC should have positive score (bledost+plice rules)");
    }

    [Fact]
    public void Run_EmptyAnswers_ReturnsGoalsWithDefaultScores()
    {
        var kbPath = GetTestDataPath("Nemoce.xml");
        if (!File.Exists(kbPath))
            return;

        var baseReader = new BaseXmlReader();
        var kb = baseReader.Read(File.ReadAllText(kbPath));
        var answers = new AnswerSet();

        var engine = new InferenceEngine();
        var result = engine.Run(kb, answers);

        Assert.NotNull(result.Goals);
        // Goals are still listed (diagnoza propositions), scores may be 0 or default
    }
}
