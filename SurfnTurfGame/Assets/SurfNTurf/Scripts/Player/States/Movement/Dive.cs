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
		mov.av.div.diveLengthTimer = mov.av.div.diveLengthTimer.TimerCountdown();
	}

	void HandleDive()
	{
		if (mov.av.div.diving)
		{
			//I want the player to be able to nudge this dive a little, no full control
			if (mov.gcv.contactNormal == Vector3.zero || mov.gcv.contactNormal.y < 0 || mov.gcv.onSlope)
			{
				/*
				//option 1:
				mov.av.div.divingDirection += new Vector3(mov.lastInputDirection3D.x, 0, mov.lastInputDirection3D.z).normalized * mov.av.div.pushPower * Time.deltaTime;
				mov.av.div.divingDirection.Normalize();
				*/

				//option 2:
				mov.av.div.divingDirection = new Vector3(mov.lastInputDirection3D.x, 0, mov.lastInputDirection3D.z).normalized;
			}

			mov.velocity = new Vector3(mov.av.div.divingDirection.x * mov.av.div.forwardSpeed, mov.velocity.y, mov.av.div.divingDirection.z * mov.av.div.forwardSpeed);
		}

		if (mov.av.div.dived)
			mov.av.div.diveAvailable = false;

		if (mov.gcv.groundContactCount == 0 && mov.av.div.diving)
			mov.av.div.leftGround = true;

		if (mov.gcv.grounded && mov.av.div.diveLengthTimer <= 0 || (mov.av.div.leftGround && mov.gcv.allContactNormals.Length > 0))
		{
			mov.av.div.dived = false;
			mov.av.div.diving = false;
			mov.av.div.leftGround = false;
			//end the dive
		}
	}
	public override void ResetValues()
	{
		mov.av.div.diving = false;
	}

	public override void UpdateAnimator()
	{
		mov.animator.SetBool("Diving", mov.av.div.diving);
	}
}
