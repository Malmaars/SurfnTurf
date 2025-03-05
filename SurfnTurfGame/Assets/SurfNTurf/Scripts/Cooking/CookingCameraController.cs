using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class CookingCameraController : MonoBehaviour
{
    public List<CinemachineCamera> cookingCameras;
    private int cameraIndex;

    public void EnterState()
    {
        InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Cooking.GoLeft, GoLeft);
        InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Cooking.GoRight, GoRight);
    }
    public void ExitState()
    {
        InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Cooking.GoLeft, GoLeft);
        InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Cooking.GoRight, GoRight);
    }

    public void SetCamera(int index)
    {
        if (index >= cookingCameras.Count || cookingCameras[index] == null) return;
        BlackBoard.cameraController.SwitchToCamera(cookingCameras[cameraIndex], 0.2f);
    }

    public void GoLeft(InputAction.CallbackContext context)
    {
        if (cameraIndex <= 0) return;
        cameraIndex--;
        if (cookingCameras[cameraIndex] == null) return;
        BlackBoard.cameraController.SwitchToCamera(cookingCameras[cameraIndex]);
    }

    public void GoRight(InputAction.CallbackContext context)
    {
        if (cameraIndex >= cookingCameras.Count) return;
        cameraIndex++;
        if (cookingCameras[cameraIndex] == null) return;
        BlackBoard.cameraController.SwitchToCamera(cookingCameras[cameraIndex]);
    }
}
