using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Scriptable Objects/TagRules/SubtractScore")]
[System.Serializable]
public class TagSubtractScoreRule : TagRule
{
    public override int Calculate(List<FoodCell> neighbours, FoodCell owner)
    {
        int value = 0;
        foreach (FoodCell cell in neighbours)
        {
            if (cell.cellScore.mainTag == appliedRuleTag)
            {
                value += value;
            }
        }
        return -value;
    }

    public override bool DoesHaveRuleInteraction(FoodCell interaction)
    {
        if (interaction.cellScore.mainTag == appliedRuleTag)
            return true;
        else
            return false;
    }
}
