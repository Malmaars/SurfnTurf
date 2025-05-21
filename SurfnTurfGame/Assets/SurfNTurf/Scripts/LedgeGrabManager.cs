using UnityEngine;

public class LedgeGrabManager : MonoBehaviour
{

	public MovementController movementController;
	//function called at the end of the ledgegrab animation

	private void Start()
	{
		movementController = FindFirstObjectByType<MovementController>();
	}
	public void EndLedgeGrab()
	{
		movementController.lgv.endLedgeGrab = true;
	}
}
