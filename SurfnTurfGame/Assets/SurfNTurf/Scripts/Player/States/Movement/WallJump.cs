using System.Collections.Generic;
using System;
using Unity.VisualScripting;
using UnityEngine;
using SurfnTurf;

public class WallJump : Ability
{
	public WallJump(MovementController _mov) : base(_mov) { }

	public override void RunOnUpdateBeforeSetVelocity()
	{
		if (mov.WJV.active)
			CheckForWalls();
		HandleWallGrab();
	}

	public override void RunOnDrawGizmos()
	{
		if (!mov.WJV.gizmosOn)
			return;
		float angleStep = 360f;
		for (float i = 0; i < mov.WJV.wallRaycastAmount; i++)
		{
			// Calculate the angle for the current raycast
			float angle = 90 + i * (angleStep / mov.WJV.wallRaycastAmount);

			// Convert the angle to radians, then create a direction vector using cosine and sine for the x and z axes
			Vector3 direction = new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle), 0, Mathf.Sin(Mathf.Deg2Rad * angle));
			RaycastHit hit;

			Physics.Raycast(mov.RB.position, direction, out hit, mov.WJV.distanceUntilWallGrab);
			if (hit.collider != null && hit.normal.y >= 0f - mov.WJV.maxWallAngleOffsetZeroToOne && hit.normal.y <= 0f + mov.WJV.maxWallAngleOffsetZeroToOne)
			{
				//we're up against a wall

				if (Vector3.Dot(new Vector3(mov.Velocity.x, 0, mov.Velocity.z).normalized, direction) >= 1 - mov.WJV.inputDirectionLeeway)
				{
					Gizmos.color = Color.green;
					Gizmos.DrawLine(mov.RB.position, mov.RB.position + direction * mov.WJV.distanceUntilWallGrab);
				}

				else if (Vector3.Dot(new Vector3(mov.Velocity.x, 0, mov.Velocity.z).normalized, hit.point - mov.RB.position) > 0)
				{
					Gizmos.color = Color.blue;
					Gizmos.DrawLine(mov.RB.position, mov.RB.position + direction * mov.WJV.distanceUntilWallGrab);
				}
			}
			else
			{
				Gizmos.color = Color.red;
				Gizmos.DrawLine(mov.RB.position, mov.RB.position + direction * mov.WJV.distanceUntilWallGrab);
			}
		}

		Gizmos.color = Color.red;
		Vector3 startPos = mov.RB.position + new Vector3(mov.LastInputDirection3D.x * mov.WJV.distanceUntilWallGrab, mov.WJV.maxHeight, mov.LastInputDirection3D.z * mov.WJV.distanceUntilWallGrab);
		Gizmos.DrawLine(startPos, startPos + Vector3.down * mov.WJV.maxHeightRayLength);
	}

	void HandleWallGrab()
	{
		if(mov.WJV.wallgrab)
		{
			mov.AV.tj.turnOffTwirlJump = true;
		}
	}

	public override void UpdateTimers()
	{
		mov.WJV.wallJumpCoyoteTimer = mov.WJV.wallJumpCoyoteTimer.TimerCountdown();
	}

	void CheckForWalls()
	{
		Vector2 playerInput = InputDistributor.playerInputActions.Movement.DirectionalInput.ReadValue<Vector2>();

		if (mov.SUV.surfing)
			return;
		

		bool wallgrabbed = (mov.WJV.wallgrab || mov.WJV.wallRiding);

		if (mov.WJV.wallJumpLimitVelocity && mov.GCV.grounded || (mov.ACV.antiAirTimer <= 0 && playerInput != Vector2.zero))
			mov.WJV.wallJumpLimitVelocity = false;

		if (mov.WJV.wallJumpCooldownTimer > 0 || mov.ACV.antiAirTimer > 0 || Physics.Raycast(mov.RB.position, Vector3.down, mov.WJV.minimumDistanceFromGround))
		{
			mov.WJV.wallgrab = false;
			mov.WJV.wallRiding = false;

			mov.WJV.wallJumpCooldownTimer = mov.WJV.wallJumpCooldownTimer.TimerCountdown();
			return;
		}

		bool noWalls = true;


		//send out a couple raycasts in multiple directions
		float angleStep = 360f;

		List<Vector3> wallAngles = new List<Vector3>();

		mov.WJV.wallgrab = false;

		for (float i = 0; i < mov.WJV.wallRaycastAmount; i++)
		{
			// Calculate the angle for the current raycast
			float angle = 90 + i * (angleStep / mov.WJV.wallRaycastAmount);

			// Convert the angle to radians, then create a direction vector using cosine and sine for the x and z axes
			Vector3 direction = new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle), 0, Mathf.Sin(Mathf.Deg2Rad * angle));
			RaycastHit hit;

			Physics.Raycast(mov.RB.position, direction, out hit, mov.WJV.distanceUntilWallGrab);

			if (hit.collider != null && hit.normal.y >= 0f - mov.WJV.maxWallAngleOffsetZeroToOne && hit.normal.y <= 0f + mov.WJV.maxWallAngleOffsetZeroToOne)
			{
				if (playerInput != Vector2.zero && Vector3.Dot(new Vector3(mov.LastInputDirection3D.x, 0, mov.LastInputDirection3D.z).normalized, hit.point - mov.RB.position) >= 0)
					mov.ACV.antiAirTimer = 0;
				//we're up against a wall

				if (playerInput != Vector2.zero && Vector3.Dot(new Vector3(mov.LastInputDirection3D.x, 0, mov.LastInputDirection3D.z).normalized, -hit.normal) >= 1 - mov.WJV.inputDirectionLeeway)
				{
					if (mov.Velocity.y <= 0f)
					{
						mov.Velocity = new Vector3(0, mov.Velocity.y, 0);
						//player is aiming at the wall
						mov.WJV.wallgrab = true;
						mov.WJV.jumpDirection = (hit.normal + Vector3.up) / 2;
						mov.WJV.currentWallNormal = hit.normal;
						noWalls = false;
						wallAngles.Clear();
						break;
					}
				}

				else if (Vector3.Dot(new Vector3(mov.Velocity.x, 0, mov.Velocity.z).normalized, hit.point - mov.RB.position) >= 0)
				{
					Vector3 newAngle;
					//we are touching a wall just not hugging it
					newAngle = hit.normal;

					if (!wallAngles.Contains(newAngle))
						wallAngles.Add(newAngle);

					noWalls = false;
					mov.WJV.wallRiding = true;
					mov.WJV.wallgrab = false;
				}
			}
		}

		if (wallAngles.Count > 0)
		{
			Vector3 newDirection = Vector3.zero;

			foreach (Vector3 v3 in wallAngles)
			{
				newDirection += v3;
			}

			newDirection.Normalize();

			if (mov.Velocity != Vector3.zero && Vector3.Dot(new Vector3(mov.Velocity.x, 0, mov.Velocity.z).normalized, newDirection.normalized) > mov.WJV.wallRidingMinimumOffset)
			{
				newDirection = ((newDirection.normalized * 1.2f + Vector3.up + new Vector3(mov.Velocity.x, 0, mov.Velocity.z).normalized * 2f) / 3);
			}
			else
				newDirection = (newDirection.normalized + Vector3.up) / 2;


			if (newDirection != Vector3.up)
				mov.WJV.jumpDirection = newDirection;
			else
				noWalls = true;
		}

		Vector3 startPos = mov.RB.position + new Vector3(mov.LastInputDirection3D.x * mov.WJV.distanceUntilWallGrab, mov.WJV.maxHeight, mov.LastInputDirection3D.z * mov.WJV.distanceUntilWallGrab);

		if (Physics.Raycast(startPos, Vector3.down, mov.WJV.maxHeightRayLength, mov.LGV.ledgeGrabbable))
			noWalls = true;

		if (noWalls)
		{
			mov.WJV.wallgrab = false;
			mov.WJV.wallRiding = false;
		}

		if (wallgrabbed && (!mov.WJV.wallgrab && !mov.WJV.wallRiding))
		{
			//start the coyote timer
			mov.WJV.wallJumpCoyoteTimer = mov.WJV.wallJumpCoyoteTime;
			wallgrabbed = false;
		}

	}

	public override void ResetValues()
	{
		mov.WJV.wallRiding = false;
		mov.WJV.wallgrab = false;
	}

	public override void UpdateAnimator()
	{
		if (mov.WJV.wallgrab && !mov.WJV.wallgrabAnimation)
		{
			mov.PlayerAnimator.SetBool("Sliding", true);
			mov.WJV.wallgrabAnimation = true;
		}
		else if (!mov.WJV.wallgrab)
		{
			mov.PlayerAnimator.SetBool("Sliding", false);
			mov.WJV.wallgrabAnimation = false;
		}

		if (mov.WJV.wallJumpAnimation)
		{
			mov.WJV.wallJumpAnimation = false;
			mov.PlayerAnimator.SetTrigger("WallJump");
		}
	}
}
