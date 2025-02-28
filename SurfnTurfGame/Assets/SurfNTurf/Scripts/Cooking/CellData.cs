using UnityEngine;

[CreateAssetMenu(fileName = "CellData", menuName = "Scriptable Objects/CellData")]
public class CellData : ScriptableObject
{
    public int id;
    public string cellName;
    public Color color;
    //More Properties if needed
}
