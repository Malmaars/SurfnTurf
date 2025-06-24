using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Scriptable Objects/TagRules/SubtractScore")]
[System.Serializable]
public class TagSubtractScoreRule : TagRule
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
        return -localValue;
    }

    public override bool DoesHaveRuleInteraction(FoodCell interaction)
    {
        if (interaction.cellScore.mainTag == appliedRuleTag)
            return true;
        else
            return false;
    }
}
