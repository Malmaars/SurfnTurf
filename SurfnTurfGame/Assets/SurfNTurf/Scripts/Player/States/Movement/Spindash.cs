using SurfnTurf;
using UnityEngine;

public class Spindash : Ability
{
	public Spindash(IMovement _mov) : base(_mov) { }

	public override void RunOnUpdateDuringSetVelocity()
	{
		HandleSpindash();
	}

	public override void UpdateTimers()
	{
		mov.AV.spd.spinDashCoyoteTimer = mov.AV.spd.spinDashCoyoteTimer.TimerCountdown();
		mov.AV.spd.durationTimer = mov.AV.spd.durationTimer.TimerCountdown();
		mov.AV.spd.bounceCooldownTimer = mov.AV.spd.bounceCooldownTimer.TimerCountdown();
	}

	void HandleSpindash()
	{
		if ((mov.AV.spd.spinDashing && mov.AV.spd.durationTimer <= 0) || mov.AV.spd.turnOffSpinDash)
		{
			ResetValues();

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


			//I want the player to be able to nudge this dash a little, no full control
			if (mov.GCV.groundContactCount == 0 || mov.GCV.contactNormal == Vector3.zero || mov.GCV.contactNormal.y < 0 || mov.GCV.onSlope)
			{
				//option 1:
				mov.AV.spd.spindDashDirection += new Vector3(mov.LastInputDirection3D.x, 0, mov.LastInputDirection3D.z).normalized * mov.AV.spd.pushPower * Time.deltaTime;
				mov.AV.spd.spindDashDirection.Normalize();
			}
			else
			{
				//mov.AV.spd.spindDashDirection += mov.ProjectOnContactPlane(new Vector3(mov.LastInputDirection3D.x, 0, mov.LastInputDirection3D.z).normalized).normalized * mov.AV.spd.pushPower * Time.deltaTime;
				mov.AV.spd.spindDashDirection += new Vector3(mov.LastInputDirection3D.x, 0, mov.LastInputDirection3D.z).normalized * mov.AV.spd.pushPower * Time.deltaTime;
				mov.AV.spd.spindDashDirection.Normalize();
			}

			mov.Velocity = new Vector3(mov.AV.spd.spindDashDirection.x * mov.AV.spd.speed, mov.Velocity.y, mov.AV.spd.spindDashDirection.z * mov.AV.spd.speed);

			Collider[] collidersClose = Physics.OverlapSphere(mov.RB.position, mov.SWV.swipeRange);

			foreach (Collider collider in collidersClose)
			{
				if (collider.GetComponent<Destructible>() == null)
					continue;
				else
				{
					collider.GetComponent<Destructible>().Destruct(mov.RB.transform);
				}
			}
		}
	}

	public override void ResetValues()
	{
		mov.AV.spd.spinDashing = false;
		mov.AV.spd.spindDashAnimation = false;
		PlayerVFX.instance.spinner.gameObject.SetActive(false);
	}

	public override void UpdateAnimator()
	{
		mov.PlayerAnimator.SetBool("Spinner", mov.AV.spd.spindDashAnimation);
	}
}
