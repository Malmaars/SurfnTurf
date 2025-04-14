using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class Slide : Ability
{
	public Slide(IMovement _mov) : base(_mov) { }

	public override void RunOnUpdateDuringSetVelocity()
	{
		HandleSlide();
	}

	void HandleSlide()
	{
		if (mov.AV.slv.slideDurationTimer > 0)
			mov.AV.slv.slideDurationTimer -= Time.deltaTime;

		if(mov.AV.slv.slideCooldownTimer > 0)
			mov.AV.slv.slideCooldownTimer -= Time.deltaTime;

		if (mov.AV.slv.sliding && mov.AV.slv.slideDurationTimer <= 0)
			mov.AV.slv.sliding = false;

		if(mov.AV.slv.slid && !mov.SUV.surfing)
			mov.AV.slv.slid = false;

		if (mov.AV.slv.slid && mov.AV.slv.hasCoolddown && mov.AV.slv.slideCooldownTimer <= 0)
			mov.AV.slv.slid = false;
	}

    public override void UpdateAnimator()
    {
		if (mov.AV.slv.slideAnimation)
		{
			mov.AV.slv.slideAnimation = false;
			mov.PlayerAnimator.SetTrigger("SurfDash");
		}
    }
}
