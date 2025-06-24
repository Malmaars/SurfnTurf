using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Scriptable Objects/CellTag"), System.Serializable]
public class CellTag : ScriptableObject
{
    public string tagName;
    private string tagDescription;
    public Color tagColor;
    public string GetTagDescription()
    {
        tagDescription = "";
        foreach (TagRule rule in rules)
        {
            tagDescription += rule.GetDescription() + "\n";
        }
        return $"{tagDescription}";
    }
    public string GetTagName()
    {
        string hex = ColorUtility.ToHtmlStringRGB(tagColor);
        return $"<color=#{hex}>{tagName}</color>";
    }
    public List<TagRule> rules;
    [NaughtyAttributes.ShowAssetPreview(100, 100)]
    public Sprite tagSymbol;
}