using UnityEngine;

[CreateAssetMenu(fileName = "TagAddScoreRule", menuName = "Scriptable Objects/TagRules/AddScore")]
public class TagAddScoreRule : TagRule
{
    public override int Calculate(int value)
    {
        return value;
    }
}
