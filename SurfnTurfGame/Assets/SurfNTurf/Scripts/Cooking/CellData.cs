using UnityEngine;

[CreateAssetMenu(fileName = "CellData", menuName = "Scriptable Objects/CellData")]
public class CellData : ScriptableObject
{
    public int id;
    public string name;
    public Color color;
    //More Properties if needed
}
