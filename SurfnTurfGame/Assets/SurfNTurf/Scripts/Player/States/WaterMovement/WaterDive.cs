using UnityEngine;
using SurfnTurf;
public class WaterDive : WaterAbility
{
	public WaterDive(WaterMovementController _mov) : base(_mov)
	{
	}

	public override void RunOnUpdateDuringSetVelocity()
	{
		HandleDive();
	}

	public override void UpdateTimers()
	{
		mov.div.diveLengthTimer = mov.div.diveLengthTimer.TimerCountdown();
	}

	void HandleDive()
	{
		if (mov.div.diving)
		{
			mov.velocity = new Vector3(mov.div.divingDirection.x * mov.div.forwardSpeed, mov.velocity.y, mov.div.divingDirection.z * mov.div.forwardSpeed);
		}

		if (mov.div.dived)
			mov.div.diveAvailable = false;

		if (mov.wv.waterContactCount == 0 && mov.div.diving)
			mov.div.leftGround = true;

		if (mov.wv.onWater && mov.div.diveLengthTimer <= 0 || (mov.div.leftGround && mov.wv.allContactNormals.Length > 0))
		{
			mov.div.dived = false;
			mov.div.diving = false;
			mov.div.leftGround = false;
			//end the dive
		}
	}
	public override void ResetValues()
	{
		mov.div.diving = false;
	}

	public override void UpdateAnimator()
	{
		mov.animator.SetBool("Diving", mov.div.diving);
	}
}
