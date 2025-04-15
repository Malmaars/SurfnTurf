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

		if ((!mov.AV.spd.spinDashing && mov.DV.dashing) && mov.DV.alwaysDashToInput && InputDistributor.playerInputActions.Movement.DirectionalInput.ReadValue<Vector2>() == Vector2.zero)
			mov.DV.dashing = false;

		if (mov.DV.desiredDash)
			DoDash();

		if (!mov.AV.spd.spinDashing && mov.DV.dashing)
		{
			if (mov.GCV.allContactNormals.Length > 0)
			{
				foreach (Vector3 normal in mov.GCV.allContactNormals)
				{
					if(normal.y < mov.GCV.minGroundDotProduct)
						Debug.Log(Vector3.Dot(new Vector3(mov.LastInputDirection3D.x, 0, mov.LastInputDirection3D.z).normalized, -normal));
					if (normal.y < mov.GCV.minGroundDotProduct && Vector3.Dot(new Vector3(mov.LastInputDirection3D.x, 0, mov.LastInputDirection3D.z).normalized, -normal) >= 1 - mov.WJV.inputDirectionLeeway)
					{
						//stop the dash
						mov.DV.dashing = false;
					}
				}
			}

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
	}

	void DoDash()
	{
		Vector3 desiredDirection;

		Vector2 playerInput = InputDistributor.playerInputActions.Movement.DirectionalInput.ReadValue<Vector2>();
		mov.DV.desiredDash = false;

		if (mov.DV.dashed || mov.DV.dashing)
			return;

		if (mov.DV.alwaysDashToInput && playerInput == Vector2.zero)
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
		mov.AV.div.diving = false;
		if (mov.GCV.grounded || Physics.Raycast(mov.RB.position, Vector3.down, mov.AV.lv.maxDistanceFromGround))
			mov.DV.startedDashOnGround = true;
		else
			mov.DV.startedDashOnGround = false;

		if (!mov.DV.startedDashOnGround)
			mov.DV.dashed = true;

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
		PlayerVFX.instance.spinner.gameObject.SetActive(true);

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
		Vector2 playerInput = InputDistributor.playerInputActions.Movement.DirectionalInput.ReadValue<Vector2>();

		if (mov.AV.slv.canDashAnyDirection && playerInput != Vector2.zero)
		{
			if (mov.GCV.grounded)
				mov.Velocity = mov.ProjectOnContactPlane(mov.LastInputDirection3D).normalized * (mov.Velocity.magnitude + mov.AV.slv.boostPower);
			else
				mov.Velocity = mov.LastInputDirection3D.normalized * (mov.Velocity.magnitude + mov.AV.slv.boostPower);
		}
		else
		{
			if (mov.Velocity.x != 0 && mov.Velocity.z != 0)
				mov.Velocity += mov.Velocity.normalized * mov.AV.slv.boostPower;

			else
			{
				if (mov.GCV.grounded)
					mov.Velocity += mov.ProjectOnContactPlane(mov.LastInputDirection3D).normalized * mov.AV.slv.boostPower;
				else
					mov.Velocity += mov.LastInputDirection3D.normalized * mov.AV.slv.boostPower;
			}
		}
        mov.AV.slv.slideDurationTimer = mov.AV.slv.slideDuration;
		mov.AV.slv.slideAnimation = true;
		mov.AV.slv.slideCooldownTimer = mov.AV.slv.slideCooldown;
	}

	public override void ResetValues()
	{
		mov.DV.dashing = false;
		mov.DV.dashed = false;
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
