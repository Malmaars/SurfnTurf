using SurfnTurf;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class Slide : Ability
{
	public Slide(IMovement _mov) : base(_mov) { }

	public override void RunOnUpdateDuringSetVelocity()
	{
		HandleSlide();
	}

	public override void UpdateTimers()
	{
		mov.AV.slv.slideDurationTimer = mov.AV.slv.slideDurationTimer.TimerCountdown();
		mov.AV.slv.slideCooldownTimer = mov.AV.slv.slideCooldownTimer.TimerCountdown();
	}

	void HandleSlide()
	{
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
