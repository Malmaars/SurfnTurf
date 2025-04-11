using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Scriptable Objects/CellTag"),System.Serializable]
public class CellTag : ScriptableObject
{
    public string tagName;
    public List<CellTagRulePair> rules;
}

[System.Serializable]
public struct CellTagRulePair
{
    public CellTag tag;
    public int value;
    public TagRule rule;
}