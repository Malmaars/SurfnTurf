using FMODUnity;
using SurfnTurf;
using System;
using System.Collections.Generic;
using UnityEngine;

public class LedgeGrab : Ability
{
	public LedgeGrab(MovementController _mov) : base(_mov) { }

	public override void RunOnValidate()
	{

	}
	public override void RunOnUpdateDuringSetVelocity()
	{
		if (!mov.lgv.active)
			return;
		CheckForWalls();
		CheckForLedge();
		HandleLedgeGrab();
	}

	void CheckForLedge()
	{
		if (mov.lgv.ledgeGrabbing)
			return;

		//I want to check from a specific height if there's a ledge, if there is, grab onto it

		//the raycast will be cast downward from the desired distance and height

		Vector3 startPos = mov.rb.transform.position + new Vector3(mov.lastInputDirection3D.x * mov.lgv.maxDistanceForward, mov.lgv.heightToCast, mov.lastInputDirection3D.z * mov.lgv.maxDistanceForward);

		RaycastHit hit;

		Physics.Raycast(startPos, Vector3.down, out hit, mov.lgv.raycastDistance + mov.lgv.heightLeeway, mov.lgv.ledgeGrabbable);

		mov.lgv.currentGroundHit = hit;
		//you hit a backface;
		if (Vector3.Dot(Vector3.down, hit.normal) > 0)
			return;
		


		if (mov.rb.linearVelocity.y <= 0
			&& Physics.Raycast(startPos, Vector3.down, out hit, mov.lgv.raycastDistance + mov.lgv.heightLeeway, mov.lgv.ledgeGrabbable)
			&& mov.lgv.currentWallNormal != null && mov.lgv.currentWallNormal != Vector3.zero)
		{
			if (hit.collider.isTrigger)
				return;
			if ((Vector3.Distance(hit.point, startPos) > mov.lgv.raycastDistance + mov.lgv.heightLeeway || Vector3.Distance(hit.point, startPos) < mov.lgv.raycastDistance - mov.lgv.heightLeeway))
				return;
			if (hit.normal.y != 1)
				return;
			//check if there's something straight ahead
			if (Physics.Raycast(mov.rb.transform.position + Vector3.up * mov.lgv.heightToCast, mov.lastInputDirection3D, mov.lgv.maxDistanceForward))
				return;

			DoLedgeGrab();
		}
	}

	void HandleLedgeGrab()
	{
		AnimatorClipInfo[] m_CurrentClipInfo = mov.animator.GetCurrentAnimatorClipInfo(0);

		if (m_CurrentClipInfo[0].clip.name == "RM_Munch|LedgeGrab")
			mov.lgv.ledgeGrabDurationTimer = mov.lgv.ledgeGrabDurationTimer.TimerCountdown();
		else if (mov.lgv.ledgeGrabDurationTimer < mov.lgv.ledgeGrabDuration)
			mov.lgv.ledgeGrabDurationTimer = 0;
		if(mov.lgv.endLedgeGrabAnimation)
		{
			mov.lgv.endLedgeGrabAnimation = false;
			mov.rb.isKinematic = false;
		}

		if (mov.lgv.ledgeGrabbing && mov.lgv.endLedgeGrab)
		{
			//ledgegrab finished, teleport player;
			EndLedgeGrab();
		}
	}

	public void EndLedgeGrab()
	{
		mov.rb.isKinematic = true;
		mov.rb.transform.position = mov.rb.transform.position + mov.playerVisual.forward * mov.lgv.teleportoffset.x + Vector3.up * mov.lgv.teleportoffset.y;
		mov.velocity = Vector3.zero;
		mov.lgv.ledgeGrabbing = false;
		mov.lgv.turnGravityOff = false;
		mov.lgv.ledgeGrabDurationTimer = 0;
		mov.lgv.startedAnimation = false;
		mov.lgv.ledgeGrabAnimation = false;
		mov.lgv.endLedgeGrab = false;
		mov.lgv.endLedgeGrabAnimation = true;

		mov.ResetValues();
		mov.animator.SetTrigger("EndLedgeGrab");
	}

	void DoLedgeGrab()
	{

		if(mov.lgv.currentWallNormal == Vector3.zero)
		{
			Debug.LogError("Tried to do a ledgegrab, but couldn't find a wall");
			return;
		}
		//force the direction of the player towards the ledge, to make sure the animation plays properly
		mov.lgv.turnGravityOff = true;
		mov.playerVisual.forward = -new Vector3(mov.lgv.currentWallNormal.x, 0, mov.lgv.currentWallNormal.z);

		//also put the player at the exact position that's nice for the ledgegrab
		mov.rb.position = new Vector3((mov.lgv.currentWallHit.point + mov.lgv.currentWallNormal * 0.5f).x, (mov.lgv.currentGroundHit.point.y - mov.lgv.teleportoffset.y) + 0.5f, (mov.lgv.currentWallHit.point + mov.lgv.currentWallNormal * 0.5f).z);

		mov.lgv.ledgeGrabDurationTimer = mov.lgv.ledgeGrabDuration;
		mov.lgv.ledgeGrabbing = true;
		mov.lgv.ledgeGrabAnimation = true;
		RuntimeManager.PlayOneShot(mov.lgv.ledgeGrabSound);
		mov.av.tj.turnOffTwirlJump = true;
	}

	void CheckForWalls()
	{
		//send out a couple raycasts in multiple directions
		float angleStep = 360f;

		List<Vector3> wallAngles = new List<Vector3>();

		bool nowalls = true;
		for (float i = 0; i < mov.wjv.wallRaycastAmount; i++)
		{
			// Calculate the angle for the current raycast
			float angle = 90 + i * (angleStep / mov.wjv.wallRaycastAmount);

			// Convert the angle to radians, then create a direction vector using cosine and sine for the x and z axes
			Vector3 direction = new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle), 0, Mathf.Sin(Mathf.Deg2Rad * angle));
			RaycastHit hit;

			Physics.Raycast(mov.rb.position, direction, out hit, mov.lgv.maxDistanceForward * 10, mov.gcv.walkableLayers);

			if (hit.collider != null)
			{
				if (Vector3.Distance(hit.point, mov.rb.position) < Vector3.Distance(mov.lgv.currentWallHit.point, mov.rb.position))
					mov.lgv.currentWallHit = hit;
				nowalls = false;
			}
		}

		if (nowalls)
			mov.lgv.currentWallNormal = Vector3.zero;
		else
			mov.lgv.currentWallNormal = mov.lgv.currentWallHit.normal;

	}

	public override void UpdateAnimator()
	{
		if (mov.lgv.ledgeGrabAnimation)
		{
			mov.lgv.ledgeGrabAnimation = false;
			mov.animator.SetTrigger("LedgeGrab");
		}
	}

	public override void RunOnDrawGizmos()
	{
		if (!mov.lgv.gizmosOn)
			return;
		Vector3 startPos = mov.rb.position + new Vector3(mov.lastInputDirection3D.x * mov.lgv.maxDistanceForward, mov.lgv.heightToCast, mov.lastInputDirection3D.z * mov.lgv.maxDistanceForward);

		Gizmos.color = Color.red;
		Gizmos.DrawLine(startPos, startPos + Vector3.down * mov.lgv.raycastDistance);

		Gizmos.color = Color.blue;
		Gizmos.DrawLine(startPos + Vector3.down * mov.lgv.raycastDistance, (startPos + Vector3.down * mov.lgv.raycastDistance) + Vector3.up *mov.lgv.heightLeeway);
		Gizmos.DrawLine(startPos + Vector3.down * mov.lgv.raycastDistance, (startPos + Vector3.down * mov.lgv.raycastDistance) + Vector3.down *mov.lgv.heightLeeway);
	}
}
