using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CellData", menuName = "Scriptable Objects/CellData")]
public class CellData : ScriptableObject
{
    [Header("Type Settings")]
    public int id;
    public string cellName;

    [Header("Visual Settings")]
    public Color color;
    public Texture cellTexture;

    [Header("Translation Settings")]
    public int maxBakedStage;

    [Header("Score Settings")]
    public int baseScore;

    public CellTag mainTag;
    public List<CellTag> subTags;
    //More Properties if needed
}
