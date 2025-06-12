using System;
using FMODUnity;
using UnityEngine;
using UnityEngine.InputSystem;

public class GroundMovement : Ability
{
	public GroundMovement(MovementController _mov) : base(_mov) { }

	public override void RunOnUpdateBeforeSetVelocity()
	{
		CheckGrounded();
		UpdateGroundedValues();
	}

	public override void RunOnUpdateDuringSetVelocity()
	{
		AddSlope();
	}

	public override void RunOnValidate()
	{
		mov.gcv.minGroundDotProduct = Mathf.Cos(mov.gcv.maxGroundAngle * Mathf.Deg2Rad);
		mov.gcv.minSlopeDotProduct = Mathf.Cos(mov.gcv.minSlopeAngle * Mathf.Deg2Rad);
	}

	void UpdateGroundedValues()
	{
		if (mov.gcv.grounded)
		{
			mov.acv.antiAirTimer = 0;
			mov.wjv.wallJumped = false;
			mov.wjv.wallRiding = false;
			mov.jc.inAir = false;

			if (!mov.jc.jumping)
			{
				mov.jc.jumpPhase = 0;
				mov.jc.coyoteTimeAvailable = true;
			}
		}

		else
		{
			mov.gcv.contactNormal = Vector3.zero;

			if (!mov.jc.inAir)
			{
				mov.jc.jumpPhase = 1;
				mov.jc.inAir = true;
			}
		}
	}

	public override void RunOnDrawGizmos()
	{
		if (mov.gcv.gizmosOn)
		{
			Gizmos.color = Color.blue;

			Gizmos.DrawLine(mov.rb.position, mov.rb.position + mov.velocity);

			Gizmos.color = Color.red;
			Vector3 gradient;

			gradient = mov.ProjectOnContactPlane(Vector3.down);
			Gizmos.DrawLine(mov.rb.position, mov.rb.position + gradient.normalized * 3);
			Gizmos.DrawLine(mov.rb.position, mov.rb.position + Vector3.down * mov.gcv.groundSnapProbeDistance);


			if (InputDistributor.playerInputActions != null)
			{
				Vector2 playerInput = InputDistributor.playerInputActions.Movement.DirectionalInput.ReadValue<Vector2>();
				playerInput = Vector2.ClampMagnitude(playerInput, 1f);

				if (playerInput != Vector2.zero)
				{
					Vector3 cameraDirection = Camera.main.transform.forward;
					Vector3 cameraRightDirection = Camera.main.transform.right;
					cameraDirection = new Vector3(cameraDirection.x, 0, cameraDirection.z).normalized;
					cameraRightDirection = new Vector3(cameraRightDirection.x, 0, cameraRightDirection.z).normalized;
					Vector3 newMovementVector = mov.ProjectOnContactPlane(cameraDirection) * playerInput.y;
					newMovementVector += mov.ProjectOnContactPlane(cameraRightDirection) * playerInput.x;

					if (mov.gcv.SlowWalkingOn)
						newMovementVector = newMovementVector.normalized * playerInput.magnitude;
					mov.desiredVelocity = newMovementVector * mov.gcv.maxSpeed;

					Gizmos.DrawLine(mov.rb.position, mov.rb.position + mov.desiredVelocity.normalized * 3);
					mov.lastPlayerInput = playerInput;
				}
				else if (mov.lastPlayerInput != null)
				{
					Vector3 cameraDirection = Camera.main.transform.forward;
					Vector3 cameraRightDirection = Camera.main.transform.right;
					cameraDirection = new Vector3(cameraDirection.x, 0, cameraDirection.z).normalized;
					cameraRightDirection = new Vector3(cameraRightDirection.x, 0, cameraRightDirection.z).normalized;
					Vector3 newMovementVector = mov.ProjectOnContactPlane(cameraDirection) * mov.lastPlayerInput.y;
					newMovementVector += mov.ProjectOnContactPlane(cameraRightDirection) * mov.lastPlayerInput.x;

					newMovementVector = newMovementVector.normalized * mov.lastPlayerInput.magnitude;
					mov.desiredVelocity = newMovementVector * mov.gcv.maxSpeed;

					Gizmos.DrawLine(mov.rb.position, mov.rb.position + mov.desiredVelocity.normalized * 3);
				}
			}
		}
	}

	void CheckGrounded()
	{
		if (mov.gcv.groundContactCount > 0)
			mov.gcv.grounded = true;
		else
			mov.gcv.grounded = false;
	}

	public override void UpdateAnimator()
	{
		Vector2 playerInput = InputDistributor.playerInputActions.Movement.DirectionalInput.ReadValue<Vector2>();

		if (playerInput != Vector2.zero && (mov.gcv.grounded || Physics.Raycast(mov.rb.position, Vector3.down, mov.gcv.groundSnapProbeDistance, mov.gcv.walkableLayers)) && !mov.animator.GetBool("Surfing"))
		{
			if (!mov.animator.GetBool("Running"))
			{
				PlayerVFX.instance.runningDust.SendEvent("OnPlay");
				if (!mov.gcv.runningLoopInstance.isValid())
				{ mov.gcv.runningLoopInstance = RuntimeManager.CreateInstance(mov.gcv.runningLoop); }
				mov.gcv.runningLoopInstance.start();
			}
			mov.animator.SetBool("Running", true);

		}
		else
		{
			mov.animator.SetBool("Running", false);
			mov.gcv.runningLoopInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
			PlayerVFX.instance.runningDust.SendEvent("OnStop");
		}

		mov.animator.SetBool("Grounded", mov.gcv.grounded);
		mov.animator.SetBool("OnSlope", mov.gcv.onSlope);

	}

	void AddSlope()
	{
		if (!mov.gcv.onSlope)
			return;
		Vector3 gradient;

		if (mov.velocity.y > 0)
		{
			mov.velocity = Vector3.MoveTowards(mov.velocity, new Vector3(mov.velocity.x, 0, mov.velocity.z), Time.deltaTime * 50);
		}

		gradient = mov.ProjectOnContactPlane(Vector3.down);
		mov.rb.AddForce(gradient.normalized * mov.gcv.slopeGlideStrength);
	}
}
