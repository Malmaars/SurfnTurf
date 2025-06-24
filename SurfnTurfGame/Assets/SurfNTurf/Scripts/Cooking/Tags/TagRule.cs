using System.Collections.Generic;
using UnityEngine;

public class TagRule : ScriptableObject
{
    public int value;
    public string description;
    public CellTag appliedRuleTag;
    public Color valueColor = Color.white;
    public virtual string GetDescription()
    {
        string valueHex = ColorUtility.ToHtmlStringRGB(valueColor);
        string tagHex = ColorUtility.ToHtmlStringRGB(appliedRuleTag.tagColor);
        return $"{description}<color=#{valueHex}>{value}</color> for every adjacent <color=#{tagHex}>{appliedRuleTag.tagName}</color>";
    }
    public virtual int Calculate(List<FoodCell> neighbours, FoodCell owner)
    {
        return 0;
    }

    public virtual bool DoesHaveRuleInteraction(FoodCell interaction)
    {
        return false;
    }
}
