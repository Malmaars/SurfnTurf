using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TagMultiplyTotalRule", menuName = "Scriptable Objects/TagRules/MultiplyTotal")]
[System.Serializable]
public class TagMultiplyTotalRule : TagRule
{ 
    public override int Calculate(List<FoodCell> neighbours, FoodCell owner)
    {
        int localValue = 0;
        foreach (FoodCell cell in neighbours)
        {
            if (cell.cellScore.mainTag == appliedRuleTag)
            {
                localValue += (cell.cellScore.finalScore*value) - cell.cellScore.finalScore;
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
