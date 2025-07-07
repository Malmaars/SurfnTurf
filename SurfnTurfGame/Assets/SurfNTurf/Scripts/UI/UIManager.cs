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
    public GameObject[] Tutorials;
    public TutorialUIPart tutorialPart;
    public Compass compass;

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

    public void ShowTutorial(bool show)
    {
        TutorialUIPart part = tutorialPart;
        for (int i = 0; i < Tutorials.Length; i++)
        {
            if (i == (int)part)
            {
                Tutorials[i].SetActive(show);
            }
            else
            {
                Tutorials[i].SetActive(false);
            }
            /*
            for (int j = 0; j < BlackBoard.cookingManager.objectsForTutorial.Length; j++)
            {
                if (BlackBoard.cookingManager.objectsForTutorial[j].part == part)
                {
                    BlackBoard.cookingManager.objectsForTutorial[j].tutorialObject.SetActive(show);
                }
                else
                {
                    BlackBoard.cookingManager.objectsForTutorial[j].tutorialObject.SetActive(false);
                }
            }
            */
        }
    }


}
public enum TutorialUIPart
{
    Jump = 0,
    Dash = 1,
    AirDash = 2,
    Leap = 3,
    Swipe = 4,
    AirJump = 5,
    TwirlJump = 6,
    SpinDash = 7,
    Dive = 8,
    Surf = 9,
    WallJump = 10,
    inventory = 11,
    pan = 12,
    Tools = 13,
    Plates = 14,

}
[Serializable]
public class TutorialObjectIndex
{
    public TutorialUIPart part;
    public GameObject tutorialObject;
}
