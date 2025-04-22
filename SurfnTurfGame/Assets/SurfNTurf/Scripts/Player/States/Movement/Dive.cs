using SurfnTurf;
using UnityEngine;

public class Dive : Ability
{
	public Dive(MovementController _mov) : base(_mov) { }

	public override void RunOnUpdateDuringSetVelocity()
	{
		HandleDive();
	}

	public override void UpdateTimers()
	{
		mov.AV.div.diveLengthTimer = mov.AV.div.diveLengthTimer.TimerCountdown();
	}

	void HandleDive()
	{
		if (mov.AV.div.diving)
		{
			//I want the player to be able to nudge this dive a little, no full control
			if (mov.GCV.contactNormal == Vector3.zero || mov.GCV.contactNormal.y < 0 || mov.GCV.onSlope)
			{
				/*
				//option 1:
				mov.AV.div.divingDirection += new Vector3(mov.LastInputDirection3D.x, 0, mov.LastInputDirection3D.z).normalized * mov.AV.div.pushPower * Time.deltaTime;
				mov.AV.div.divingDirection.Normalize();
				*/

				//option 2:
				mov.AV.div.divingDirection = new Vector3(mov.LastInputDirection3D.x, 0, mov.LastInputDirection3D.z).normalized;
			}

			mov.Velocity = new Vector3(mov.AV.div.divingDirection.x * mov.AV.div.forwardSpeed, mov.Velocity.y, mov.AV.div.divingDirection.z * mov.AV.div.forwardSpeed);
		}

		if (mov.AV.div.dived)
			mov.AV.div.diveAvailable = false;

		if (mov.GCV.groundContactCount == 0 && mov.AV.div.diving)
			mov.AV.div.leftGround = true;

		if (mov.GCV.grounded && mov.AV.div.diveLengthTimer <= 0 || (mov.AV.div.leftGround && mov.GCV.allContactNormals.Length > 0))
		{
			mov.AV.div.dived = false;
			mov.AV.div.diving = false;
			mov.AV.div.leftGround = false;
			//end the dive
		}
	}
	public override void ResetValues()
	{
		mov.AV.div.diving = false;
	}

	public override void UpdateAnimator()
	{
		mov.PlayerAnimator.SetBool("Diving", mov.AV.div.diving);
	}
}
