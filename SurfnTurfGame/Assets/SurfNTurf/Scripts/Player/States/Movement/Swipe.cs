using SurfnTurf;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Swipe : Ability
{
	public Swipe(MovementController _mov) : base(_mov) { }

	public override void RunOnEnterState()
	{
		InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Movement.Swipe, StartSwipe);
		InputDistributor.inputManager.AddActionToInputCancelled(InputDistributor.playerInputActions.Movement.Swipe, EndSwipe);
	}

	public override void RunOnExitState()
	{
		InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Movement.Swipe, StartSwipe);
		InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Movement.Swipe, EndSwipe);
	}

	public override void RunOnUpdateDuringSetVelocity()
	{
		HandleSwipe();
		HandleSwipeDoubleJump();
	}

	public override void RunOnDrawGizmos()
	{
		if (mov.swv.gizmosOn)
		{
			Gizmos.color = Color.red;

			Gizmos.DrawWireSphere(mov.rb.position, mov.swv.swipeRange);
		}
	}
	void StartSwipe(InputAction.CallbackContext context)
	{
		mov.swv.desiredSwipe = true;
	}

	void EndSwipe(InputAction.CallbackContext context)
	{

	}

	public override void UpdateTimers()
	{
		mov.swv.swipeDurationTimer = mov.swv.swipeDurationTimer.TimerCountdown();
	}

	void HandleSwipe()
	{
		if (mov.swv.swipeDurationTimer <= 0)
		{
			if (mov.swv.swiping)
			{
				mov.swv.swiping = false;
				mov.swv.swipingOnGround = false;
			}
		}

		if (mov.swv.desiredSwipe)
		{
			mov.swv.desiredSwipe = false;
			DoSwipe();
		}

		if (mov.swv.swiping)
		{
			if (mov.gcv.grounded)
				mov.swv.swipingOnGround = true;
		}
	}

	void DoSwipe()
	{
		//destroy destructibles around the player
		if (mov.suv.surfing)
		{
			if (mov.av.btv.shoveItCooldownTimer <= 0 && mov.av.btv.activeShoveItTokens > 0)
				ShoveIt();
			
			return;
		}

		if (mov.dv.dashing && !mov.av.div.dived)
		{
			//Dive
			Dive();
			return;
		}

		if (mov.swv.swiping || mov.swv.swipeDurationTimer > 0)
			return;

		if (mov.jc.inAir && !mov.av.sdj.jumped)
		{
			SwipeDoubleJump();
			PlayerVFX.instance.doubleJump.SendEvent("OnPlay");
			mov.swv.doubleJumpAnimation = true;
		}
		//invoke unityEvent OnSwipe
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

		mov.swv.swiping = true;
		if(mov.swv.doubleJumpAnimation == false)
		{
			PlayerVFX.instance.swipe.SendEvent("OnPlay");
			mov.swv.swipeAnimation = true;
		}
		mov.swv.swipeDurationTimer = mov.swv.swipeDuration;
	}

	void HandleSwipeDoubleJump()
	{
		if (!mov.jc.inAir)
			mov.av.sdj.jumped = false;
	}

	void SwipeDoubleJump()
	{
		mov.velocity = new Vector3(mov.velocity.x, 0, mov.velocity.z);
		mov.velocity += Vector3.up * mov.av.sdj.doubleJumpHeight;
		mov.av.sdj.jumped = true;
		mov.av.tj.turnOffTwirlJump = true;
		mov.av.div.diving = false;
	}

	void ShoveIt()
	{
		//360 trick
		if (mov.velocity.y < mov.av.btv.shoveItHeight)
			mov.velocity = new Vector3(mov.velocity.x, 0, mov.velocity.z);

		mov.velocity += new Vector3(0, mov.av.btv.shoveItHeight, 0);

		mov.av.btv.shoveItCooldownTimer = mov.av.btv.shoveItCooldown;
		mov.av.btv.shoveItAnimation = true;
		mov.av.btv.activeShoveItTokens--;
		mov.av.tsv.twirlSurfing = false;

        ComboMeter.AddToCombo("Shoveit");
    }

    void Dive()
	{
		if (mov.av.div.divingResetsVelocity)
		{
			mov.rb.linearVelocity = Vector3.zero;
			mov.velocity = Vector3.zero;
		}

		mov.velocity += new Vector3(0, mov.av.div.upwardSpeed, 0);

		mov.av.div.divingDirection = new Vector3(mov.lastInputDirection3D.x, 0, mov.lastInputDirection3D.z).normalized;

		mov.av.div.diving = true;
		mov.av.div.dived = true;
		mov.av.div.diveLengthTimer = mov.av.div.diveLength;
		mov.dv.dashing = false;
		mov.swv.swiping = false;

        ComboMeter.AddToCombo("Dive");

        mov.av.div.onDive.Invoke();
	}

	public override void ResetValues()
	{
		mov.swv.swiping = false;
		mov.swv.swipingOnGround = false;
	}

	public override void UpdateAnimator()
	{
		if (mov.swv.swipeAnimation && mov.swv.swiping)
		{
			mov.animator.SetTrigger("Swipe"); 
			mov.swv.swipeAnimation = false;
		}
		if (mov.swv.doubleJumpAnimation && mov.swv.swiping)
		{
			mov.animator.SetTrigger("DoubleJump"); 
			mov.swv.doubleJumpAnimation = false;
		}
	}
}
