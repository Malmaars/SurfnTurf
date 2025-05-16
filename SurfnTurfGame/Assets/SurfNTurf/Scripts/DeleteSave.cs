using UnityEngine;

public class DeleteSave : MonoBehaviour
{
    public void DeletePlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        CloudSaveSystem.Instance.DeleteSave();
    }
}
