using System;
using Unity.VisualScripting;
using UnityEngine;

public class TwirlJump : Ability
{
	public TwirlJump(MovementController _mov) : base(_mov) { }

	public override void RunOnUpdateDuringSetVelocity()
	{
		HandleTwirlJump();
	}
	void HandleTwirlJump()
	{
		if ((mov.av.tj.twirlJumping && (mov.gcv.grounded && !mov.jc.jumping || mov.dv.dashing || mov.suv.surfing)) || mov.av.tj.turnOffTwirlJump)
		{
			mov.av.tj.twirlJumping = false;
			mov.animator.SetBool("Twirling", false);
			PlayerVFX.instance.twirl.gameObject.SetActive(false);
			mov.av.tj.turnOffTwirlJump = false;
		}
	}

	public override void ResetValues()
	{
		mov.av.tj.twirlJumping = false;
		mov.av.tj.turnOffTwirlJump = true;
		mov.animator.SetBool("Twirling", false);
	}

	public override void UpdateAnimator()
	{
		SetAnimatorTriggers();
		if (mov.av.tj.twirlJumpAnimation)
		{
			mov.animator.SetBool("Twirling", true);
			mov.av.tj.twirlJumpAnimation = false;
			mov.av.tj.twirlJumpLoopInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
		}

	}

	public override void SetAnimatorTriggers()
	{
		if (mov.lgv.ledgeGrabbing)
			return;

		if (mov.av.tj.twirlJumpAnimation)
		{
			mov.animator.SetTrigger("Twirl");
		}
	}
}
