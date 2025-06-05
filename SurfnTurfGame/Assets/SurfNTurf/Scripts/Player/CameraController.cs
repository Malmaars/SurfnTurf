using Unity.Cinemachine;
using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine.Rendering.Universal;

public class CameraController : MonoBehaviour
{
	public Rigidbody playerRb;
	public CinemachineOrbitalFollow orbitalFollow;
	public CinemachineBrain cinemachineBrain;
	public CinemachineInputAxisController inputAxisController;

	List<CinemachineCamera> knownCameras = new List<CinemachineCamera>();
	[Header("Camera Sensitivity")]
	public bool mouseHorizontalInvert;
	public bool mouseVerticalInvert;
	public float mouseHorizontalSensitivity;
	public float mouseVerticalSensitivity;
	public bool controllerHorizontalInvert;
	public bool controllerVerticalInvert;
	public float controllerHorizontalSensitivity;
	public float controllerVerticalSensitivity;


    //Make the distance of the camera greate based on how fast the player is
    [Header("velocity based distance")]
	public float vbd_damping;
	public float vbd_multiplier;
	public float vbd_min = 20;
	public float vbd_max = 200;
	public Material fullscreenWaterMaterial;
	private Material localFullscreenWaterMaterial;
	float vbd_newRadius;

	CinemachineCamera currentCamera, previousCamera;

	private void Awake()
	{
		//find the renderCamera and add it as a stack overlay to the main camera
		Camera.main.GetUniversalAdditionalCameraData().cameraStack.Add(GameObject.Find("RenderCamera").GetComponent<Camera>());
		BlackBoard.cameraController = this;

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		SetRenderMaterial();
	}

    private void Start()
    {
		SetSensitivity();
    }

    private void Update()
	{
		SetSensitivity();
		//UpdateVelocityBasedDistance();
		SetCameraPositionForUnderwater();
	}
	void UpdateVelocityBasedDistance()
	{
		vbd_newRadius = Mathf.Lerp(vbd_newRadius, playerRb.linearVelocity.magnitude * vbd_multiplier, vbd_damping * Time.deltaTime);

		vbd_newRadius = Mathf.Clamp(vbd_newRadius, vbd_min, vbd_max);
		orbitalFollow.Radius = vbd_newRadius;
	}

	public void SwitchToCamera(CinemachineCamera _newCamera) { SwitchToCamera(_newCamera, 2); }

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

	public void SetCameraPositionForUnderwater()
	{
		localFullscreenWaterMaterial.SetVector("_CameraPos", new Vector4(Camera.main.transform.position.x, Camera.main.transform.position.y, Camera.main.transform.position.z, 0));
	}
	private void SetRenderMaterial()
	{
		localFullscreenWaterMaterial = new Material(fullscreenWaterMaterial);

		// Get the current pipeline asset and renderer data
		var urpAsset = (UniversalRenderPipelineAsset)UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
		var rendererDataListField = typeof(UniversalRenderPipelineAsset).GetField("m_RendererDataList", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		if (rendererDataListField == null)
		{
			Debug.LogWarning("Could not find m_RendererDataList field.");
			return;
		}

		var rendererDataList = rendererDataListField.GetValue(urpAsset) as ScriptableRendererData[];
		if (rendererDataList == null || rendererDataList.Length == 0)
		{
			Debug.LogWarning("No renderer data found.");
			return;
		}

		// Usually the first renderer is the main one
		var rendererData = rendererDataList[0];
		foreach (var feature in rendererData.rendererFeatures)
		{
			if (feature is FullScreenPassRendererFeature fsFeature && feature.name == "CameraUnderWater")
			{
				fsFeature.passMaterial = localFullscreenWaterMaterial;
				break;
			}
		}
	}

	public void SetSensitivity()
	{
        if (InputDistributor.inputManager.isKeyboardAndMouse)
        {
            for (int i = 0; i < inputAxisController.Controllers.Count; i++)
            {
                if (inputAxisController.Controllers[i].Name == "Look Orbit X")
                    inputAxisController.Controllers[i].Input.Gain = mouseHorizontalInvert ? -mouseHorizontalSensitivity : mouseHorizontalSensitivity;

                if (inputAxisController.Controllers[i].Name == "Look Orbit Y")
                    inputAxisController.Controllers[i].Input.Gain = mouseVerticalInvert ? -mouseVerticalSensitivity : mouseVerticalSensitivity;

            }
        }
        else
        {
            for (int i = 0; i < inputAxisController.Controllers.Count; i++)
            {
                if (inputAxisController.Controllers[i].Name == "Look Orbit X")
                    inputAxisController.Controllers[i].Input.Gain = controllerHorizontalInvert ? -controllerHorizontalSensitivity : controllerHorizontalSensitivity;

                if (inputAxisController.Controllers[i].Name == "Look Orbit Y")
                    inputAxisController.Controllers[i].Input.Gain = controllerVerticalInvert ? -controllerVerticalSensitivity : controllerVerticalSensitivity;
            }
        }
    }


    private void OnApplicationQuit()
    {
		SetRenderMaterialBack();
    }
    private void SetRenderMaterialBack()
	{
		// Get the current pipeline asset and renderer data
		var urpAsset = (UniversalRenderPipelineAsset)UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
		var rendererDataListField = typeof(UniversalRenderPipelineAsset).GetField("m_RendererDataList", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		if (rendererDataListField == null)
		{
			Debug.LogWarning("Could not find m_RendererDataList field.");
			return;
		}

		var rendererDataList = rendererDataListField.GetValue(urpAsset) as ScriptableRendererData[];
		if (rendererDataList == null || rendererDataList.Length == 0)
		{
			Debug.LogWarning("No renderer data found.");
			return;
		}

		// Usually the first renderer is the main one
		var rendererData = rendererDataList[0];
		foreach (var feature in rendererData.rendererFeatures)
		{
			if (feature is FullScreenPassRendererFeature fsFeature && feature.name == "CameraUnderWater")
			{
				fsFeature.passMaterial = fullscreenWaterMaterial;
				break;
			}
		}
	}
}
