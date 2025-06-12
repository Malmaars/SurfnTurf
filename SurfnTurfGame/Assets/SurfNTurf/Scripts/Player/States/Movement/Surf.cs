using FMODUnity;
using SurfnTurf;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Surf : Ability
{
	public Surf(MovementController _mov) : base(_mov) { }

	public override void RunOnEnterState()
	{
		InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Movement.Surf, StartSurf);
		InputDistributor.inputManager.AddActionToInputCancelled(InputDistributor.playerInputActions.Movement.Surf, EndSurf);

		//check if surfing is true
		if (InputDistributor.playerInputActions.Movement.Surf.IsPressed())
		{
			DoSurf();
		}
	}

	public override void RunOnExitState()
	{
		InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Movement.Surf, StartSurf);
		InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Movement.Surf, EndSurf);
	}

	public override void RunOnUpdateDuringSetVelocity()
	{
		HandleSurfing();
		HandleGroundParry();
	}
	void StartSurf(InputAction.CallbackContext context)
	{
		if (mov.suv.surfCooldownTimer > 0)
			return;

		mov.suv.desiredSurf = true;
		mov.suv.startSurfBufferTimer = mov.suv.startSurfBuffer;
	}

	void EndSurf(InputAction.CallbackContext context)
	{
		if (!mov.suv.surfing)
			return;

		mov.suv.desiredSurf = false;
		mov.suv.surfing = false;
		mov.suv.surfingOnLandLoopInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
		mov.suv.surfCooldownTimer = mov.suv.surfCooldown;
	}

	public override void UpdateTimers()
	{
		mov.av.sp.jumpParryCoyoteTimer = mov.av.sp.jumpParryCoyoteTimer.TimerCountdown();
		mov.suv.surfCooldownTimer = mov.suv.surfCooldownTimer.TimerCountdown();
		mov.suv.startSurfBufferTimer = mov.suv.startSurfBufferTimer.TimerCountdown();
	}

	void HandleSurfing()
	{
		if (mov.lgv.ledgeGrabbing)
		{
			mov.suv.desiredSurf = false;
			mov.suv.surfing = false;
			mov.suv.surfingOnLandLoopInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
			mov.suv.surfCooldownTimer = mov.suv.surfCooldown;
			return;
		}

		if (mov.suv.desiredSurf)
		{
			mov.suv.desiredSurf = false;

			if (!mov.suv.active)
				return;

			if (mov.av.tsv.active && (mov.swv.swiping || mov.av.spd.spinDashing))
			{
				TwirlSurf();
			}

			if (mov.av.sp.active)
			{
				if (!mov.gcv.grounded && mov.rb.linearVelocity.y < 0 && mov.rb.linearVelocity.magnitude > mov.av.sp.minimumVelocityToParry)
					mov.av.sp.parryIsReady = true;
				else
					mov.av.sp.parryIsReady = false;

				if (mov.dv.dashing && (mov.gcv.grounded || Physics.Raycast(mov.rb.position, Vector3.down, mov.av.sp.distanceFromGroundForDashParry, mov.gcv.walkableLayers)))
					mov.av.sp.dashParryIsReady = true;
				if (mov.av.sp.jumpParryCoyoteTimer > 0)
					mov.av.sp.jumpParryIsReady = true;
			}
			DoSurf();
		}

		else
		{
			if (mov.suv.surfing && mov.gcv.grounded && !mov.av.gv.grinding)
			{
				if (mov.gcv.contactNormal.y < mov.suv.groundNormalThreshold)
				{
					mov.velocity += mov.ProjectOnContactPlane(Vector3.down).normalized * mov.suv.accelarationSpeed * Time.deltaTime * (1 - mov.gcv.contactNormal.y);
				}
				else
				{
					//slow down
					mov.velocity -= mov.velocity.normalized * mov.suv.decelerationSpeed * Time.deltaTime * mov.gcv.contactNormal.y;
				}

				float velocityMag = mov.velocity.magnitude;

				//slightly change the angle of the surf
				mov.velocity = (mov.velocity.normalized + (mov.lastInputDirection3D * mov.suv.steeringStrength * Time.deltaTime)).normalized * velocityMag;

				if (new Vector3(mov.velocity.x, 0, mov.velocity.z).sqrMagnitude < 0.01f)
					mov.velocity = new Vector3(0, mov.velocity.y, 0);

			}
		}
	}

	void HandleGroundParry()
	{
		if (mov.av.sp.parryIsReady && mov.gcv.grounded && mov.suv.startSurfBufferTimer > 0)
		{
			//perform a ground parry
			ParryGround();

			mov.suv.startSurfBufferTimer = 0;
			mov.av.sp.jumpParryIsReady = false;
			mov.av.sp.dashParryIsReady = false;
			mov.av.sp.parryIsReady = false;
		}

		else if (mov.av.sp.dashParryIsReady)
		{
			DashParry();
			mov.av.sp.jumpParryIsReady = false;
			mov.av.sp.dashParryIsReady = false;
			mov.av.sp.parryIsReady = false;
		}

		else if (mov.av.sp.jumpParryIsReady)
		{
			ParryGround();
			mov.av.sp.jumpParryIsReady = false;
			mov.av.sp.dashParryIsReady = false;
			mov.av.sp.parryIsReady = false;
		}
	}

	void ParryGround()
	{

		mov.velocity = new Vector3(mov.velocity.x, 0, mov.velocity.z);

		if (mov.av.sp.goInNormalDirection)
			mov.velocity += mov.gcv.contactNormal * mov.av.sp.surfParryJumpHeight;
		else
		{
			mov.velocity += Vector3.up * mov.av.sp.surfParryJumpHeight;
		}

		mov.av.sp.parryAnimation = true;

		PlayerVFX.instance.parrySpark.SendEvent("OnPlay");
		mov.av.sp.OnParry.Invoke();
	}

	void DashParry()
	{
		mov.dv.dashing = false;
		Vector3 newVelocityDirection = new Vector3(mov.velocity.x, 0, mov.velocity.z).normalized * mov.av.sp.dashParryForwardSpeed;

		mov.velocity = new Vector3(newVelocityDirection.x, mov.av.sp.dashParryHeight, newVelocityDirection.z);
		mov.av.sp.parryAnimation = true;

		PlayerVFX.instance.parrySpark.SendEvent("OnPlay");
		mov.av.sp.dashParryIsReady = false;
	}

	void DoSurf()
	{
		mov.suv.surfing = true;
		if (!mov.suv.surfingOnLandLoopInstance.isValid())
		{ RuntimeManager.CreateInstance(mov.suv.surfingOnLandLoop); }
		mov.suv.surfingOnLandLoopInstance.start();
		mov.av.tj.turnOffTwirlJump = true;
		mov.swv.swiping = false;
	}

	void TwirlSurf()
	{
		mov.av.spd.turnOffSpinDash = true;

		if (mov.velocity.magnitude < mov.av.tsv.maximumVelocityMagnitudeForStartBoost)
			mov.velocity += new Vector3(mov.velocity.x, 0, mov.velocity.z).normalized * mov.av.tsv.startBoost;
		mov.av.tsv.twirlSurfing = true;
		mov.av.tsv.twirlSurfAnimation = true;
		if (!mov.av.tsv.twirlSurfLoopInstance.isValid())
		{ mov.av.tsv.twirlSurfLoopInstance = RuntimeManager.CreateInstance(mov.av.tsv.twirlSurfLoop); }
		mov.av.tsv.twirlSurfLoopInstance.start();
		mov.swv.swiping = false;
	}

	public override void ResetValues()
	{
		mov.suv.surfing = false;
		mov.suv.surfingOnLandLoopInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
	}
	public override void UpdateAnimator()
	{
		SetAnimatorTriggers();

		mov.animator.SetBool("Surfing", mov.suv.surfing);
		BlackBoard.playerVFX.landTrail.SetBool("On", mov.suv.surfing && mov.gcv.grounded);
		SurfBoardManager.instance.ToggleSurfboard(mov.suv.surfing);
	}

	public override void SetAnimatorTriggers()
	{
		if (mov.lgv.ledgeGrabbing)
			return;
		if (mov.suv.surfing && !mov.animator.GetBool("Surfing") && !mov.av.sp.parryAnimation)
		{
			BlackBoard.playerVFX.landTrail.Reinit();
			mov.animator.SetTrigger("Surf");
			Debug.Log(" play the land trail");
		}
	}
}
