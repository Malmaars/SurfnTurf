using Unity.Cinemachine;
using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;

public class CameraController : MonoBehaviour
{
	public Rigidbody playerRb;
	public CinemachineOrbitalFollow orbitalFollow;

	List<CinemachineCamera> knownCameras = new List<CinemachineCamera>();
	//Make the distance of the camera greate based on how fast the player is
	[Header("velocity based distance")]
	public float vbd_damping;
	public float vbd_multiplier;
	public float vbd_min = 20;
	public float vbd_max = 200;
	float vbd_newRadius;

	// Static field to hold the instance
	private static CameraController _instance;

	// Property to get the instance
	public static CameraController Instance
	{
		get
		{
			if (_instance == null)
			{
				// Try to find the instance in the scene
				_instance = FindFirstObjectByType<CameraController>();

				// If no instance found, you can log a warning
				if (_instance == null)
				{
					Debug.LogWarning("CameraController instance not found in the scene!");
				}
			}
			return _instance;
		}
	}

	private void Awake()
	{
		// If an instance already exists and it's not this one, destroy this object
		if (_instance != null && _instance != this)
		{
			Destroy(gameObject);
		}
		else
		{
			// Otherwise, set the instance to this object
			_instance = this;
			DontDestroyOnLoad(gameObject); // Optional: persists across scene loads
		}

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
	}
}
