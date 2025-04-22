using SurfnTurf;
using UnityEngine;

public class Spindash : Ability
{
	public Spindash(MovementController _mov) : base(_mov) { }

	public override void RunOnUpdateDuringSetVelocity()
	{
		HandleSpindash();
	}

	public override void UpdateTimers()
	{
		mov.av.spd.spinDashCoyoteTimer = mov.av.spd.spinDashCoyoteTimer.TimerCountdown();
		mov.av.spd.durationTimer = mov.av.spd.durationTimer.TimerCountdown();
		mov.av.spd.bounceCooldownTimer = mov.av.spd.bounceCooldownTimer.TimerCountdown();
	}

	void HandleSpindash()
	{
		if ((mov.av.spd.spinDashing && mov.av.spd.durationTimer <= 0) || mov.av.spd.turnOffSpinDash)
		{
			ResetValues();

			mov.dv.dashing = false;
			mov.dv.dashTimer = 0;
			mov.dv.dashControlTimer = 0;
			mov.av.spd.turnOffSpinDash = false;
		}

		if (mov.av.spd.spinDashing)
		{
			if (mov.av.spd.bounceCooldownTimer <= 0)
			{
				foreach (Vector3 normal in mov.gcv.allContactNormals)
				{
					if (normal.y < 0.6f)
					{
						//bounce away from it
						mov.av.spd.spindDashDirection = Vector3.Reflect(mov.av.spd.spindDashDirection, normal);
						mov.lastInputDirection3D = mov.av.spd.spindDashDirection;
						mov.av.spd.bounceCooldownTimer = mov.av.spd.bounceCooldown;
						if (mov.av.spd.hitResetsDuration)
							mov.av.spd.durationTimer = mov.av.spd.duration;
						break;
					}
				}
			}


			//I want the player to be able to nudge this dash a little, no full control
			if (mov.gcv.groundContactCount == 0 || mov.gcv.contactNormal == Vector3.zero || mov.gcv.contactNormal.y < 0 || mov.gcv.onSlope)
			{
				//option 1:
				mov.av.spd.spindDashDirection += new Vector3(mov.lastInputDirection3D.x, 0, mov.lastInputDirection3D.z).normalized * mov.av.spd.pushPower * Time.deltaTime;
				mov.av.spd.spindDashDirection.Normalize();
			}
			else
			{
				//mov.av.spd.spindDashDirection += mov.ProjectOnContactPlane(new Vector3(mov.lastInputDirection3D.x, 0, mov.lastInputDirection3D.z).normalized).normalized * mov.av.spd.pushPower * Time.deltaTime;
				mov.av.spd.spindDashDirection += new Vector3(mov.lastInputDirection3D.x, 0, mov.lastInputDirection3D.z).normalized * mov.av.spd.pushPower * Time.deltaTime;
				mov.av.spd.spindDashDirection.Normalize();
			}

			mov.velocity = new Vector3(mov.av.spd.spindDashDirection.x * mov.av.spd.speed, mov.velocity.y, mov.av.spd.spindDashDirection.z * mov.av.spd.speed);

			Collider[] collidersClose = Physics.OverlapSphere(mov.rb.position, mov.swv.swipeRange);

			foreach (Collider collider in collidersClose)
			{
				if (collider.GetComponent<Destructible>() == null)
					continue;
				else
				{
					collider.GetComponent<Destructible>().Destruct(mov.rb.transform);
				}
			}
		}
	}

	public override void ResetValues()
	{
		mov.av.spd.spinDashing = false;
		mov.av.spd.spindDashAnimation = false;
		PlayerVFX.instance.spinner.gameObject.SetActive(false);
	}

	public override void UpdateAnimator()
	{
		mov.animator.SetBool("Spinner", mov.av.spd.spindDashAnimation);
	}
}
