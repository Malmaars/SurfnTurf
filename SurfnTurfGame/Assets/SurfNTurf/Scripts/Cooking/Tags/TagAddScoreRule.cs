using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TagAddScoreRule", menuName = "Scriptable Objects/TagRules/AddScore")]
[System.Serializable]
public class TagAddScoreRule : TagRule
{
    public override int Calculate(List<FoodCell> neighbours, FoodCell owner)
    {
        int localValue = 0;
        foreach (FoodCell cell in neighbours)
        {
            if (cell.cellScore.mainTag == appliedRuleTag)
            {
                localValue += value;
            }
        }
        return localValue;
    }
    public override bool DoesHaveRuleInteraction(FoodCell interaction)
    {
        if (interaction.cellScore.mainTag == appliedRuleTag)
            return true;
        else
            return false;
    }
}
