using System;
using UnityEngine;

public class AirMovement : Ability
{
	public AirMovement(IMovement _mov) : base(_mov)	{ }

	public override void RunOnUpdateBeforeSetVelocity()
	{
		CheckFalling();
	}

	public override void RunOnUpdateDuringSetVelocity()
	{
		CheckLanding();
		AddGravity();
	}

	void AddGravity()
	{
		if (mov.GCV.grounded == true || mov.DV.gravityOff)
			return;

		if (mov.RB.linearVelocity.y > mov.ACV.maximumDownVelocity && !mov.GCV.onSlope)
		{
			//apply a consistent downforce, perhaps greater than normal gravity
			if (mov.WJV.wallgrab)
				mov.Velocity = new Vector3(mov.Velocity.x, mov.WJV.wallGrabGravity, mov.Velocity.z);
			else if (mov.AV.tj.twirlJumping && !mov.JC.jumping)
				mov.Velocity = new Vector3(mov.Velocity.x, mov.AV.tj.twirlJumpGravityStrength, mov.Velocity.z);
			
			else 
				mov.RB.AddForce(Vector3.up * mov.ACV.customGravityStrength * Time.deltaTime * 100); 

		}
	}

	void CheckFalling()
	{
		if (!mov.GCV.grounded && mov.JC.inAir && !mov.JC.jumping && !Physics.Raycast(mov.RB.position, Vector3.down, mov.GCV.groundSnapProbeDistance))
		{
			mov.ACV.falling = true;
			if (mov.JC.coyoteTimeAvailable)
			{
				mov.JC.coyoteTimer = mov.JC.coyoteTime;
				mov.JC.coyoteTimeAvailable = false;
			}
		}
	}

	void CheckLanding()
	{
		if (mov.GCV.grounded && !mov.JC.jumping && !mov.GCV.onSlope && !mov.JC.hasLanded && mov.ACV.falling)
		{
			mov.JC.hasLanded = true;
			mov.JC.hasLandedAnimation = true;
			mov.ACV.falling = false;
		}
		if (!mov.GCV.grounded)
			mov.JC.hasLanded = false;
	}

	public override void UpdateAnimator()
	{
		if (((!mov.GCV.grounded && !mov.JC.jumping && mov.ACV.falling) || mov.GCV.onSlope) != mov.PlayerAnimator.GetBool("Falling"))
			mov.PlayerAnimator.SetBool("Falling", ((!mov.GCV.grounded && !mov.JC.jumping && mov.ACV.falling) || mov.GCV.onSlope));

	}
}
