using UnityEngine;

public class UIManager : MonoBehaviour
{
    //Fuction that toggle the vsync and saves it in player prefs
    public void ToggleVsync(bool vsync)
    {
        QualitySettings.vSyncCount = vsync ? 1 : 0;
        PlayerPrefs.SetInt("vsync", QualitySettings.vSyncCount);
    }

    //funtion that loads the vsync from player prefs and sets the toggle
    public void LoadVsync()
    {
        int vsync = PlayerPrefs.GetInt("vsync", 0);
        QualitySettings.vSyncCount = vsync;
        GameObject.Find("Vsync").GetComponent<UnityEngine.UI.Toggle>().isOn = vsync == 1;
    }  


}
