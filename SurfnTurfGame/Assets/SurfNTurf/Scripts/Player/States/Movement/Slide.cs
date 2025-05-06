using SurfnTurf;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class Slide : Ability
{
	public Slide(MovementController _mov) : base(_mov) { }

	public override void RunOnUpdateDuringSetVelocity()
	{
		HandleSlide();
	}

	public override void UpdateTimers()
	{
		mov.av.slv.slideDurationTimer = mov.av.slv.slideDurationTimer.TimerCountdown();
		mov.av.slv.slideCooldownTimer = mov.av.slv.slideCooldownTimer.TimerCountdown();
	}

	void HandleSlide()
	{
		if (mov.av.slv.sliding && mov.av.slv.slideDurationTimer <= 0)
			mov.av.slv.sliding = false;

		if(mov.av.slv.slid && !mov.suv.surfing)
			mov.av.slv.slid = false;

		if (mov.av.slv.slid && mov.av.slv.hasCoolddown && mov.av.slv.slideCooldownTimer <= 0)
			mov.av.slv.slid = false;
	}

    public override void UpdateAnimator()
    {
		SetAnimatorTriggers();
    }

	public override void SetAnimatorTriggers()
	{
		if (mov.lgv.ledgeGrabbing)
			return;
		if (mov.av.slv.slideAnimation)
		{
			mov.av.slv.slideAnimation = false;
			mov.animator.SetTrigger("SurfDash");
		}

	}
}
