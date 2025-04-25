using UnityEngine;

public class DeleteSave : MonoBehaviour
{
    public void DeletePlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
        Debug.Log("PlayerPrefs deleted.");
    }
}
