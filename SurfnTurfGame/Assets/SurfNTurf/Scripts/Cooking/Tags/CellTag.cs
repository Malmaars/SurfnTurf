using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Scriptable Objects/CellTag"),System.Serializable]
public class CellTag : ScriptableObject
{
    public string tagName;
    public string tagEventName;
    [TextArea(15, 20)]
    public string tagDescription;
    public List<TagRule> rules;
    [NaughtyAttributes.ShowAssetPreview(100,100)]
    public Sprite tagSymbol;
}