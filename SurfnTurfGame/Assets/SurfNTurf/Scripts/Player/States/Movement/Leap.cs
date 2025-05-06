using SurfnTurf;
using System;
using UnityEditor;
using UnityEngine;

public class Leap : Ability
{
	public Leap(MovementController _mov) : base(_mov) { }

	public override void RunOnUpdateDuringSetVelocity()
	{
		HandleLeap();
	}

	public override void UpdateTimers()
	{
		mov.av.lv.leapLengthTimer = mov.av.lv.leapLengthTimer.TimerCountdown();
		mov.av.lv.leapControlTimer = mov.av.lv.leapControlTimer.TimerCountdown();
		mov.av.lv.leapCoyoteTimer = mov.av.lv.leapCoyoteTimer.TimerCountdown();
	}
	void HandleLeap()
	{
		if (mov.av.lv.leapt)
			mov.av.lv.leapAvailable = false;

		if (mov.av.lv.leaping && mov.av.lv.leapLengthTimer <= 0)
				mov.av.lv.leaping = false;

		if (mov.gcv.grounded)
		{
			if (mov.dv.dashingResetsLeap && mov.dv.dashed)
			{
				mov.av.lv.leapCoyoteTimer = mov.av.lv.leapCoyoteTime;
			}

			if (!mov.av.lv.leaping)
			{
				mov.av.lv.leapt = false;
			}
		}
	}

	public override void ResetValues()
	{
		mov.av.lv.leaping = false;
		mov.av.lv.leapt = false;
	}

	public override void UpdateAnimator()
	{
		SetAnimatorTriggers();
	}

	public override void SetAnimatorTriggers()
	{
		if (mov.lgv.ledgeGrabbing)
			return;

		if (mov.av.lv.leapAnimation && mov.av.lv.leaping)
		{
			mov.av.lv.leapAnimation = false;
			mov.animator.SetTrigger("Leap");
		}
	}

}
