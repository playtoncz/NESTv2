using System.Collections.Generic;
using System.Linq;
using NestCore.Model;

namespace NestCore.Inference;

/// <summary>
/// Odvozuje ze znalostní báze a pravidel, které otázky zobrazit, v jakém pořadí
/// a kdy je zobrazit (podmíněná viditelnost podle závislosti na jiných vstupech).
/// Žádné hardcodované ID – vše z XML/pravidel.
/// </summary>
public static class QuestionAnalyzer
{
    /// <summary>
    /// Atributy, které se vyskytují v podmínkách pravidel a zároveň nejsou v závěrech (čisté vstupy k dotazování).
    /// Odvozené atributy (plice, HCD, diagnoza…) se vynechají – ty se nevyplňují, odvozuje je engine.
    /// </summary>
    public static HashSet<string> GetInputAttributeIds(KnowledgeBase kb)
    {
        var inConditions = new HashSet<string>();
        foreach (var rule in kb.CompositionalRules)
        {
            foreach (var conj in rule.Condition.Conjunctions)
            {
                foreach (var lit in conj.Literals)
                {
                    if (!string.IsNullOrEmpty(lit.AttributeId))
                        inConditions.Add(lit.AttributeId);
                }
            }
        }
        var derivedIds = GetDerivedAttributeIds(kb);
        inConditions.ExceptWith(derivedIds);
        return inConditions;
    }

    /// <summary>Atributy, které se vyskytují v závěrech pravidel (odvozené).</summary>
    public static HashSet<string> GetDerivedAttributeIds(KnowledgeBase kb)
    {
        var set = new HashSet<string>();
        foreach (var rule in kb.CompositionalRules)
        {
            foreach (var c in rule.Conclusions)
            {
                if (!string.IsNullOrEmpty(c.AttributeId))
                    set.Add(c.AttributeId);
            }
        }
        return set;
    }

    /// <summary>
    /// Vrátí atributy vhodné k dotazování (Scope=Case, v podmínkách), v pořadí odvozeném z pravidel.
    /// Pořadí: nejdřív vstupy, které nezávisí na odvozených; pak ty, které se ptají v pravidlech společně s odvozenými (až po jejich „vstupech“).
    /// </summary>
    public static List<Model.Attribute> GetInputAttributesInOrder(KnowledgeBase kb)
    {
        var inputIds = GetInputAttributeIds(kb);
        var derivedIds = GetDerivedAttributeIds(kb);
        if (inputIds.Count == 0)
            return new List<Model.Attribute>();

        var attrById = kb.Attributes.Where(a => a.Scope == ScopeKind.Case).ToDictionary(a => a.Id);
        var ordered = new List<Model.Attribute>();
        var remaining = new HashSet<string>(inputIds.Where(id => attrById.ContainsKey(id)));

        var dependsOn = new Dictionary<string, HashSet<string>>();
        foreach (var id in remaining)
        {
            var deps = new HashSet<string>(GetVisibilityDependencies(kb, id, inputIds, derivedIds));
            dependsOn[id] = deps;
        }

        while (remaining.Count > 0)
        {
            var next = remaining.FirstOrDefault(id =>
            {
                var deps = dependsOn.TryGetValue(id, out var d) ? d : null;
                if (deps == null || deps.Count == 0) return true;
                return deps.All(dep => !remaining.Contains(dep));
            });
            if (next == null)
                next = remaining.First();
            remaining.Remove(next);
            ordered.Add(attrById[next]);
        }

        return ordered;
    }

    /// <summary>
    /// Atributy (vstupní ID), které musí mít uživatel vyplněné, aby se měl zobrazit blok pro daný vstup.
    /// Prázdné = zobrazit vždy. Neprázdné = zobrazit až když aspoň jeden z těchto atributů má odpověď.
    /// </summary>
    public static List<string> GetVisibilityDependencies(KnowledgeBase kb, string inputAttributeId)
    {
        var inputIds = GetInputAttributeIds(kb);
        var derivedIds = GetDerivedAttributeIds(kb);
        return GetVisibilityDependencies(kb, inputAttributeId, inputIds, derivedIds);
    }

    private static List<string> GetVisibilityDependencies(
        KnowledgeBase kb,
        string inputAttributeId,
        HashSet<string> inputIds,
        HashSet<string> derivedIds)
    {
        var result = new HashSet<string>();
        foreach (var rule in kb.CompositionalRules)
        {
            foreach (var conj in rule.Condition.Conjunctions)
            {
                var conditionAttrIds = new HashSet<string>();
                foreach (var lit in conj.Literals)
                {
                    if (!string.IsNullOrEmpty(lit.AttributeId))
                        conditionAttrIds.Add(lit.AttributeId);
                }
                if (!conditionAttrIds.Contains(inputAttributeId))
                    continue;

                foreach (var otherId in conditionAttrIds)
                {
                    if (otherId == inputAttributeId) continue;
                    if (!derivedIds.Contains(otherId)) continue;

                    foreach (var r in kb.CompositionalRules)
                    {
                        foreach (var c in r.Conclusions)
                        {
                            if (c.AttributeId != otherId) continue;
                            foreach (var conj2 in r.Condition.Conjunctions)
                            {
                                foreach (var lit2 in conj2.Literals)
                                {
                                    if (!string.IsNullOrEmpty(lit2.AttributeId) && inputIds.Contains(lit2.AttributeId))
                                        result.Add(lit2.AttributeId);
                                }
                            }
                        }
                    }
                }
            }
        }
        return result.ToList();
    }
}
