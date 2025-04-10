using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Surf : Ability
{
	public Surf(IMovement _mov) : base(_mov) { }

	public override void RunOnEnterState()
	{
		InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Movement.Surf, StartSurf);
		InputDistributor.inputManager.AddActionToInputCancelled(InputDistributor.playerInputActions.Movement.Surf, EndSurf);
	}

	public override void RunOnExitState()
	{
		InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Movement.Surf, StartSurf);
		InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Movement.Surf, EndSurf);
	}

	public override void RunOnUpdateDuringSetVelocity()
	{
		HandleSurfing();
		HandleGroundParry();
	}
	void StartSurf(InputAction.CallbackContext context)
	{
		Debug.Log(mov.SUV.surfCooldownTimer > 0);
		if (mov.SUV.surfCooldownTimer > 0)
			return;

		mov.SUV.desiredSurf = true;
		mov.SUV.startSurfBufferTimer = mov.SUV.startSurfBuffer;
	}

	void EndSurf(InputAction.CallbackContext context)
	{
		if (!mov.SUV.surfing)
			return;

		mov.SUV.desiredSurf = false;
		mov.SUV.surfing = false;
		mov.SUV.surfCooldownTimer = mov.SUV.surfCooldown;
	}

	void HandleSurfing()
	{
		if (mov.SUV.surfCooldownTimer > 0)
			mov.SUV.surfCooldownTimer -= Time.deltaTime;
		if (mov.SUV.startSurfBufferTimer > 0)
			mov.SUV.startSurfBufferTimer -= Time.deltaTime;

		if (mov.SUV.desiredSurf)
		{
			if (!mov.GCV.grounded && mov.RB.linearVelocity.y < 0 && mov.RB.linearVelocity.magnitude > mov.AV.sp.minimumVelocityToParry)
				mov.AV.sp.parryIsReady = true;
			else
				mov.AV.sp.parryIsReady = false;
			DoSurf();
			mov.SUV.desiredSurf = false;
		}

		else
		{
			if (mov.SUV.surfing && mov.GCV.grounded)
			{
				Debug.Log("surfing on the ground");
				if (mov.GCV.contactNormal.y < mov.SUV.groundNormalThreshold)
				{
					mov.Velocity += mov.ProjectOnContactPlane(Vector3.down).normalized * mov.SUV.accelarationSpeed * Time.deltaTime * (1 - mov.GCV.contactNormal.y);
				}
				else
				{
					//slow down
					mov.Velocity -= mov.Velocity.normalized * mov.SUV.decelerationSpeed * Time.deltaTime * mov.GCV.contactNormal.y;
				}
			}
		}
	}

	void HandleGroundParry()
	{
		if (mov.AV.sp.parryIsReady && mov.GCV.grounded && mov.SUV.startSurfBufferTimer > 0)
		{
			//perform a ground parry
			ParryGround();
			mov.AV.sp.OnParry.Invoke();
			mov.SUV.startSurfBufferTimer = 0;
		}
	}

	void ParryGround()
	{
		mov.Velocity = new Vector3(mov.Velocity.x,0,mov.Velocity.z);

		if (mov.AV.sp.goInNormalDirection)
			mov.Velocity += mov.GCV.contactNormal * mov.AV.sp.surfParryJumpHeight;
		else
		{
			Vector3 horizontalVelocity = new Vector3(mov.Velocity.x, 0, mov.Velocity.z).normalized;
			mov.Velocity = new Vector3(horizontalVelocity.x, 1, horizontalVelocity.z) * mov.AV.sp.surfParryJumpHeight;
		}

		PlayerVFX.instance.parrySpark.SendEvent("OnPlay");
	}


	void DoSurf()
	{
		mov.SUV.surfing = true;
	}

	public override void UpdateAnimator()
	{
		if (mov.SUV.surfing && !mov.PlayerAnimator.GetBool("Surfing"))
		{
			mov.PlayerAnimator.SetTrigger("Surf");
		}
		animator.SetBool("Surfing", suv.surfing);
		SurfBoardManager.instance.ToggleSurfboard(suv.surfing);

	}
}
