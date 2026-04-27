namespace NestCore.Model;

/// <summary>Kaskádové přemapování ID po přejmenování atributu / výroku ve znalostní bázi.</summary>
public static class KnowledgeBaseReferenceUpdater
{
    /// <summary>Nahradí odkazy na atribut po změně ID (podmínky, závěry, kontexty, integrita).</summary>
    public static void RenameAttribute(KnowledgeBase kb, string oldAttributeId, string newAttributeId)
    {
        if (string.IsNullOrWhiteSpace(oldAttributeId) || oldAttributeId == newAttributeId)
            return;

        foreach (var rule in kb.CompositionalRules)
        {
            ReplaceLiteralsRenameAttribute(rule.Condition, oldAttributeId, newAttributeId);
            foreach (var c in rule.Conclusions)
                if (c.AttributeId == oldAttributeId)
                    c.AttributeId = newAttributeId;
        }

        foreach (var ctx in kb.Contexts)
            ReplaceLiteralsRenameAttribute(ctx.Condition, oldAttributeId, newAttributeId);

        foreach (var ic in kb.IntegrityConstraints)
        {
            ReplaceLiteralsRenameAttribute(ic.Condition, oldAttributeId, newAttributeId);
            foreach (var c in ic.Conclusions)
                if (c.AttributeId == oldAttributeId)
                    c.AttributeId = newAttributeId;
        }
    }

    /// <summary>Nahradí odkazy na výrok u daného atributu po změně ID výroku.</summary>
    public static void RenameProposition(KnowledgeBase kb, string attributeId, string oldPropositionId, string newPropositionId)
    {
        if (string.IsNullOrWhiteSpace(oldPropositionId) || oldPropositionId == newPropositionId)
            return;

        foreach (var rule in kb.CompositionalRules)
        {
            ReplaceLiteralsRenameProposition(rule.Condition, attributeId, oldPropositionId, newPropositionId);
            foreach (var c in rule.Conclusions)
            {
                if (c.AttributeId != attributeId) continue;
                if (c.PropositionId == oldPropositionId)
                    c.PropositionId = newPropositionId;
            }
        }

        foreach (var ctx in kb.Contexts)
            ReplaceLiteralsRenameProposition(ctx.Condition, attributeId, oldPropositionId, newPropositionId);

        foreach (var ic in kb.IntegrityConstraints)
        {
            ReplaceLiteralsRenameProposition(ic.Condition, attributeId, oldPropositionId, newPropositionId);
            foreach (var c in ic.Conclusions)
            {
                if (c.AttributeId != attributeId) continue;
                if (c.PropositionId == oldPropositionId)
                    c.PropositionId = newPropositionId;
            }
        }
    }

    static void ReplaceLiteralsRenameAttribute(Condition condition, string oldAttrId, string newAttrId)
    {
        foreach (var conj in condition.Conjunctions)
            foreach (var lit in conj.Literals)
                if (lit.AttributeId == oldAttrId)
                    lit.AttributeId = newAttrId;
    }

    static void ReplaceLiteralsRenameProposition(Condition condition, string attributeId, string oldPropId, string newPropId)
    {
        foreach (var conj in condition.Conjunctions)
            foreach (var lit in conj.Literals)
                if (lit.AttributeId == attributeId && lit.PropositionId == oldPropId)
                    lit.PropositionId = newPropId;
    }
}
