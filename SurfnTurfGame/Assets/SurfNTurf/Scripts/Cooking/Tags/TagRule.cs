using UnityEngine;

public class TagRule : ScriptableObject
{
    public virtual int Calculate(int value)
    {
        return 0;
    }
}

/*
[CreateAssetMenu(menuName = "Scriptable Objects/TagRules/AddScore")]
public class TagAddScoreRule : TagRule
{
    public override int Calculate(int value)
    {
        return value;
    }
}

[CreateAssetMenu(menuName = "Scriptable Objects/TagRules/SubtractScore")]
public class TagSubtractScoreRule : TagRule
{
    public override int Calculate(int value)
    {
        return -value;
    }
}
*/
