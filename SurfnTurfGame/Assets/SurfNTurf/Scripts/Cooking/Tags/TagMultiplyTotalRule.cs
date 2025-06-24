using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TagMultiplyTotalRule", menuName = "Scriptable Objects/TagRules/MultiplyTotal")]
[System.Serializable]
public class TagMultiplyTotalRule : TagRule
{ 
    public override int Calculate(List<FoodCell> neighbours, FoodCell owner)
    {
        int value = 0;
        foreach (FoodCell cell in neighbours)
        {
            if (cell.cellScore.mainTag == appliedRuleTag)
            {
                value += (cell.cellScore.finalScore*value) - cell.cellScore.finalScore;
            }
        }
        return value;
    }
    public override bool DoesHaveRuleInteraction(FoodCell interaction)
    {
        if (interaction.cellScore.mainTag == appliedRuleTag)
            return true;
        else
            return false;
    }
}
