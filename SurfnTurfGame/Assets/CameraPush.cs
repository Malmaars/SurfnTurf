using UnityEngine;
using Unity.Cinemachine;

//A script made so the player can look up when using input, but not when not using Input
public class CameraPush : MonoBehaviour
{
	public CinemachineOrbitalFollow orbitFollow;

	Vector2 verticalRange;

	public float newMin = -10f;

	private void Awake()
	{
		verticalRange = orbitFollow.VerticalAxis.Range;
	}
	private void Update()
	{

		Vector2 CameraInput = InputDistributor.playerInputActions.Movement.Look.ReadValue<Vector2>();

		//TODO change value for inverse vertical input?
		if (CameraInput.y > 0.1f)
		{
			orbitFollow.VerticalAxis.Range = new Vector2(newMin, verticalRange.y);
		}

		else
		{
			orbitFollow.VerticalAxis.Range = new Vector2(Mathf.Lerp(orbitFollow.VerticalAxis.Range.x, verticalRange.x, Time.deltaTime * 5), verticalRange.y);
		}
	}
}
