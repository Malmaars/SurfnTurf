using UnityEngine;

public class DeleteSave : MonoBehaviour
{
    public void DeletePlayerPrefs()
    {
        CloudSaveSystem.Instance.DeleteSave();
    }
}
