using NestCore.Inference;
using NestCore.Model;
using NestFormat;

namespace NestStudio;

internal static class ConsoleRunner
{
    public static int Run(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("NEST Studio – použití:");
            Console.WriteLine("  NestStudio <cesta_k_base.xml> [cesta_k_answers.xml]");
            Console.WriteLine("");
            Console.WriteLine("Příklad: NestStudio old/Nemoce.xml answers.xml");
            return 0;
        }

        var basePath = args[0];
        var answersPath = args.Length > 1 ? args[1] : null;

        if (!File.Exists(basePath))
        {
            Console.WriteLine($"Soubor nenalezen: {basePath}");
            return 1;
        }

        var baseXml = File.ReadAllText(basePath);
        var reader = new BaseXmlReader();
        var kb = reader.Read(baseXml);

        Console.WriteLine($"Načtena znalostní báze: {kb.Global.Description ?? basePath}");
        Console.WriteLine($"  Atributy: {kb.Attributes.Count}, Pravidla: {kb.CompositionalRules.Count}");
        Console.WriteLine();

        AnswerSet answers;
        if (!string.IsNullOrEmpty(answersPath) && File.Exists(answersPath))
        {
            var answersReader = new AnswersXmlReader();
            answers = answersReader.Read(File.ReadAllText(answersPath));
            Console.WriteLine($"Načteny odpovědi: {answersPath}");
        }
        else
        {
            answers = new AnswerSet();
            Console.WriteLine("Bez odpovědí (prázdná sada).");
        }

        Console.WriteLine();
        var engine = new InferenceEngine();
        var result = engine.Run(kb, answers);

        Console.WriteLine("Výsledky (cíle seřazené podle skóre):");
        Console.WriteLine(new string('-', 60));
        foreach (var g in result.Goals)
        {
            var name = g.DisplayName;
            if (string.IsNullOrEmpty(name))
                name = g.PropositionId != null ? $"{g.AttributeId}({g.PropositionId})" : g.AttributeId;
            Console.WriteLine($"  {name}: Min={g.MinWeight:F3}, Max={g.MaxWeight:F3}");
        }
        if (result.FiredRules.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine($"Aktivovaná pravidla: {result.FiredRules.Count}");
        }
        Console.WriteLine();
        return 0;
    }
}
