using System;
using UnityEditor;
using UnityEngine;

public class Leap : Ability
{
	public Leap(IMovement _mov) : base(_mov) { }

	public override void RunOnUpdateDuringSetVelocity()
	{
		HandleLeap();
	}
	void HandleLeap()
	{
		if (mov.AV.lv.leapt)
			mov.AV.lv.leapAvailable = false;

		if (mov.AV.lv.leapLengthTimer > 0)
		{
			mov.AV.lv.leapLengthTimer -= Time.deltaTime;
			if (mov.AV.lv.leapLengthTimer <= 0)
				mov.AV.lv.leaping = false;
		}

		if (mov.AV.lv.leapControlTimer > 0)
			mov.AV.lv.leapControlTimer -= Time.deltaTime;

		if (mov.AV.lv.leapCoyoteTimer > 0)
			mov.AV.lv.leapCoyoteTimer -= Time.deltaTime;

		if (mov.GCV.grounded)
		{
			if (mov.DV.dashingResetsLeap && mov.DV.dashed && mov.DV.dashTimer <= 0)
			{
				mov.AV.lv.leapCoyoteTimer = mov.AV.lv.leapCoyoteTime;
			}

			if (!mov.AV.lv.leaping)
			{
				mov.AV.lv.leapt = false;
			}
		}
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
