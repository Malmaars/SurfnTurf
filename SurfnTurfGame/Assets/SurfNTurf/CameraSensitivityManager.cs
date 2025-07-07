using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class CameraSensitivityManager : MonoBehaviour
{
    public bool mouseHorizontalInvert, mouseVerticalInvert;
    public float mouseHorizontalSensitivity, mouseVerticalSensitivity;
    public bool controllerHorizontalInvert, controllerVerticalInvert;
    public float controllerHorizontalSensitivity, controllerVerticalSensitivity;

    CinemachineInputAxisController inputAxisController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inputAxisController = GetComponent<CinemachineInputAxisController>();
        for (int i = 0; i < inputAxisController.Controllers.Count; i++)
        {
            Debug.Log(i + " = " + inputAxisController.Controllers[i].Name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (InputDistributor.inputManager.isKeyboardAndMouse)
        {
            for (int i = 0; i < inputAxisController.Controllers.Count; i++)
            {
                if (inputAxisController.Controllers[i].Name == "Look Orbit X")
                    inputAxisController.Controllers[i].InputValue = mouseHorizontalInvert ? -mouseHorizontalSensitivity : mouseHorizontalSensitivity;
                
                if (inputAxisController.Controllers[i].Name == "Look Orbit Y")
                    inputAxisController.Controllers[i].InputValue = mouseVerticalInvert ? -mouseVerticalSensitivity : mouseVerticalSensitivity;

            }
        }
        else
        {
            for (int i = 0; i < inputAxisController.Controllers.Count; i++)
            {
                if (inputAxisController.Controllers[i].Name == "Look Orbit X")
                    inputAxisController.Controllers[i].InputValue = controllerHorizontalInvert ? -controllerHorizontalSensitivity : controllerHorizontalSensitivity;

                if (inputAxisController.Controllers[i].Name == "Look Orbit Y")
                    inputAxisController.Controllers[i].InputValue = controllerVerticalInvert ? -controllerVerticalSensitivity : controllerVerticalSensitivity;
            }
        }
    }
}
