using SurfnTurf;
using UnityEngine;

public class WaterAirControl : WaterAbility
{
	public WaterAirControl(WaterMovementController _mov) : base(_mov) { }


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
		if (mov.wv.onWater == true)
			return;

		if (mov.rb.linearVelocity.y > mov.acv.maximumDownVelocity)
		{
			//apply a consistent downforce, perhaps greater than normal gravity
				mov.velocity += (Vector3.up * mov.acv.customGravityStrength * Time.deltaTime * 100);
		}
	}

	void CheckFalling()
	{
		if (!mov.wv.onWater)
		{
			mov.acv.falling = true;
		}
	}

	void CheckLanding()
	{
		if (mov.wv.onWater && mov.acv.falling)
		{
			mov.acv.falling = false;
		}
	}
	public override void ResetValues()
	{
		mov.acv.falling = false;
	}

	public override void UpdateAnimator()
	{
		if ((!mov.wv.onWater && mov.acv.falling && mov.velocity.y <= 0) != mov.animator.GetBool("Falling"))
			mov.animator.SetBool("Falling", (!mov.wv.onWater && mov.acv.falling && mov.velocity.y <= 0));

	}
}
