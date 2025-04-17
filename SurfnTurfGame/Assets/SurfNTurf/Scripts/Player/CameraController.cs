using Unity.Cinemachine;
using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;

public class CameraController : MonoBehaviour
{
	public Rigidbody playerRb;
	public CinemachineOrbitalFollow orbitalFollow;
	public CinemachineBrain cinemachineBrain;

	List<CinemachineCamera> knownCameras = new List<CinemachineCamera>();
	//Make the distance of the camera greate based on how fast the player is
	[Header("velocity based distance")]
	public float vbd_damping;
	public float vbd_multiplier;
	public float vbd_min = 20;
	public float vbd_max = 200;
	float vbd_newRadius;

	CinemachineCamera currentCamera, previousCamera;

	private void Awake()
	{
		BlackBoard.cameraController = this;

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	private void Update()
	{
		//UpdateVelocityBasedDistance();
	}
	void UpdateVelocityBasedDistance()
	{
		vbd_newRadius = Mathf.Lerp(vbd_newRadius, playerRb.linearVelocity.magnitude * vbd_multiplier, vbd_damping * Time.deltaTime);

		vbd_newRadius = Mathf.Clamp(vbd_newRadius, vbd_min, vbd_max);
		orbitalFollow.Radius = vbd_newRadius;
	}

	public void SwitchToCamera(CinemachineCamera _newCamera){ SwitchToCamera(_newCamera, 2); }

	public void SwitchToCamera(CinemachineCamera _newCamera, float _speed)
	{
		if (currentCamera != null || _newCamera != currentCamera)
			previousCamera = currentCamera;
		foreach (CinemachineCamera cc in knownCameras)
		{
			cc.Priority = 0;
		}
		if (!knownCameras.Contains(_newCamera))
			knownCameras.Add(_newCamera);

		currentCamera = _newCamera;
		_newCamera.Priority = 2;
		cinemachineBrain.DefaultBlend.Time = _speed;
	}

	public void SwitchToPreviousCamera() { SwitchToCamera(previousCamera); }
	public void SwitchToPreviousCamera(float _speed) { SwitchToCamera(previousCamera, _speed); }
}
