using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Dash : Ability
{
	public Dash(IMovement _mov) : base(_mov) { }

	public override void RunOnEnterState()
	{
		InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Movement.Dash, StartDash);
	}

	public override void RunOnExitState()
	{
		InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Movement.Dash, StartDash);
	}

	public override void RunOnUpdateDuringSetVelocity() 
	{
		HandleDash();
	}

	void HandleDash()
	{
		if (mov.Velocity != Vector3.zero && !(mov.Velocity.x == 0 && mov.Velocity.z == 0))
			mov.DV.LastHorizontalDirection = mov.Velocity.normalized;

		if (!mov.AV.spd.spinDashing && mov.DV.dashing)
		{
			if (mov.DV.dashLengthTimer > 0)
			{
				mov.DV.dashLengthTimer -= Time.deltaTime;


				Vector3 desiredDirection;
				if (mov.DV.threeDimensionalDash)
					desiredDirection = mov.DV.LastHorizontalDirection;

				else
				{
					if (mov.GCV.contactNormal == Vector3.zero || mov.GCV.contactNormal.y < 0 || mov.GCV.onSlope)
					{
						desiredDirection = new Vector3(mov.LastInputDirection3D.x, 0, mov.LastInputDirection3D.z).normalized;
					}
					else
					{
						desiredDirection = mov.ProjectOnContactPlane(new Vector3(mov.LastInputDirection3D.x, 0, mov.LastInputDirection3D.z).normalized).normalized;
					}
				}

				Debug.Log(mov.DV.dashSpeed);
				mov.Velocity = desiredDirection * mov.DV.dashSpeed;

			}
			else
			{
				mov.DV.dashCoyoteTimer = mov.DV.dashCoyoteTime;
				mov.DV.dashing = false;
			}
			return;
		}

		if (mov.DV.dashTimer > 0)
		{
			mov.DV.dashTimer -= Time.deltaTime;
		}

		if (!mov.DV.dashing)
		{
			mov.DV.gravityOff = false;
			if (mov.DV.immediateStop)
				mov.Velocity = Vector3.zero;

			if (mov.DV.dashed && mov.DV.dashCoyoteTimer > 0)
				mov.DV.dashCoyoteTimer -= Time.deltaTime;
		}

		if (mov.DV.dashControlTimer > 0)
			mov.DV.dashControlTimer -= Time.deltaTime;

		if (mov.GCV.grounded && !mov.GCV.onSlope)
		{
			if (mov.DV.dashTimer <= 0)
				mov.DV.dashed = false;
		}

		if (mov.DV.desiredDash)
			DoDash();

	}

	void DoDash()
	{
		Vector3 desiredDirection;
		mov.DV.desiredDash = false;
		if (mov.DV.dashed)
			return;

		if (mov.SUV.surfing) 
		{
			//do a slide
			Slide();
			return;
		}

		mov.Velocity = Vector3.zero;

		if (mov.SWV.swiping && mov.GCV.grounded)
		{
			SpinDash();
			return;
		}

		if (mov.DV.threeDimensionalDash)
			desiredDirection = mov.DV.LastHorizontalDirection;

		else
		{
			if (mov.GCV.contactNormal == Vector3.zero || mov.GCV.contactNormal.y < 0 || mov.GCV.onSlope)
			{
				if (mov.DV.fullDashControl)
					desiredDirection = new Vector3(mov.LastInputDirection3D.x, 0, mov.LastInputDirection3D.z).normalized;
				else
					desiredDirection = new Vector3(mov.DV.LastHorizontalDirection.x, 0, mov.DV.LastHorizontalDirection.z).normalized;
			}
			else
			{
				if (mov.DV.fullDashControl)
					desiredDirection = mov.ProjectOnContactPlane(new Vector3(mov.LastInputDirection3D.x, 0, mov.LastInputDirection3D.z).normalized).normalized;
				else
					desiredDirection = mov.ProjectOnContactPlane(new Vector3(mov.DV.LastHorizontalDirection.x, 0, mov.DV.LastHorizontalDirection.z).normalized).normalized;
			}
		}
		mov.DV.dashed = true;
		mov.DV.dashing = true;
		mov.DV.airJumped = false;
		mov.DV.dashTimer = mov.DV.dashCooldown;
		mov.DV.dashLengthTimer = mov.DV.dashLength;
		mov.DV.dashControlTimer = mov.DV.dashControlTime;
		mov.DV.startDash = true;
		mov.DV.gravityOff = true;
		mov.DV.onDash.Invoke();
		mov.JC.jumping = false;
		mov.AV.lv.leapAvailable = true;

		if (mov.DV.dashingResetsLeap)
		{
			mov.AV.lv.leapt = false;
		}
	}

	void SpinDash()
	{
		mov.AV.spd.spinDashing = true;
		mov.AV.spd.durationTimer = mov.AV.spd.duration;

		mov.AV.spd.spindDashAnimation = true;
		PlayerVFX.instance.twirl.gameObject.SetActive(true);

		if (mov.GCV.contactNormal == Vector3.zero || mov.GCV.contactNormal.y < 0 || mov.GCV.onSlope)
		{
			mov.AV.spd.spindDashDirection = new Vector3(mov.LastInputDirection3D.x, 0, mov.LastInputDirection3D.z).normalized;
		}
		else
		{
			mov.AV.spd.spindDashDirection = mov.ProjectOnContactPlane(new Vector3(mov.LastInputDirection3D.x, 0, mov.LastInputDirection3D.z).normalized).normalized;
		}

		mov.DV.dashed = true;
		mov.DV.airJumped = false;
		mov.DV.onDash.Invoke();
		mov.JC.jumping = false;
		mov.AV.lv.leapAvailable = true;
		mov.DV.gravityOff = false;

		if (mov.DV.dashingResetsLeap)
		{
			mov.AV.lv.leapt = false;
		}

		return;
	}

	void Slide()
	{
		if (mov.AV.slv.sliding || mov.AV.slv.slid)
			return;

		//boost the player forward;
		mov.AV.slv.sliding = true;
		mov.AV.slv.slid = true;
		mov.Velocity += mov.Velocity.normalized * mov.AV.slv.boostPower;
		mov.AV.slv.slideDurationTimer = mov.AV.slv.slideDuration;
	}

	public override void UpdateAnimator()
	{
		if (!mov.AV.spd.spinDashing)
			mov.PlayerAnimator.SetBool("Dashing", mov.DV.dashing);

	}

	void StartDash(InputAction.CallbackContext context)
	{
		mov.DV.desiredDash = true;
	}
}
