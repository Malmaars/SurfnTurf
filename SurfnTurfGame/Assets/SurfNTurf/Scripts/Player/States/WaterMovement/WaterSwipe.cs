using FMODUnity;
using SurfnTurf;
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
	public override void UpdateTimers()
	{
		mov.ws.swipeDurationTimer = mov.ws.swipeDurationTimer.TimerCountdown();
	}
	void HandleSwipe()
	{
		if (mov.ws.desiredSwipe)
		{
			mov.ws.desiredSwipe = false;
			DoSwipe();
		}
		if (mov.wv.onWater)
		{
			mov.djv.jumped = false;
		}

		if (mov.ws.swipeDurationTimer <= 0)
			mov.ws.swiping = false;
	}

	void DoSwipe()
	{
		if (mov.suv.surfing && mov.wtv.shoveItCooldownTimer <= 0 && mov.wtv.activeShoveItTokens > 0)
		{
			DoShoveIt();
			return;
		}

		if (mov.div.active && mov.wdv.dashing && !mov.div.dived)
		{
			//Dive
			Dive();
			return;
		}

		if (mov.ws.swiping || mov.ws.swipeDurationTimer > 0 || mov.wv.onWater)
			return;

		if (mov.wj.active && !mov.suv.surfing && !mov.djv.jumped && !mov.wv.onWater)
		{
			SwipeDoubleJump();
			PlayerVFX.instance.doubleJump.SendEvent("OnPlay");
			mov.ws.doubleJumpAnimation = true;
			RuntimeManager.PlayOneShot(mov.djv.doubleJumpSound);
		}


		mov.ws.swiping = true;
		if (mov.ws.doubleJumpAnimation == false)
		{
			PlayerVFX.instance.swipe.SendEvent("OnPlay");
			mov.ws.swipeAnimation = true;
			RuntimeManager.PlayOneShot(mov.ws.swipeSound);
		}
		mov.ws.swipeDurationTimer = mov.ws.swipeDuration;
	}

	void SwipeDoubleJump()
	{
		mov.velocity = new Vector3(mov.velocity.x, 0, mov.velocity.z);
		mov.velocity += Vector3.up * mov.djv.doubleJumpHeight;
		mov.djv.jumped = true;
		mov.div.diving = false;


	}

	void Dive()
	{
		if (mov.div.divingResetsVelocity)
		{
			mov.rb.linearVelocity = Vector3.zero;
			mov.velocity = Vector3.zero;
		}

		mov.velocity += new Vector3(0, mov.div.upwardSpeed, 0);

		mov.div.divingDirection = new Vector3(mov.lastInputDirection3D.x, 0, mov.lastInputDirection3D.z).normalized;

		mov.div.diving = true;
		mov.div.dived = true;
		mov.div.diveLengthTimer = mov.div.diveLength;
		mov.wdv.dashing = false;
		mov.ws.swiping = false;

		ComboMeter.AddToCombo("Dive");
		RuntimeManager.PlayOneShot(mov.div.diveSound);
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

        ComboMeter.AddToCombo("Shoveit");
    }

    void StartSwipe(InputAction.CallbackContext context)
	{
		mov.ws.desiredSwipe = true;
	}

	public override void UpdateAnimator()
	{
		SetAnimatorTriggers();
	}

	public void SetAnimatorTriggers()
	{
		if (mov.ws.swipeAnimation && mov.ws.swiping)
		{
			mov.animator.SetTrigger("Swipe");
			mov.ws.swipeAnimation = false;
		}
		if (mov.ws.doubleJumpAnimation && mov.ws.swiping)
		{
			mov.animator.SetTrigger("DoubleJump");
			mov.ws.doubleJumpAnimation = false;
		}

	}
}
