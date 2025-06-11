using UnityEngine;
using UnityEngine.InputSystem;

public class WaterDash : WaterAbility
{
	public WaterDash(WaterMovementController _mov) : base(_mov) { }

	public override void RunOnEnterState()
	{
		InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Movement.Dash, StartSwipe);
	}

	public override void RunOnExitState()
	{
		InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Movement.Dash, StartSwipe);
	}
	public override void RunOnUpdateDuringSetVelocity()
	{
		HandleDash();
	}
	void HandleDash()
	{
		if (mov.wdv.desiredDash)
		{
			mov.wdv.desiredDash = false;
			Debug.Log("Dash");
			DoDash();
		}
	}

	void DoDash()
	{
		if (mov.suv.surfing && mov.wtv.barrelRollCooldownTimer <= 0 && mov.wtv.activeBarrelRollTokens > 0)
			DoBarrelRoll();
	}
	void DoBarrelRoll()
	{
			Debug.Log("Barrel Roll");

		if (mov.velocity.y < mov.wtv.barrelRollHeight)
			mov.velocity = new Vector3(mov.velocity.x, 0, mov.velocity.z);

		mov.velocity += new Vector3(0, mov.wtv.barrelRollHeight, 0);

		mov.wtv.barrelRollCooldownTimer = mov.wtv.barrelRollCooldown;
		mov.wtv.barrelRollAnimation = true;
		mov.wtv.activeBarrelRollTokens--;

		ComboMeter.AddToCombo("Barrel Roll");
	}

	void StartSwipe(InputAction.CallbackContext context)
	{
		mov.wdv.desiredDash = true;
	}
}
