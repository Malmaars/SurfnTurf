using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TagAddScoreRule", menuName = "Scriptable Objects/TagRules/AddScore")]
[System.Serializable]
public class TagAddScoreRule : TagRule
{
    public CellTag appliedRuleTag;
    public int valueToAdd;
    public override int Calculate(List<FoodCell> neighbours, FoodCell owner)
    {
        int value = 0;
        foreach (FoodCell cell in neighbours)
        {
            if (cell.cellScore.mainTag == appliedRuleTag)
            {
                value += valueToAdd;
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
