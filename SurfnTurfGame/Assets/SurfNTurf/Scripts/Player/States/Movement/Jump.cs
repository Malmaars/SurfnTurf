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
		mov.jc.jumpDirection = Vector3.zero;
	}

	public override void RunOnUpdateDuringSetVelocity()
	{
		HandleJumping();
	}

	public override void UpdateTimers()
	{
		mov.jc.jumpBufferTimer = mov.jc.jumpBufferTimer.TimerCountdown();
		mov.jc.coyoteTimer = mov.jc.coyoteTimer.TimerCountdown();
	}

	void HandleJumping()
	{
		if (mov.gcv.grounded && !mov.jc.jumping)
			mov.jc.jumpPhase = 0;

		if (!mov.gcv.grounded && mov.jc.jumping)
			mov.jc.coyoteTimeAvailable = false;

		if (mov.jc.desiredJump || (mov.gcv.grounded && mov.jc.jumpBufferTimer > 0 && !mov.jc.jumping))
			DoJump();


		if (mov.jc.jumping && mov.velocity.y < 0f)
		{
			mov.jc.jumping = false;
		}
	}

	void DoJump()
	{
		mov.jc.desiredJump = false;

		if (!mov.jc.active)
			return;

		int newMaxAirJumps = mov.jc.maxAirJumps;
		newMaxAirJumps = (mov.dv.dashingGivesExtraJump && mov.dv.dashed && mov.dv.dashCoyoteTimer > 0) ? newMaxAirJumps + 1 : newMaxAirJumps;

		if (mov.suv.surfing)
		{
			if (mov.av.btv.active && mov.av.btv.kickflipCooldownTimer <= 0 && mov.av.btv.activeKickFlipTokens > 0)
			{
				Kickflip();
			}
			return;
		}

		if (mov.gcv.grounded)
			mov.av.sp.jumpParryCoyoteTimer = mov.av.sp.jumpParryCoyoteTime;

		if (mov.av.lv.active 
			&& mov.av.lv.leapAvailable
			&& !mov.wjv.wallgrab 
			&& !mov.av.lv.leaping 
			&& !mov.wjv.wallRiding 
			&& !mov.av.lv.leapt 
			&& !mov.gcv.onSlope 
			&& (((mov.dv.dashing || mov.av.lv.leapCoyoteTimer > 0) || mov.av.spd.spinDashing) 
				&& (mov.gcv.grounded || Physics.Raycast(mov.rb.position, Vector3.down, mov.av.lv.maxDistanceFromGround) || (mov.jc.coyoteTimer > 0 && mov.dv.startedDashOnGround)) || (mov.dv.dashed && (mov.gcv.grounded || Physics.Raycast(mov.rb.position, Vector3.down, mov.av.lv.maxDistanceFromGround)) && mov.jc.jumpBufferTimer > 0)))
		{
			Leap();
			return;
		}
		else if (mov.wjv.active &&  (mov.wjv.wallgrab || mov.wjv.wallRiding || mov.wjv.wallJumpCoyoteTimer > 0))
		{
			mov.jc.jumping = true;

			Vector3 newDir = (mov.wjv.jumpDirection + Vector3.up);
			newDir.Normalize();
			newDir = new Vector3(newDir.x, Mathf.Tan(Mathf.Deg2Rad * mov.wjv.walljumpAngle), newDir.z);

			mov.lastInputDirection3D = new Vector3(newDir.x, 0, newDir.z).normalized;
			mov.velocity = Vector3.zero;
			mov.velocity += newDir * mov.wjv.wallJumpForce;
			mov.acv.antiAirTimer = mov.wjv.wallJumpAntiAirTimer;
			mov.wjv.wallJumpCooldownTimer = mov.wjv.wallJumpCooldown;
			mov.wjv.wallJumpCoyoteTimer = 0;
			mov.wjv.wallJumped = true;
			mov.wjv.wallgrab = false;
			mov.wjv.wallRiding = false;
			mov.wjv.wallJumpLimitVelocity = true;
			mov.wjv.onWallJump.Invoke();
			mov.wjv.wallJumpAnimation = true;
			RotatePlayerInstantly(new Vector3(newDir.x, 0, newDir.z).normalized);
			mov.jc.jumpPhase = 1;
		}
		else
		{
			if (mov.dv.breakDashWithJump && !mov.dv.airJumped && mov.jc.inAir && (mov.dv.dashing || mov.dv.dashCoyoteTimer > 0))
			{
				mov.dv.airJumped = true;
				mov.velocity = Vector3.zero;
				mov.dv.dashing = false;
			}

			if ((mov.gcv.grounded || mov.jc.jumpPhase <= newMaxAirJumps || mov.jc.coyoteTimer > 0))
			{
				mov.jc.jumpBufferTimer = 0;
				mov.jc.jumping = true;

				float jumpSpeed = mov.jc.jumpHeight;

				if (!mov.wjv.wallgrab)
				{
					mov.velocity = new Vector3(mov.velocity.x, 0, mov.velocity.z);
					if (!mov.gcv.onSlope)
					{
						if (mov.av.tj.active && mov.swv.swiping)
							TwirlJump();
						else
							mov.velocity += Vector3.up * jumpSpeed;
					}
					else
					{
						mov.velocity = new Vector3(mov.velocity.x, 0, mov.velocity.z);
						if (!mov.gcv.onSlope)
							mov.velocity += Vector3.up * jumpSpeed;
						else
							mov.velocity += mov.gcv.contactNormal * jumpSpeed;
					}
					if (jumpSpeed > 0f)
						mov.jc.jumpPhase++;
				}
				if (mov.jc.coyoteTimer > 0)
					mov.jc.jumpPhase = 1;

				mov.jc.coyoteTimeAvailable = false;
				mov.jc.coyoteTimer = 0;
				mov.wjv.wallJumped = false;
				mov.dv.dashing = false;
				mov.av.lv.leaping = false;
				mov.jc.onJump.Invoke();
			}
		}
	}

	void TwirlJump()
	{
		PlayerVFX.instance.twirl.gameObject.SetActive(true);
		mov.av.tj.twirlJumpAnimation = true;
		mov.av.tj.twirlJumping = true;
		mov.velocity += Vector3.up * mov.av.tj.twirlJumpHeight;

        ComboMeter.AddToCombo("Twirl Jump");
    }

    void Kickflip()
	{
		//kickflip
		if (mov.velocity.y < mov.av.btv.kickflipHeight)
			mov.velocity = new Vector3(mov.velocity.x, 0, mov.velocity.z);

		mov.velocity += new Vector3(0, mov.av.btv.kickflipHeight, 0);

		mov.av.btv.kickflipCooldownTimer = mov.av.btv.kickflipCooldown;
		mov.av.btv.kickFlipAnimation = true;
		mov.av.btv.activeKickFlipTokens--;
		mov.av.tsv.twirlSurfing = false;

        ComboMeter.AddToCombo("Kickflip");
    }

    void Leap()
	{
		//perform a leap if you're close enough to the ground
		RaycastHit hit;

		if (mov.gcv.grounded || mov.jc.coyoteTime > 0 || Physics.Raycast(mov.rb.position, Vector3.down, out hit, mov.av.lv.maxDistanceFromGround))
		{
			if (mov.av.lv.leapingResetsVelocity)
			{
				mov.rb.linearVelocity = Vector3.zero;
				mov.velocity = Vector3.zero;
			}

			Vector2 playerInput = InputDistributor.playerInputActions.Movement.DirectionalInput.ReadValue<Vector2>();


			float upwardSpeed = mov.av.lv.upwardSpeed;
			float forwardSpeed = mov.av.lv.forwardSpeed;

			if (playerInput != Vector2.zero)
			{
				if (mov.gcv.eightWayDirectionInput)
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

				mov.velocity += new Vector3(newMovementVector.x * forwardSpeed, upwardSpeed, newMovementVector.z * forwardSpeed);
			}
			else
				mov.velocity += new Vector3(mov.dv.LastHorizontalDirection.x * forwardSpeed, upwardSpeed, mov.dv.LastHorizontalDirection.z * forwardSpeed);


			mov.av.lv.leapAnimation = true;
			mov.av.lv.leaping = true;
			mov.av.lv.leapt = true;
			mov.av.lv.leapLengthTimer = mov.av.lv.leapLength;
			mov.av.lv.leapCoyoteTimer = 0;
			mov.av.lv.leapControlTimer = mov.av.lv.leapControlTime;
			mov.jc.jumping = true;
			mov.dv.dashing = false;
			mov.av.spd.turnOffSpinDash = true;

            ComboMeter.AddToCombo("Leap");

            if (mov.av.lv.leapingResetsDash)
				mov.dv.dashed = false;

			mov.av.lv.onLeap.Invoke();
		}
	}

	void RotatePlayerInstantly(Vector3 dir)
	{
		mov.playerVisual.localRotation = Quaternion.Euler(dir);
	}

	public override void ResetValues()
	{
		mov.jc.desiredJump = false;
	}

	public override void UpdateAnimator()
	{
		if (mov.jc.jumping != mov.animator.GetBool("Jumping"))
			mov.animator.SetBool("Jumping", mov.jc.jumping);
	}

	public void StartJump(InputAction.CallbackContext context)
	{
		if (mov.iv.interacting)
			return;

		mov.jc.desiredJump = true;
		mov.jc.jumpBufferTimer = mov.jc.jumpBufferTime;
	}

	public void EndJump(InputAction.CallbackContext context)
	{
		if (mov.iv.interacting)
			return;

		mov.jc.desiredJump = false;
	}

}
