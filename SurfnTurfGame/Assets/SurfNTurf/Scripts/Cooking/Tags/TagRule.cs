using System.Collections.Generic;
using UnityEngine;

public class TagRule : ScriptableObject
{
    public virtual int Calculate(List<FoodCell> neighbours, FoodCell owner)
    {
        return 0;
    }

    public virtual bool DoesHaveRuleInteraction(FoodCell interaction)
    {
        return false;
    }
}
