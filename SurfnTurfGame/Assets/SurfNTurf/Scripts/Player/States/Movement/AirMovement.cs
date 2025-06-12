using FMODUnity;
using SurfnTurf;
using System;
using UnityEngine;

public class AirMovement : Ability
{
	public AirMovement(MovementController _mov) : base(_mov)	{ }

	public override void RunOnUpdateBeforeSetVelocity()
	{
		CheckFalling();
	}

	public override void RunOnUpdateDuringSetVelocity()
	{
		CheckLanding();
		AddGravity();
	}

	public override void UpdateTimers()
	{
		mov.acv.antiAirTimer = mov.acv.antiAirTimer.TimerCountdown();
	}

	void AddGravity()
	{
		if (mov.gcv.grounded == true || mov.dv.gravityOff || mov.lgv.turnGravityOff || mov.av.gv.grindGravityOff)
			return;

		if (mov.rb.linearVelocity.y > mov.acv.maximumDownVelocity && !mov.gcv.onSlope)
		{
			//apply a consistent downforce, perhaps greater than normal gravity
			if (mov.wjv.wallgrab)
				mov.velocity = new Vector3(mov.velocity.x, mov.wjv.wallGrabGravity, mov.velocity.z);
			else if (mov.av.tj.twirlJumping && !mov.jc.jumping)
				mov.velocity = new Vector3(mov.velocity.x, mov.av.tj.twirlJumpGravityStrength, mov.velocity.z);

			else
				mov.velocity += (Vector3.up * mov.acv.customGravityStrength * Time.deltaTime * 100);
		}
	}

	void CheckFalling()
	{
		if (!mov.gcv.grounded && mov.jc.inAir && !mov.jc.jumping && !Physics.Raycast(mov.rb.position, Vector3.down, mov.gcv.groundSnapProbeDistance, mov.gcv.walkableLayers))
		{
			mov.acv.falling = true;
			if (!mov.acv.fallingLoopInstance.isValid())
			{
				mov.acv.fallingLoopInstance = RuntimeManager.CreateInstance(mov.acv.fallingLoop);
			}
			mov.acv.fallingLoopInstance.start();
			
			if (mov.jc.coyoteTimeAvailable)
			{
				mov.jc.coyoteTimer = mov.jc.coyoteTime;
				mov.jc.coyoteTimeAvailable = false;
			}
		}
	}

	void CheckLanding()
	{
		if (mov.gcv.grounded && !mov.jc.jumping && !mov.gcv.onSlope && !mov.jc.hasLanded && mov.acv.falling)
		{
			mov.jc.hasLanded = true;
			RuntimeManager.PlayOneShot(mov.jc.landSound);
			BlackBoard.playerVFX.onJump.SendEvent("OnLand");
			BlackBoard.playerVFX.landTrail.Reinit();
			mov.jc.hasLandedAnimation = true;
			mov.acv.falling = false;
			mov.acv.fallingLoopInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
		}
		if (!mov.gcv.grounded)
			mov.jc.hasLanded = false;
	}
	public override void ResetValues()
	{
		mov.acv.falling = false;
		mov.acv.fallingLoopInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
	}

	public override void UpdateAnimator()
	{
		if (((!mov.gcv.grounded && !mov.jc.jumping && mov.acv.falling) || mov.gcv.onSlope) != mov.animator.GetBool("Falling"))
			mov.animator.SetBool("Falling", ((!mov.gcv.grounded && !mov.jc.jumping && mov.acv.falling) || mov.gcv.onSlope));

	}
}
