using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class CameraSensitivity : MonoBehaviour
{
    public bool mouse;
        public bool controller;
    public bool horizontal;
        public bool vertical;
    Slider slider;

    private void Start()
    {
        if (slider == null) slider = GetComponent<Slider>();

        slider.value = 1;
        
        if (mouse)
        {
            if (horizontal)
                slider.value = BlackBoard.cameraController.mouseHorizontalSensitivity;
            if (vertical)
                slider.value = BlackBoard.cameraController.mouseVerticalSensitivity;
        }

        if (controller)
        {
            if (horizontal)
                slider.value = BlackBoard.cameraController.controllerHorizontalSensitivity;
            if (vertical)
                slider.value = BlackBoard.cameraController.controllerVerticalSensitivity;
        }
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
    }
}
