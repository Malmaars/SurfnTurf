using System;
using UnityEngine;

public class GroundMovement : Ability
{
	public GroundMovement(IMovement _mov) : base(_mov) { }

	public override void RunOnUpdateBeforeSetVelocity()
	{
		CheckGrounded();
		UpdateGroundedValues();
	}

	public override void RunOnUpdateDuringSetVelocity()
	{
		AddSlope();
	}

	void UpdateGroundedValues()
	{
		if (mov.GCV.grounded)
		{
			mov.ACV.antiAirTimer = 0;
			mov.WJV.wallJumped = false;
			mov.WJV.wallRiding = false;
			mov.JC.inAir = false;

			if (!mov.JC.jumping)
			{
				mov.JC.jumpPhase = 0;
				mov.JC.coyoteTimeAvailable = true;
			}
		}

		else
		{
			mov.GCV.contactNormal = Vector3.zero;

			if (!mov.JC.inAir)
			{
				mov.JC.jumpPhase = 1;
				mov.JC.inAir = true;
			}
		}
	}

	public override void RunOnDrawGizmos()
	{
		if (mov.GCV.gizmosOn)
		{
			Gizmos.color = Color.blue;

			Gizmos.DrawLine(mov.RB.position, mov.RB.position + mov.Velocity);

			Gizmos.color = Color.red;
			Vector3 gradient;

			gradient = mov.ProjectOnContactPlane(Vector3.down);
			Gizmos.DrawLine(mov.RB.position, mov.RB.position + gradient.normalized * 3);
			Gizmos.DrawLine(mov.RB.position, mov.RB.position + Vector3.down * mov.GCV.groundSnapProbeDistance);


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


					newMovementVector = newMovementVector.normalized * playerInput.magnitude;
					mov.DesiredVelocity = newMovementVector * mov.GCV.maxSpeed;

					Gizmos.DrawLine(mov.RB.position, mov.RB.position + mov.DesiredVelocity.normalized * 3);
					mov.LastPlayerInput = playerInput;
				}
				else if (mov.LastPlayerInput != null)
				{
					Vector3 cameraDirection = Camera.main.transform.forward;
					Vector3 cameraRightDirection = Camera.main.transform.right;
					cameraDirection = new Vector3(cameraDirection.x, 0, cameraDirection.z).normalized;
					cameraRightDirection = new Vector3(cameraRightDirection.x, 0, cameraRightDirection.z).normalized;
					Vector3 newMovementVector = mov.ProjectOnContactPlane(cameraDirection) * mov.LastPlayerInput.y;
					newMovementVector += mov.ProjectOnContactPlane(cameraRightDirection) * mov.LastPlayerInput.x;

					newMovementVector = newMovementVector.normalized * mov.LastPlayerInput.magnitude;
					mov.DesiredVelocity = newMovementVector * mov.GCV.maxSpeed;

					Gizmos.DrawLine(mov.RB.position, mov.RB.position + mov.DesiredVelocity.normalized * 3);
				}
			}
		}
	}

	void CheckGrounded()
	{
		if (mov.GCV.groundContactCount > 0)
			mov.GCV.grounded = true;
		else
			mov.GCV.grounded = false;
	}

	void AddSlope()
	{
		if (!mov.GCV.onSlope)
			return;
		Vector3 gradient;

		if (mov.Velocity.y > 0)
		{
			mov.Velocity = Vector3.MoveTowards(mov.Velocity, new Vector3(mov.Velocity.x, 0, mov.Velocity.z), Time.deltaTime * 50);
		}

		gradient = mov.ProjectOnContactPlane(Vector3.down);
		mov.RB.AddForce(gradient.normalized * mov.GCV.slopeGlideStrength);
	}
}
