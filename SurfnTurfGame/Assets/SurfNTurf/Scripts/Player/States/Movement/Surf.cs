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
			if (mov.SWV.swiping || mov.AV.spd.spinDashing)
			{
				TwirlSurf();
			}

			if (!mov.GCV.grounded && mov.RB.linearVelocity.y < 0 && mov.RB.linearVelocity.magnitude > mov.AV.sp.minimumVelocityToParry)
				mov.AV.sp.parryIsReady = true;
			else
				mov.AV.sp.parryIsReady = false;

			if (mov.DV.dashing && (mov.GCV.grounded || Physics.Raycast(mov.RB.position, Vector3.down, mov.AV.sp.distanceFromGroundForDashParry)))
				mov.AV.sp.dashParryIsReady = true;
			if(mov.AV.sp.jumpParryCoyoteTimer > 0)
				mov.AV.sp.jumpParryIsReady = true;

			DoSurf();
			mov.SUV.desiredSurf = false;
		}

		else
		{
			if (mov.SUV.surfing && mov.GCV.grounded)
			{
				if (mov.GCV.contactNormal.y < mov.SUV.groundNormalThreshold)
				{
					mov.Velocity += mov.ProjectOnContactPlane(Vector3.down).normalized * mov.SUV.accelarationSpeed * Time.deltaTime * (1 - mov.GCV.contactNormal.y);
				}
				else
				{
					//slow down
					mov.Velocity -= mov.Velocity.normalized * mov.SUV.decelerationSpeed * Time.deltaTime * mov.GCV.contactNormal.y;
				}

				float velocityMag = mov.Velocity.magnitude;

				//slightly change the angle of the surf
				mov.Velocity = (mov.Velocity.normalized + (mov.LastInputDirection3D * mov.SUV.steeringStrength * Time.deltaTime)).normalized * velocityMag;
			}
		}
	}

	void HandleGroundParry()
	{
		if (mov.AV.sp.parryIsReady && mov.GCV.grounded && mov.SUV.startSurfBufferTimer > 0)
		{
			//perform a ground parry
			ParryGround();
			mov.SUV.startSurfBufferTimer = 0;
		}

		if(mov.AV.sp.dashParryIsReady)
			DashParry();

		if (mov.AV.sp.jumpParryCoyoteTimer > 0)
			mov.AV.sp.jumpParryCoyoteTimer -= Time.deltaTime;

		if (mov.AV.sp.jumpParryIsReady)
		{
			ParryGround();
			mov.AV.sp.jumpParryIsReady = false;
		}

	}

	void ParryGround()
	{

		mov.Velocity = new Vector3(mov.Velocity.x,0,mov.Velocity.z);

		if (mov.AV.sp.goInNormalDirection)
			mov.Velocity += mov.GCV.contactNormal * mov.AV.sp.surfParryJumpHeight;
		else
		{
			mov.Velocity += Vector3.up * mov.AV.sp.surfParryJumpHeight;
		}

		mov.AV.sp.parryAnimation = true;

		PlayerVFX.instance.parrySpark.SendEvent("OnPlay");
		mov.AV.sp.OnParry.Invoke();
	}

	void DashParry()
	{
		mov.DV.dashing = false;
		Vector3 newVelocityDirection = new Vector3(mov.Velocity.x, 0, mov.Velocity.z).normalized * mov.AV.sp.dashParryForwardSpeed;

		mov.Velocity = new Vector3(newVelocityDirection.x,mov.AV.sp.dashParryHeight,newVelocityDirection.z);
		mov.AV.sp.parryAnimation = true;

		PlayerVFX.instance.parrySpark.SendEvent("OnPlay");
		mov.AV.sp.dashParryIsReady = false;
	}	

	void DoSurf()
	{
		mov.SUV.surfing = true;
	}

	void TwirlSurf()
	{
		mov.AV.spd.turnOffSpinDash = true;

		if (mov.Velocity.magnitude < mov.AV.tsv.maximumVelocityMagnitudeForStartBoost)
			mov.Velocity += new Vector3(mov.Velocity.x, 0, mov.Velocity.z).normalized * mov.AV.tsv.startBoost;
		mov.AV.tsv.twirlSurfing = true;
	}

	public override void UpdateAnimator()
	{
		if (mov.SUV.surfing && !mov.PlayerAnimator.GetBool("Surfing") && !mov.AV.sp.parryAnimation)
		{
			mov.PlayerAnimator.SetTrigger("Surf");
		}
		mov.PlayerAnimator.SetBool("Surfing", mov.SUV.surfing);
		SurfBoardManager.instance.ToggleSurfboard(mov.SUV.surfing);
	}
}
