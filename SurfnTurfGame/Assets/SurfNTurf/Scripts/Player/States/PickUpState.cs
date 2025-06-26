using Unity.Cinemachine;
using UnityEngine;

public class PickUpState : PlayerState
{
	public CinemachineCamera pickUpCamera;
	public override void EnterState()
	{
		pickUpCamera.transform.position = Camera.main.transform.position;
		pickUpCamera.transform.rotation = Camera.main.transform.rotation;
		BlackBoard.cameraController.SwitchToCamera(pickUpCamera);
	}
}
