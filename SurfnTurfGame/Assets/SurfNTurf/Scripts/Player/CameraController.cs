using Unity.Cinemachine;
using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;

[RequireComponent(typeof(CinemachineBrain))]
public class CameraController : MonoBehaviour
{
	public Rigidbody playerRb;
	public CinemachineOrbitalFollow orbitalFollow;
	CinemachineBrain cinemachineBrain;

	List<CinemachineCamera> knownCameras = new List<CinemachineCamera>();
	//Make the distance of the camera greate based on how fast the player is
	[Header("velocity based distance")]
	public float vbd_damping;
	public float vbd_multiplier;
	public float vbd_min = 20;
	public float vbd_max = 200;
	float vbd_newRadius;

	private void Awake()
	{
		BlackBoard.cameraController = this;

		cinemachineBrain = GetComponent<CinemachineBrain>();
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	private void Update()
	{
		UpdateVelocityBasedDistance();
	}
	void UpdateVelocityBasedDistance()
	{
		vbd_newRadius = Mathf.Lerp(vbd_newRadius, playerRb.linearVelocity.magnitude * vbd_multiplier, vbd_damping * Time.deltaTime);

		vbd_newRadius = Mathf.Clamp(vbd_newRadius, vbd_min, vbd_max);
		orbitalFollow.Radius = vbd_newRadius;
	}

	public void SwitchToCamera(CinemachineCamera _newCamera)
	{
		foreach(CinemachineCamera cc in knownCameras)
		{
			cc.Priority = 0;
		}
		if (!knownCameras.Contains(_newCamera))
			knownCameras.Add(_newCamera);

		_newCamera.Priority = 2;
		cinemachineBrain.DefaultBlend.Time = 2;

	}

	public void SwitchToCamera(CinemachineCamera _newCamera, int _speed)
	{
		foreach (CinemachineCamera cc in knownCameras)
		{
			cc.Priority = 0;
		}
		if (!knownCameras.Contains(_newCamera))
			knownCameras.Add(_newCamera);

		_newCamera.Priority = 2;
		cinemachineBrain.DefaultBlend.Time = _speed;
	}
}
