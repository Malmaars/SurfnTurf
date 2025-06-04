using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class CameraSensitivity : MonoBehaviour
{
    public bool mouse;
        public bool controller;
    public bool horizontal;
        public bool vertical;
    Slider slider;

	private String prefName; 

	private void Start()
    {
        if (slider == null) slider = GetComponent<Slider>();

        prefName = transform.parent.gameObject.name;
        slider.value = PlayerPrefs.GetFloat(prefName, 1f);

        SetSensitivity();
    }

    public void SetSensitivity()
    {
        if(slider == null) slider = GetComponent<Slider>();
        if (mouse)
        {
            if(horizontal)
                BlackBoard.cameraController.mouseHorizontalSensitivity = slider.value;
            if(vertical)
                BlackBoard.cameraController.mouseVerticalSensitivity = slider.value;
        }

        if(controller)
        {
            Debug.Log(vertical);

            if (horizontal)
                BlackBoard.cameraController.controllerHorizontalSensitivity = slider.value;
            if (vertical)
                BlackBoard.cameraController.controllerVerticalSensitivity = slider.value;
        }

        BlackBoard.cameraController.SetSensitivity();
		PlayerPrefs.SetFloat(prefName, slider.value);
		PlayerPrefs.Save();
	}
}
