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
		if (mov.wjv.active)
			CheckForWalls();
		HandleWallGrab();
	}

	public override void RunOnDrawGizmos()
	{
		if (!mov.wjv.gizmosOn)
			return;
		float angleStep = 360f;
		for (float i = 0; i < mov.wjv.wallRaycastAmount; i++)
		{
			// Calculate the angle for the current raycast
			float angle = 90 + i * (angleStep / mov.wjv.wallRaycastAmount);

			// Convert the angle to radians, then create a direction vector using cosine and sine for the x and z axes
			Vector3 direction = new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle), 0, Mathf.Sin(Mathf.Deg2Rad * angle));
			RaycastHit hit;

			Physics.Raycast(mov.rb.position, direction, out hit, mov.wjv.distanceUntilWallGrab);
			if (hit.collider != null && hit.normal.y >= 0f - mov.wjv.maxWallAngleOffsetZeroToOne && hit.normal.y <= 0f + mov.wjv.maxWallAngleOffsetZeroToOne)
			{
				//we're up against a wall

				if (Vector3.Dot(new Vector3(mov.velocity.x, 0, mov.velocity.z).normalized, direction) >= 1 - mov.wjv.inputDirectionLeeway)
				{
					Gizmos.color = Color.green;
					Gizmos.DrawLine(mov.rb.position, mov.rb.position + direction * mov.wjv.distanceUntilWallGrab);
				}

				else if (Vector3.Dot(new Vector3(mov.velocity.x, 0, mov.velocity.z).normalized, hit.point - mov.rb.position) > 0)
				{
					Gizmos.color = Color.blue;
					Gizmos.DrawLine(mov.rb.position, mov.rb.position + direction * mov.wjv.distanceUntilWallGrab);
				}
			}
			else
			{
				Gizmos.color = Color.red;
				Gizmos.DrawLine(mov.rb.position, mov.rb.position + direction * mov.wjv.distanceUntilWallGrab);
			}
		}

		Gizmos.color = Color.red;
		Vector3 startPos = mov.rb.position + new Vector3(mov.lastInputDirection3D.x * mov.wjv.distanceUntilWallGrab, mov.wjv.maxHeight, mov.lastInputDirection3D.z * mov.wjv.distanceUntilWallGrab);
		Gizmos.DrawLine(startPos, startPos + Vector3.down * mov.wjv.maxHeightRayLength);
	}

	void HandleWallGrab()
	{
		if(mov.wjv.wallgrab)
		{
			mov.av.tj.turnOffTwirlJump = true;
		}
	}

	public override void UpdateTimers()
	{
		mov.wjv.wallJumpCoyoteTimer = mov.wjv.wallJumpCoyoteTimer.TimerCountdown();
	}

	void CheckForWalls()
	{
		Vector2 playerInput = InputDistributor.playerInputActions.Movement.DirectionalInput.ReadValue<Vector2>();

		if (mov.suv.surfing)
			return;
		

		bool wallgrabbed = (mov.wjv.wallgrab || mov.wjv.wallRiding);

		if (mov.wjv.wallJumpLimitVelocity && mov.gcv.grounded || (mov.acv.antiAirTimer <= 0 && playerInput != Vector2.zero))
			mov.wjv.wallJumpLimitVelocity = false;

		if (mov.wjv.wallJumpCooldownTimer > 0 || mov.acv.antiAirTimer > 0 || Physics.Raycast(mov.rb.position, Vector3.down, mov.wjv.minimumDistanceFromGround))
		{
			mov.wjv.wallgrab = false;
			mov.wjv.wallRiding = false;

			mov.wjv.wallJumpCooldownTimer = mov.wjv.wallJumpCooldownTimer.TimerCountdown();
			return;
		}

		bool noWalls = true;


		//send out a couple raycasts in multiple directions
		float angleStep = 360f;

		List<Vector3> wallAngles = new List<Vector3>();

		mov.wjv.wallgrab = false;

		for (float i = 0; i < mov.wjv.wallRaycastAmount; i++)
		{
			// Calculate the angle for the current raycast
			float angle = 90 + i * (angleStep / mov.wjv.wallRaycastAmount);

			// Convert the angle to radians, then create a direction vector using cosine and sine for the x and z axes
			Vector3 direction = new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle), 0, Mathf.Sin(Mathf.Deg2Rad * angle));
			RaycastHit hit;

			Physics.Raycast(mov.rb.position, direction, out hit, mov.wjv.distanceUntilWallGrab);

			if (hit.collider != null && hit.normal.y >= 0f - mov.wjv.maxWallAngleOffsetZeroToOne && hit.normal.y <= 0f + mov.wjv.maxWallAngleOffsetZeroToOne)
			{
				if (playerInput != Vector2.zero && Vector3.Dot(new Vector3(mov.lastInputDirection3D.x, 0, mov.lastInputDirection3D.z).normalized, hit.point - mov.rb.position) >= 0)
					mov.acv.antiAirTimer = 0;
				//we're up against a wall

				if (playerInput != Vector2.zero && Vector3.Dot(new Vector3(mov.lastInputDirection3D.x, 0, mov.lastInputDirection3D.z).normalized, -hit.normal) >= 1 - mov.wjv.inputDirectionLeeway)
				{
					if (mov.velocity.y <= 0f)
					{
						mov.velocity = new Vector3(0, mov.velocity.y, 0);
						//player is aiming at the wall
						mov.wjv.wallgrab = true;
						mov.wjv.jumpDirection = (hit.normal + Vector3.up) / 2;
						mov.wjv.currentWallNormal = hit.normal;
						noWalls = false;
						wallAngles.Clear();
						break;
					}
				}

				else if (Vector3.Dot(new Vector3(mov.velocity.x, 0, mov.velocity.z).normalized, hit.point - mov.rb.position) >= 0)
				{
					Vector3 newAngle;
					//we are touching a wall just not hugging it
					newAngle = hit.normal;

					if (!wallAngles.Contains(newAngle))
						wallAngles.Add(newAngle);

					noWalls = false;
					mov.wjv.wallRiding = true;
					mov.wjv.wallgrab = false;
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

			if (mov.velocity != Vector3.zero && Vector3.Dot(new Vector3(mov.velocity.x, 0, mov.velocity.z).normalized, newDirection.normalized) > mov.wjv.wallRidingMinimumOffset)
			{
				newDirection = ((newDirection.normalized * 1.2f + Vector3.up + new Vector3(mov.velocity.x, 0, mov.velocity.z).normalized * 2f) / 3);
			}
			else
				newDirection = (newDirection.normalized + Vector3.up) / 2;


			if (newDirection != Vector3.up)
				mov.wjv.jumpDirection = newDirection;
			else
				noWalls = true;
		}

		Vector3 startPos = mov.rb.position + new Vector3(mov.lastInputDirection3D.x * mov.wjv.distanceUntilWallGrab, mov.wjv.maxHeight, mov.lastInputDirection3D.z * mov.wjv.distanceUntilWallGrab);

		if (Physics.Raycast(startPos, Vector3.down, mov.wjv.maxHeightRayLength, mov.lgv.ledgeGrabbable))
			noWalls = true;

		if (noWalls)
		{
			mov.wjv.wallgrab = false;
			mov.wjv.wallRiding = false;
		}

		if (wallgrabbed && (!mov.wjv.wallgrab && !mov.wjv.wallRiding))
		{
			//start the coyote timer
			mov.wjv.wallJumpCoyoteTimer = mov.wjv.wallJumpCoyoteTime;
			wallgrabbed = false;
		}

	}

	public override void ResetValues()
	{
		mov.wjv.wallRiding = false;
		mov.wjv.wallgrab = false;
	}

	public override void UpdateAnimator()
	{
		if (mov.wjv.wallgrab && !mov.wjv.wallgrabAnimation)
		{
			mov.animator.SetBool("Sliding", true);
			mov.wjv.wallgrabAnimation = true;
		}
		else if (!mov.wjv.wallgrab)
		{
			mov.animator.SetBool("Sliding", false);
			mov.wjv.wallgrabAnimation = false;
		}

		if (mov.wjv.wallJumpAnimation)
		{
			mov.wjv.wallJumpAnimation = false;
			mov.animator.SetTrigger("WallJump");
		}
	}
}
