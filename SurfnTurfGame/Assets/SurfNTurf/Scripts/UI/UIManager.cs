using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public GameObject CookingHud;
    public Canvas[] canvases;
    public GameObject[] objects;
    public GameObject hudCoin;
    public TMP_Text coinCounter;

    private void Awake()
    {
        instance = this;
    }
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
        if (GameObject.Find("Vsync") != null)
            GameObject.Find("Vsync").GetComponent<UnityEngine.UI.Toggle>().isOn = vsync == 1;
    }
    private void Start()
    {
        LoadVsync();
    }

    public void SetFocus(GameObject focusObject)
    {
        EventSystem.current.SetSelectedGameObject(focusObject);
    }

    public void SetVisibleUI(bool visible = true)
    {
        foreach (Canvas canvas in canvases)
        {
            canvas.enabled = visible;
        }
        foreach (GameObject obj in objects)
        {
            obj.SetActive(visible);
        }
    }


}
