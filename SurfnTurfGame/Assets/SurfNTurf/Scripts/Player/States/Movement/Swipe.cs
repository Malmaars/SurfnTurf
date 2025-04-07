using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Swipe : Ability
{
	public Swipe(IMovement _mov) : base(_mov) { }

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
		if (mov.SWV.gizmosOn)
		{
			Gizmos.color = Color.red;

			Gizmos.DrawWireSphere(mov.RB.position, mov.SWV.swipeRange);
		}
	}
	void StartSwipe(InputAction.CallbackContext context)
	{
		mov.SWV.desiredSwipe = true;
	}

	void EndSwipe(InputAction.CallbackContext context)
	{
		if (mov.AV.tj.holdToGlide)
			mov.AV.tj.twirlJumping = false;
	}

	void HandleSwipe()
	{
		if (mov.SWV.swipeDurationTimer > 0)
			mov.SWV.swipeDurationTimer -= Time.deltaTime;

		if (mov.SWV.swipeDurationTimer <= 0)
		{
			if (mov.SWV.swiping)
			{
				mov.SWV.swiping = false;
				mov.SWV.swipingOnGround = false;
			}
		}

		if (mov.SWV.desiredSwipe)
		{
			mov.SWV.desiredSwipe = false;
			DoSwipe();
		}

		if (mov.SWV.swiping)
		{
			if (mov.GCV.grounded)
				mov.SWV.swipingOnGround = true;
		}
	}

	void DoSwipe()
	{
		//destroy destructibles around the player

		if (mov.SWV.swiping || mov.SWV.swipeDurationTimer > 0)
			return;

		if (mov.JC.inAir && !mov.AV.sdj.jumped)
		{
			SwipeDoubleJump();
		}
		//invoke unityEvent OnSwipe
		mov.SWV.onSwipe.Invoke();
		Collider[] collidersClose = Physics.OverlapSphere(mov.RB.position, mov.SWV.swipeRange);

		foreach (Collider collider in collidersClose)
		{
			if (collider.GetComponent<Destructible>() == null)
				continue;
			else
			{
				collider.GetComponent<Destructible>().Destruct(mov.RB.transform);
			}
		}

		mov.SWV.swiping = true;
		mov.SWV.swipeAnimation = true;
		mov.SWV.swipeDurationTimer = mov.SWV.swipeDuration;
	}

	void HandleSwipeDoubleJump()
	{
		if (!mov.JC.inAir)
			mov.AV.sdj.jumped = false;
	}

	void SwipeDoubleJump()
	{
		mov.Velocity = new Vector3(mov.Velocity.x, 0, mov.Velocity.z);
		mov.Velocity += Vector3.up * mov.AV.sdj.doubleJumpHeight;
		mov.AV.sdj.jumped = true;
		mov.AV.tj.twirlJumping = false;
	}
}
