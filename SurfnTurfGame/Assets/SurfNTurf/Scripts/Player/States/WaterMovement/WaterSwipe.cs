using UnityEngine;
using UnityEngine.InputSystem;

public class WaterSwipe : WaterAbility
{
	public WaterSwipe(WaterMovementController _mov) : base(_mov) { }

	public override void RunOnEnterState()
	{
		InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Movement.Swipe, StartSwipe);
	}

	public override void RunOnExitState()
	{
		InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Movement.Swipe, StartSwipe);
	}
	public override void RunOnUpdateDuringSetVelocity()
	{
		HandleSwipe();
	}
	void HandleSwipe()
	{
		if (mov.ws.desiredSwipe)
		{
			mov.ws.desiredSwipe = false;
			DoSwipe();
		}
	}

	void DoSwipe()
	{
		if (mov.suv.surfing && mov.wtv.shoveItCooldownTimer <= 0 && mov.wtv.activeShoveItTokens > 0)
			DoShoveIt();
	}
	void DoShoveIt()
	{
		//kickflip
		if (mov.velocity.y < mov.wtv.shoveItHeight)
			mov.velocity = new Vector3(mov.velocity.x, 0, mov.velocity.z);

		mov.velocity += new Vector3(0, mov.wtv.shoveItHeight, 0);

		mov.wtv.shoveItCooldownTimer = mov.wtv.shoveItCooldown;
		mov.wtv.shoveItAnimation = true;
		mov.wtv.activeShoveItTokens--;
	}

	void StartSwipe(InputAction.CallbackContext context)
	{
		mov.ws.desiredSwipe = true;
	}
}
