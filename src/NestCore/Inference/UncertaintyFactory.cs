using NestCore.Model;

namespace NestCore.Inference;

public static class UncertaintyFactory
{
    public static IUncertainty Create(UncertaintyType type)
    {
        // Zatím všechny typy používají stejnou implementaci jako Standard.
        // Pokud budou k dispozici přesné definice pro ostatní typy, lze je zde snadno doplnit.
        return new UncertaintyStandard(0.999);
    }

    public static UncertaintyType FromString(string? mechanism)
    {
        return mechanism?.ToLowerInvariant() switch
        {
            "logical" => UncertaintyType.Logical,
            "neural" => UncertaintyType.Neural,
            "hybrid" => UncertaintyType.Hybrid,
            "godel" => UncertaintyType.Godel,
            "product" => UncertaintyType.Product,
            _ => UncertaintyType.Standard
        };
    }
}

