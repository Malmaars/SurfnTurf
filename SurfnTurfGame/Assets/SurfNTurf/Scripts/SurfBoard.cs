using UnityEngine;

public class SurfBoard : MonoBehaviour
{
    public string surfBoardName = "Surfboard";
    public int price;
    public SavedProperty<bool> isUnlocked;
    public bool isEquipped;

    private void Awake()
    {
        isUnlocked = new (nameof(isUnlocked) + surfBoardName, isUnlocked.Value);
    }

}


