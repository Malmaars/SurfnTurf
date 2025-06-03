using UnityEngine;
using UnityEngine.UI;

public class InvertCameraControls : MonoBehaviour
{
    public bool mouse, controller;
    public bool horizontal, vertical;
    Toggle toggle;

    private void Start()
    {
        toggle = GetComponent<Toggle>();
        Debug.Log(toggle);

        if (mouse)
        {
            if (horizontal)
                toggle.isOn = BlackBoard.cameraController.mouseHorizontalInvert;
            if (vertical)
                toggle.isOn = BlackBoard.cameraController.mouseVerticalInvert;
        }

        if (controller)
        {
            if (horizontal)
                toggle.isOn = BlackBoard.cameraController.controllerHorizontalInvert;
            if (vertical)
                toggle.isOn = BlackBoard.cameraController.controllerVerticalInvert;
        }
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

        BlackBoard.cameraController.SetSensitivity();
    }
}
