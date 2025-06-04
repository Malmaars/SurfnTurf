using FMODUnity;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class WaterJump : WaterAbility
{
	public WaterJump(WaterMovementController _mov) : base(_mov) { }

	public override void RunOnEnterState()
	{
		InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Movement.Jump, StartJump);
	}

	public override void RunOnExitState()
	{
		InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Movement.Jump, StartJump);
	}
	public override void RunOnUpdateDuringSetVelocity()
	{
		HandleJump();
	}

	void HandleJump()
	{
		if (mov.wj.desiredJump)
		{
			mov.wj.desiredJump = false;
			DoJump();
		}
	}

	void DoJump() 
	{
		if (mov.suv.surfing && mov.wtv.kickflipCooldownTimer <= 0 && mov.wtv.activeKickFlipTokens > 0)
			DoKickFlip();
		//jump up from swimming
		if (!mov.suv.surfing && mov.wv.onWater && mov.sv.swimming)
			SwimJump();
	}
	void DoKickFlip()
	{
		//kickflip
		if (mov.velocity.y < mov.wtv.kickflipHeight)
			mov.velocity = new Vector3(mov.velocity.x, 0, mov.velocity.z);

		mov.velocity += new Vector3(0, mov.wtv.kickflipHeight, 0);

		mov.wtv.kickflipCooldownTimer = mov.wtv.kickflipCooldown;
		mov.wtv.kickFlipAnimation = true;
		mov.wtv.activeKickFlipTokens--;

        ComboMeter.AddToCombo("Kickflip");
    }

	void SwimJump()
	{
		if (mov.velocity.y < mov.wtv.kickflipHeight)
			mov.velocity = new Vector3(mov.velocity.x, 0, mov.velocity.z);

		mov.velocity += new Vector3(0, mov.wj.JumpForce, 0);
		RuntimeManager.PlayOneShot(mov.wj.jumpOutOfWaterSound);
	}

    public void StartJump(InputAction.CallbackContext context)
	{
		if (mov.iv.interacting)
			return;

		mov.wj.desiredJump = true;
	}
}
