using UnityEngine;

public class Spindash : Ability
{
	public Spindash(IMovement _mov) : base(_mov) { }

	public override void RunOnUpdateDuringSetVelocity()
	{
		HandleSpindash();
	}

	void HandleSpindash()
	{
		if (mov.AV.spd.spinDashCoyoteTimer > 0)
			mov.AV.spd.spinDashCoyoteTimer -= Time.deltaTime;

		if (mov.AV.spd.durationTimer > 0)
			mov.AV.spd.durationTimer -= Time.deltaTime;

		if ((mov.AV.spd.spinDashing && mov.AV.spd.durationTimer <= 0) || mov.AV.spd.turnOffSpinDash)
		{
			mov.AV.spd.spinDashing = false;
			mov.AV.spd.spindDashAnimation = false;
			PlayerVFX.instance.twirl.gameObject.SetActive(false);

			mov.DV.dashing = false;
			mov.DV.dashTimer = 0;
			mov.DV.dashControlTimer = 0;
			mov.AV.spd.turnOffSpinDash = false;
		}

		if (mov.AV.spd.spinDashing)
		{
			if (mov.AV.spd.bounceCooldownTimer <= 0)
			{
				foreach (Vector3 normal in mov.GCV.allContactNormals)
				{
					if (normal.y < 0.6f)
					{
						//bounce away from it
						mov.AV.spd.spindDashDirection = Vector3.Reflect(mov.AV.spd.spindDashDirection, normal);
						mov.LastInputDirection3D = mov.AV.spd.spindDashDirection;
						mov.AV.spd.bounceCooldownTimer = mov.AV.spd.bounceCooldown;
						break;
					}
				}
			}

			if (mov.AV.spd.bounceCooldownTimer > 0)
				mov.AV.spd.bounceCooldownTimer -= Time.deltaTime;
			//I want the plaer to be able to nudge this dash a little, no full control
			if (mov.GCV.contactNormal == Vector3.zero || mov.GCV.contactNormal.y < 0 || mov.GCV.onSlope)
			{
				//option 1:
				mov.AV.spd.spindDashDirection += new Vector3(mov.LastInputDirection3D.x, 0, mov.LastInputDirection3D.z).normalized * mov.AV.spd.pushPower * Time.deltaTime;
				mov.AV.spd.spindDashDirection.Normalize();
			}
			else
			{
				mov.AV.spd.spindDashDirection += mov.ProjectOnContactPlane(new Vector3(mov.LastInputDirection3D.x, 0, mov.LastInputDirection3D.z).normalized).normalized * mov.AV.spd.pushPower * Time.deltaTime;
				mov.AV.spd.spindDashDirection.Normalize();
			}

			mov.Velocity = new Vector3(mov.AV.spd.spindDashDirection.x * mov.AV.spd.speed, mov.Velocity.y, mov.AV.spd.spindDashDirection.z * mov.AV.spd.speed);
		}
	}

	public override void UpdateAnimator()
	{
		mov.PlayerAnimator.SetBool("Spinner", mov.AV.spd.spindDashAnimation);
	}
}
