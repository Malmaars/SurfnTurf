using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	
	public Rigidbody playerRb;
	public CinemachineOrbitalFollow orbitalFollow;
	
	//Make the distance of the camera greate based on how fast the player is
	[Header("velocity based distance")]
	public float vbd_damping;
	public float vbd_multiplier;
	public float vbd_min = 20;
	public float vbd_max = 200;
	float vbd_newRadius;

	//The camera will head to the direction that the player is moving, forward or backwards
	//[Header("camera heading")]

	private void Awake()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	private void Update()
	{
		UpdateVelocityBasedDistance();
		UpdateCameraHeading();
	}

	void UpdateCameraHeading()
	{
		Vector2 playerInput = InputDistributor.playerInputActions.Movement.DirectionalInput.ReadValue<Vector2>();
		if (playerInput == Vector2.zero)
			return;

		Vector2 cameraForward = new Vector2(Camera.main.transform.forward.x, Camera.main.transform.forward.z).normalized;
		Vector2 playerDirection2D = new Vector2(playerRb.linearVelocity.x, playerRb.linearVelocity.z).normalized;
		
		if(Vector2.Dot(playerDirection2D, cameraForward) < Vector2.Dot(playerDirection2D, -cameraForward))
		{
			//player is facing forward

			//align the 

		}
		else
		{
			//player is facing backwards
		}
	}

	void UpdateVelocityBasedDistance()
	{
		vbd_newRadius = Mathf.Lerp(vbd_newRadius, playerRb.linearVelocity.magnitude * vbd_multiplier, vbd_damping * Time.deltaTime);

		vbd_newRadius = Mathf.Clamp(vbd_newRadius, vbd_min, vbd_max);
		orbitalFollow.Radius = vbd_newRadius;
	}
}
