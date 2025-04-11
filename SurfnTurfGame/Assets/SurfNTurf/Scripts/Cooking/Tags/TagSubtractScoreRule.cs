using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/TagRules/SubtractScore")]
public class TagSubtractScoreRule : TagRule
{
    public override int Calculate(int value)
    {
        return -value;
    }
}
