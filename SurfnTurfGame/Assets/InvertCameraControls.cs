using System;
using UnityEngine;
using UnityEngine.UI;

public class InvertCameraControls : MonoBehaviour
{
    public bool mouse, controller;
    public bool horizontal, vertical;
    Toggle toggle;

	private String prefName;

	private void Awake()
    {
        toggle = GetComponent<Toggle>();

		prefName = gameObject.name;
		int boolInt = PlayerPrefs.GetInt(prefName, 0);
        toggle.isOn = (boolInt == 0) ? false : true;

        SetInvert();
    }

    public void SetInvert()
    {
        if(mouse)
        {
            if(horizontal)
                BlackBoard.cameraController.mouseHorizontalInvert = toggle.isOn;
            if(vertical)
                BlackBoard.cameraController.mouseVerticalInvert = toggle.isOn;
        }

        if(controller)
        {
            if (horizontal)
                BlackBoard.cameraController.controllerHorizontalInvert = toggle.isOn;
            if (vertical)
                BlackBoard.cameraController.controllerVerticalInvert = toggle.isOn;
        }

		PlayerPrefs.SetInt(prefName, toggle.isOn ? 1 : 0);
		PlayerPrefs.Save();
	}
}
