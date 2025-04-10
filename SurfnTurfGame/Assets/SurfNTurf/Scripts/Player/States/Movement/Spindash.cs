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
	}

	public override void UpdateAnimator()
	{
		mov.PlayerAnimator.SetBool("Spinner", mov.AV.spd.spindDashAnimation);
	}
}
