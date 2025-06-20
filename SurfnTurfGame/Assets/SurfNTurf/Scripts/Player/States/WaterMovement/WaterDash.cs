using FMODUnity;
using SurfnTurf;
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

	public override void UpdateTimers()
	{
		mov.wdv.dashLengthTimer = mov.wdv.dashLengthTimer.TimerCountdown();
		mov.wdv.dashControlTimer = mov.wdv.dashControlTimer.TimerCountdown();
		mov.wdv.dashCoyoteTimer = mov.wdv.dashCoyoteTimer.TimerCountdown();
		mov.wdv.dashTimer = mov.wdv.dashTimer.TimerCountdown();
	}
	void HandleDash()
	{
		if (mov.wdv.desiredDash)
		{
			mov.wdv.desiredDash = false;
			DoDash();
		}

		if (mov.wdv.dashing)
		{
			if (mov.wdv.dashLengthTimer > 0)
			{
				Vector3 desiredDirection;
				if (mov.wdv.threeDimensionalDash)
					desiredDirection = mov.wdv.LastHorizontalDirection;

				else
						desiredDirection = new Vector3(mov.lastInputDirection3D.x, 0, mov.lastInputDirection3D.z).normalized;

				mov.velocity = desiredDirection * mov.wdv.dashSpeed;

			}
			else
			{
				mov.wdv.dashCoyoteTimer = mov.wdv.dashCoyoteTime;
				mov.wdv.dashing = false;
			}
			return;
		}



		if (!mov.wdv.dashing)
		{
			mov.wdv.gravityOff = false;
			if (mov.wdv.immediateStop)
				mov.velocity = Vector3.zero;

			if (mov.wdv.dashed && mov.wdv.dashCoyoteTimer > 0)
				mov.wdv.dashCoyoteTimer -= Time.deltaTime;

			if (mov.wv.onWater)
				mov.wdv.dashed = false;
		}
	}

	void DoDash()
	{
		if (mov.suv.surfing && mov.wtv.barrelRollCooldownTimer <= 0 && mov.wtv.activeBarrelRollTokens > 0)
		{
			DoBarrelRoll();
			return;
		}

		if (mov.wdv.dashed || mov.wdv.dashing || mov.wv.onWater || mov.suv.surfing)
			return;

		Vector3 desiredDirection;

		Vector2 playerInput = InputDistributor.playerInputActions.Movement.DirectionalInput.ReadValue<Vector2>();

		if (mov.wdv.alwaysDashToInput && playerInput == Vector2.zero)
			return;

		mov.velocity = Vector3.zero;


			if (mov.wv.contactNormal == Vector3.zero || mov.wv.contactNormal.y < 0)
			{
				if (mov.wv.onWater)
					desiredDirection = new Vector3(mov.lastInputDirection3D.x, 0, mov.lastInputDirection3D.z).normalized;
				else
					desiredDirection = new Vector3(mov.wdv.LastHorizontalDirection.x, 0, mov.wdv.LastHorizontalDirection.z).normalized;
			}


		mov.wdv.dashing = true;
		mov.wdv.airJumped = false;
		mov.wdv.dashTimer = mov.wdv.dashCooldown;
		mov.wdv.dashLengthTimer = mov.wdv.dashLength;
		mov.wdv.dashControlTimer = mov.wdv.dashControlTime;
		mov.wdv.startDash = true;
		mov.wdv.gravityOff = true;
		RuntimeManager.PlayOneShot(mov.wdv.dashSound);
		BlackBoard.playerVFX.onJump.SendEvent("OnDash");
		mov.wj.waterJumping = false;
		mov.div.diving = false;
			mov.wdv.startedDashOnGround = false;

		if (!mov.wdv.startedDashOnGround)
			mov.wdv.dashed = true;
	}
	void DoBarrelRoll()
	{

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

	public override void UpdateAnimator()
	{
			mov.animator.SetBool("Dashing", mov.wdv.dashing);
	}
}
