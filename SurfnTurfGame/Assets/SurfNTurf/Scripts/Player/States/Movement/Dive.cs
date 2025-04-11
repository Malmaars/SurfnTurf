using UnityEngine;

public class Dive : Ability
{
	public Dive(IMovement _mov) : base(_mov) { }

	public override void RunOnUpdateDuringSetVelocity()
	{
		HandleDive();
	}

	void HandleDive()
	{
		if (mov.AV.div.diving)
		{
			//I want the player to be able to nudge this dive a little, no full control
			if (mov.GCV.contactNormal == Vector3.zero || mov.GCV.contactNormal.y < 0 || mov.GCV.onSlope)
			{
				//option 1:
				mov.AV.div.divingDirection += new Vector3(mov.LastInputDirection3D.x, 0, mov.LastInputDirection3D.z).normalized * mov.AV.div.pushPower * Time.deltaTime;
				mov.AV.div.divingDirection.Normalize();
			}

			mov.Velocity = new Vector3(mov.AV.div.divingDirection.x * mov.AV.div.forwardSpeed, mov.Velocity.y, mov.AV.div.divingDirection.z * mov.AV.div.forwardSpeed);
		}

		if (mov.AV.div.dived)
			mov.AV.div.diveAvailable = false;

		if (mov.AV.div.diveLengthTimer > 0)
			mov.AV.div.diveLengthTimer -= Time.deltaTime;

		if (mov.GCV.grounded && mov.AV.div.diveLengthTimer <= 0)
		{
			mov.AV.div.dived = false;
			mov.AV.div.diving = false;
			//end the dive
		}
	}
}
