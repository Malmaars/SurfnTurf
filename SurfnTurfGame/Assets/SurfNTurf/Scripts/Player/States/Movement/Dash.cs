using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using SurfnTurf;

public class Dash : Ability
{
	public Dash(MovementController _mov) : base(_mov) { }

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
		if (mov.velocity != Vector3.zero && !(mov.velocity.x == 0 && mov.velocity.z == 0))
			mov.dv.LastHorizontalDirection = mov.velocity.normalized;

		if ((!mov.av.spd.spinDashing && mov.dv.dashing) && mov.dv.alwaysDashToInput && InputDistributor.playerInputActions.Movement.DirectionalInput.ReadValue<Vector2>() == Vector2.zero)
			mov.dv.dashing = false;

		if (mov.dv.desiredDash)
			DoDash();

		if (!mov.av.spd.spinDashing && mov.dv.dashing)
		{
			if (mov.gcv.allContactNormals.Length > 0)
			{
				foreach (Vector3 normal in mov.gcv.allContactNormals)
				{
					if (normal.y < mov.gcv.minGroundDotProduct && Vector3.Dot(new Vector3(mov.lastInputDirection3D.x, 0, mov.lastInputDirection3D.z).normalized, -normal) >= 1 - mov.wjv.inputDirectionLeeway)
					{
						//stop the dash
						mov.dv.dashing = false;
					}
				}
			}

			if (mov.dv.dashLengthTimer > 0)
			{
				Vector3 desiredDirection;
				if (mov.dv.threeDimensionalDash)
					desiredDirection = mov.dv.LastHorizontalDirection;

				else
				{
					if (mov.gcv.contactNormal == Vector3.zero || mov.gcv.contactNormal.y < 0 || mov.gcv.onSlope)
					{
						desiredDirection = new Vector3(mov.lastInputDirection3D.x, 0, mov.lastInputDirection3D.z).normalized;
					}
					else
					{
						desiredDirection = mov.ProjectOnContactPlane(new Vector3(mov.lastInputDirection3D.x, 0, mov.lastInputDirection3D.z).normalized).normalized;
					}
				}

				mov.velocity = desiredDirection * mov.dv.dashSpeed;

			}
			else
			{
				mov.dv.dashCoyoteTimer = mov.dv.dashCoyoteTime;
				mov.dv.dashing = false;
			}
			return;
		}



		if (!mov.dv.dashing)
		{
			mov.dv.gravityOff = false;
			if (mov.dv.immediateStop)
				mov.velocity = Vector3.zero;

			if (mov.dv.dashed && mov.dv.dashCoyoteTimer > 0)
				mov.dv.dashCoyoteTimer -= Time.deltaTime;
		}


		if (mov.gcv.grounded && !mov.gcv.onSlope)
		{
			if (mov.dv.dashTimer <= 0)
				mov.dv.dashed = false;
		}
	}

	public override void UpdateTimers()
	{		
		mov.dv.dashControlTimer = mov.dv.dashControlTimer.TimerCountdown();
		mov.dv.dashTimer = mov.dv.dashTimer.TimerCountdown();
		mov.dv.dashLengthTimer = mov.dv.dashLengthTimer.TimerCountdown();
		if (!mov.dv.dashing && mov.dv.dashed)
			mov.dv.dashCoyoteTimer = mov.dv.dashCoyoteTimer.TimerCountdown();

	}

	void DoDash()
	{
		mov.dv.desiredDash = false;

		if (!mov.dv.active)
			return;

		Vector3 desiredDirection;

		Vector2 playerInput = InputDistributor.playerInputActions.Movement.DirectionalInput.ReadValue<Vector2>();

		if (mov.dv.dashed || mov.dv.dashing
			|| (!(mov.gcv.grounded || Physics.Raycast(mov.rb.position, Vector3.down, mov.av.lv.maxDistanceFromGround)) && !mov.av.ad.active))
			return;

		if (mov.dv.alwaysDashToInput && playerInput == Vector2.zero)
			return;

		if (mov.av.slv.active && mov.suv.surfing) 
		{
			//do a slide
			Slide();
			return;
		}

		mov.velocity = Vector3.zero;

		if (mov.av.spd.active && mov.swv.swiping && mov.gcv.grounded)
		{
			SpinDash();
			return;
		}

		if (mov.dv.threeDimensionalDash)
			desiredDirection = mov.dv.LastHorizontalDirection;

		else
		{
			if (mov.gcv.contactNormal == Vector3.zero || mov.gcv.contactNormal.y < 0 || mov.gcv.onSlope)
			{
				if (mov.dv.fullDashControl)
					desiredDirection = new Vector3(mov.lastInputDirection3D.x, 0, mov.lastInputDirection3D.z).normalized;
				else
					desiredDirection = new Vector3(mov.dv.LastHorizontalDirection.x, 0, mov.dv.LastHorizontalDirection.z).normalized;
			}
			else
			{
				if (mov.dv.fullDashControl)
					desiredDirection = mov.ProjectOnContactPlane(new Vector3(mov.lastInputDirection3D.x, 0, mov.lastInputDirection3D.z).normalized).normalized;
				else
					desiredDirection = mov.ProjectOnContactPlane(new Vector3(mov.dv.LastHorizontalDirection.x, 0, mov.dv.LastHorizontalDirection.z).normalized).normalized;
			}
		}

		mov.dv.dashing = true;
		mov.dv.airJumped = false;
		mov.dv.dashTimer = mov.dv.dashCooldown;
		mov.dv.dashLengthTimer = mov.dv.dashLength;
		mov.dv.dashControlTimer = mov.dv.dashControlTime;
		mov.dv.startDash = true;
		mov.dv.gravityOff = true;
		mov.dv.onDash.Invoke();
		mov.jc.jumping = false;
		mov.av.lv.leapAvailable = true;
		mov.av.div.diving = false;
		if (mov.gcv.grounded || Physics.Raycast(mov.rb.position, Vector3.down, mov.av.lv.maxDistanceFromGround))
		{
			mov.dv.startedDashOnGround = true;
			mov.av.ad.airDashing = false;
		}
		else
			mov.dv.startedDashOnGround = false;

		if (!mov.dv.startedDashOnGround)
			mov.dv.dashed = true;

		if (mov.dv.dashingResetsLeap)
		{
			mov.av.lv.leapt = false;
		}
	}

	void SpinDash()
	{
		mov.av.spd.spinDashing = true;
		mov.av.spd.durationTimer = mov.av.spd.duration;

		mov.av.spd.spindDashAnimation = true;
		PlayerVFX.instance.spinner.gameObject.SetActive(true);

		if (mov.gcv.contactNormal == Vector3.zero || mov.gcv.contactNormal.y < 0 || mov.gcv.onSlope)
		{
			mov.av.spd.spindDashDirection = new Vector3(mov.lastInputDirection3D.x, 0, mov.lastInputDirection3D.z).normalized;
		}
		else
		{
			mov.av.spd.spindDashDirection = mov.ProjectOnContactPlane(new Vector3(mov.lastInputDirection3D.x, 0, mov.lastInputDirection3D.z).normalized).normalized;
		}

		mov.dv.dashed = true;
		mov.dv.airJumped = false;
		mov.dv.onDash.Invoke();
		mov.jc.jumping = false;
		mov.av.lv.leapAvailable = true;
		mov.dv.gravityOff = false;
		mov.av.tj.turnOffTwirlJump = true;

		ComboMeter.AddToCombo("Spin Dash");

		if (mov.dv.dashingResetsLeap)
		{
			mov.av.lv.leapt = false;
		}

		return;
	}

	void Slide()
	{
		if (mov.av.slv.sliding || mov.av.slv.slid)
			return;

		//boost the player forward;
		mov.av.slv.sliding = true;
		mov.av.slv.slid = true;
		Vector2 playerInput = InputDistributor.playerInputActions.Movement.DirectionalInput.ReadValue<Vector2>();

		if (mov.av.slv.canDashAnyDirection && playerInput != Vector2.zero)
		{
			if (mov.gcv.grounded)
				mov.velocity = mov.ProjectOnContactPlane(mov.lastInputDirection3D).normalized * (mov.velocity.magnitude + mov.av.slv.boostPower);
			else
				mov.velocity = mov.lastInputDirection3D.normalized * (mov.velocity.magnitude + mov.av.slv.boostPower);
		}
		else
		{
			if (mov.velocity.x != 0 && mov.velocity.z != 0)
				mov.velocity += mov.velocity.normalized * mov.av.slv.boostPower;

			else
			{
				if (mov.gcv.grounded)
					mov.velocity += mov.ProjectOnContactPlane(mov.lastInputDirection3D).normalized * mov.av.slv.boostPower;
				else
					mov.velocity += mov.lastInputDirection3D.normalized * mov.av.slv.boostPower;
			}
		}
		Debug.Log(mov.velocity.magnitude);

		if(mov.velocity.magnitude > mov.av.slv.maximumSpeed)
		{
			mov.velocity = mov.velocity.normalized * mov.av.slv.maximumSpeed;
		}
        mov.av.slv.slideDurationTimer = mov.av.slv.slideDuration;
		mov.av.slv.slideAnimation = true;
		mov.av.slv.slideCooldownTimer = mov.av.slv.slideCooldown;
	}

	public override void ResetValues()
	{
		mov.dv.dashing = false;
		mov.dv.dashed = false;
	}

	public override void UpdateAnimator()
	{
		if (!mov.av.spd.spinDashing)
			mov.animator.SetBool("Dashing", mov.dv.dashing);
	}

	void StartDash(InputAction.CallbackContext context)
	{
		mov.dv.desiredDash = true;
	}
}
