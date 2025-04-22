using NaughtyAttributes;
using Steamworks;
using SurfnTurf;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Jump : Ability
{
	public Jump(MovementController _mov) : base(_mov){ }

	public override void RunOnEnterState()
	{
		InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Movement.Jump, StartJump);
		InputDistributor.inputManager.AddActionToInputCancelled(InputDistributor.playerInputActions.Movement.Jump, EndJump);

	}

	public override void RunOnExitState()
	{
		InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Movement.Jump, StartJump);
		InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Movement.Jump, EndJump);
	}
	public override void RunOnAwake()
	{
		mov.JC.jumpDirection = Vector3.zero;
	}

	public override void RunOnUpdateDuringSetVelocity()
	{
		HandleJumping();
	}

	public override void UpdateTimers()
	{
		mov.JC.jumpBufferTimer = mov.JC.jumpBufferTimer.TimerCountdown();
		mov.JC.coyoteTimer = mov.JC.coyoteTimer.TimerCountdown();
	}

	void HandleJumping()
	{
		if (mov.GCV.grounded && !mov.JC.jumping)
			mov.JC.jumpPhase = 0;

		if (!mov.GCV.grounded && mov.JC.jumping)
			mov.JC.coyoteTimeAvailable = false;

		if (mov.JC.desiredJump || (mov.GCV.grounded && mov.JC.jumpBufferTimer > 0 && !mov.JC.jumping))
		{
			mov.JC.desiredJump = false;
			DoJump();
		}


		if (mov.JC.jumping && mov.Velocity.y < 0f)
		{
			mov.JC.jumping = false;
		}
	}

	void DoJump()
	{
		int newMaxAirJumps = mov.JC.maxAirJumps;
		newMaxAirJumps = (mov.DV.dashingGivesExtraJump && mov.DV.dashed && mov.DV.dashCoyoteTimer > 0) ? newMaxAirJumps + 1 : newMaxAirJumps;

		if (mov.SUV.surfing)
		{
			if (mov.AV.btv.kickflipCooldownTimer <= 0 && mov.AV.btv.activeKickFlipTokens > 0)
			{
				Kickflip();
			}
			return;
		}

		if (mov.GCV.grounded)
			mov.AV.sp.jumpParryCoyoteTimer = mov.AV.sp.jumpParryCoyoteTime;

		if (mov.AV.lv.active 
			&& mov.AV.lv.leapAvailable
			&& !mov.WJV.wallgrab 
			&& !mov.AV.lv.leaping 
			&& !mov.WJV.wallRiding 
			&& !mov.AV.lv.leapt 
			&& !mov.GCV.onSlope 
			&& (((mov.DV.dashing || mov.AV.lv.leapCoyoteTimer > 0) || mov.AV.spd.spinDashing) 
				&& (mov.GCV.grounded || Physics.Raycast(mov.RB.position, Vector3.down, mov.AV.lv.maxDistanceFromGround) || (mov.JC.coyoteTimer > 0 && mov.DV.startedDashOnGround)) || (mov.DV.dashed && (mov.GCV.grounded || Physics.Raycast(mov.RB.position, Vector3.down, mov.AV.lv.maxDistanceFromGround)) && mov.JC.jumpBufferTimer > 0)))
		{
			Leap();
			return;
		}
		else if (mov.WJV.wallgrab || mov.WJV.wallRiding || mov.WJV.wallJumpCoyoteTimer > 0)
		{
			mov.JC.jumping = true;

			Vector3 newDir = (mov.WJV.jumpDirection + Vector3.up);
			newDir.Normalize();
			newDir = new Vector3(newDir.x, Mathf.Tan(Mathf.Deg2Rad * mov.WJV.walljumpAngle), newDir.z);

			mov.LastInputDirection3D = new Vector3(newDir.x, 0, newDir.z).normalized;
			mov.Velocity = Vector3.zero;
			mov.Velocity += newDir * mov.WJV.wallJumpForce;
			mov.ACV.antiAirTimer = mov.WJV.wallJumpAntiAirTimer;
			mov.WJV.wallJumpCooldownTimer = mov.WJV.wallJumpCooldown;
			mov.WJV.wallJumpCoyoteTimer = 0;
			mov.WJV.wallJumped = true;
			mov.WJV.wallgrab = false;
			mov.WJV.wallRiding = false;
			mov.WJV.wallJumpLimitVelocity = true;
			mov.WJV.onWallJump.Invoke();
			mov.WJV.wallJumpAnimation = true;
			RotatePlayerInstantly(new Vector3(newDir.x, 0, newDir.z).normalized);
			mov.JC.jumpPhase = 1;
		}
		else
		{
			if (mov.DV.breakDashWithJump && !mov.DV.airJumped && mov.JC.inAir && (mov.DV.dashing || mov.DV.dashCoyoteTimer > 0))
			{
				mov.DV.airJumped = true;
				mov.Velocity = Vector3.zero;
				mov.DV.dashing = false;
			}

			if ((mov.GCV.grounded || mov.JC.jumpPhase <= newMaxAirJumps || mov.JC.coyoteTimer > 0))
			{
				mov.JC.jumpBufferTimer = 0;
				mov.JC.jumping = true;

				float jumpSpeed = mov.JC.jumpHeight;

				if (!mov.WJV.wallgrab)
				{
					mov.Velocity = new Vector3(mov.Velocity.x, 0, mov.Velocity.z);
					if (!mov.GCV.onSlope)
					{
						if (mov.SWV.swiping)
							TwirlJump();
						else
							mov.Velocity += Vector3.up * jumpSpeed;
					}
					else
					{
						mov.Velocity = new Vector3(mov.Velocity.x, 0, mov.Velocity.z);
						if (!mov.GCV.onSlope)
							mov.Velocity += Vector3.up * jumpSpeed;
						else
							mov.Velocity += mov.GCV.contactNormal * jumpSpeed;
					}
					if (jumpSpeed > 0f)
						mov.JC.jumpPhase++;
				}
				if (mov.JC.coyoteTimer > 0)
					mov.JC.jumpPhase = 1;

				mov.JC.coyoteTimeAvailable = false;
				mov.JC.coyoteTimer = 0;
				mov.WJV.wallJumped = false;
				mov.DV.dashing = false;
				mov.AV.lv.leaping = false;
				mov.JC.onJump.Invoke();
			}
		}
	}

	void TwirlJump()
	{
		PlayerVFX.instance.twirl.gameObject.SetActive(true);
		mov.AV.tj.twirlJumpAnimation = true;
		mov.AV.tj.twirlJumping = true;
		mov.Velocity += Vector3.up * mov.AV.tj.twirlJumpHeight;
	}

	void Kickflip()
	{
		//kickflip
		if (mov.Velocity.y < mov.AV.btv.kickflipHeight)
			mov.Velocity = new Vector3(mov.Velocity.x, 0, mov.Velocity.z);

		mov.Velocity += new Vector3(0, mov.AV.btv.kickflipHeight, 0);

		mov.AV.btv.kickflipCooldownTimer = mov.AV.btv.kickflipCooldown;
		mov.AV.btv.kickFlipAnimation = true;
		mov.AV.btv.activeKickFlipTokens--;
		mov.AV.tsv.twirlSurfing = false;
	}

	void Leap()
	{
		//perform a leap if you're close enough to the ground
		RaycastHit hit;

		if (mov.GCV.grounded || mov.JC.coyoteTime > 0 || Physics.Raycast(mov.RB.position, Vector3.down, out hit, mov.AV.lv.maxDistanceFromGround))
		{
			if (mov.AV.lv.leapingResetsVelocity)
			{
				mov.RB.linearVelocity = Vector3.zero;
				mov.Velocity = Vector3.zero;
			}

			Vector2 playerInput = InputDistributor.playerInputActions.Movement.DirectionalInput.ReadValue<Vector2>();


			float upwardSpeed = mov.AV.lv.upwardSpeed;
			float forwardSpeed = mov.AV.lv.forwardSpeed;

			if (playerInput != Vector2.zero)
			{
				if (mov.GCV.eightWayDirectionInput)
				{
					float inputMagnitude = playerInput.magnitude;
					playerInput = new Vector2(MathF.Round(playerInput.x), MathF.Round(playerInput.y));
					playerInput = playerInput.normalized * inputMagnitude;
				}

				else
				{
					playerInput = Vector2.ClampMagnitude(playerInput, 1f);
				}

				Vector3 cameraDirection = Camera.main.transform.forward;
				cameraDirection.y = 0;
				Vector3 cameraRightDirection = Camera.main.transform.right;
				cameraRightDirection.y = 0;
				Vector3 newMovementVector = mov.ProjectOnContactPlane((cameraDirection * playerInput.y) + cameraRightDirection * playerInput.x).normalized;

				mov.Velocity += new Vector3(newMovementVector.x * forwardSpeed, upwardSpeed, newMovementVector.z * forwardSpeed);
			}
			else
				mov.Velocity += new Vector3(mov.DV.LastHorizontalDirection.x * forwardSpeed, upwardSpeed, mov.DV.LastHorizontalDirection.z * forwardSpeed);


			mov.AV.lv.leapAnimation = true;
			mov.AV.lv.leaping = true;
			mov.AV.lv.leapt = true;
			mov.AV.lv.leapLengthTimer = mov.AV.lv.leapLength;
			mov.AV.lv.leapCoyoteTimer = 0;
			mov.AV.lv.leapControlTimer = mov.AV.lv.leapControlTime;
			mov.JC.jumping = true;
			mov.DV.dashing = false;
			mov.AV.spd.turnOffSpinDash = true;

			if (mov.AV.lv.leapingResetsDash)
				mov.DV.dashed = false;

			mov.AV.lv.onLeap.Invoke();
		}
	}

	void RotatePlayerInstantly(Vector3 dir)
	{
		mov.PlayerVisual.localRotation = Quaternion.Euler(dir);
	}

	public override void ResetValues()
	{
		mov.JC.desiredJump = false;
	}

	public override void UpdateAnimator()
	{
		if (mov.JC.jumping != mov.PlayerAnimator.GetBool("Jumping"))
			mov.PlayerAnimator.SetBool("Jumping", mov.JC.jumping);
	}

	public void StartJump(InputAction.CallbackContext context)
	{
		if (mov.IV.interacting)
			return;

		mov.JC.desiredJump = true;
		mov.JC.jumpBufferTimer = mov.JC.jumpBufferTime;
	}

	public void EndJump(InputAction.CallbackContext context)
	{
		if (mov.IV.interacting)
			return;

		mov.JC.desiredJump = false;
	}

}
