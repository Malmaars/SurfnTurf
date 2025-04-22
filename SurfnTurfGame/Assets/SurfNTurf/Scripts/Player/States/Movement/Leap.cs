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
		mov.AV.lv.leapLengthTimer = mov.AV.lv.leapLengthTimer.TimerCountdown();
		mov.AV.lv.leapControlTimer = mov.AV.lv.leapControlTimer.TimerCountdown();
		mov.AV.lv.leapCoyoteTimer = mov.AV.lv.leapCoyoteTimer.TimerCountdown();
	}
	void HandleLeap()
	{
		if (mov.AV.lv.leapt)
			mov.AV.lv.leapAvailable = false;

		if (mov.AV.lv.leaping && mov.AV.lv.leapLengthTimer <= 0)
				mov.AV.lv.leaping = false;

		if (mov.GCV.grounded)
		{
			if (mov.DV.dashingResetsLeap && mov.DV.dashed)
			{
				mov.AV.lv.leapCoyoteTimer = mov.AV.lv.leapCoyoteTime;
			}

			if (!mov.AV.lv.leaping)
			{
				mov.AV.lv.leapt = false;
			}
		}
	}

	public override void ResetValues()
	{
		mov.AV.lv.leaping = false;
		mov.AV.lv.leapt = false;
	}

	public override void UpdateAnimator()
	{
		if (mov.AV.lv.leapAnimation && mov.AV.lv.leaping)
		{
			mov.AV.lv.leapAnimation = false;
			mov.PlayerAnimator.SetTrigger("Leap");
		}
	}

}
