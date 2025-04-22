using SurfnTurf;
using System;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class LedgeGrab : Ability
{
	public LedgeGrab(MovementController _mov) : base(_mov) { }
	
	public override void RunOnUpdateDuringSetVelocity()
	{
		if (!mov.lgv.active)
			return;
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

		if (mov.rb.linearVelocity.y <= 0
			&& Physics.Raycast(startPos, Vector3.down, out hit, mov.lgv.raycastDistance + mov.lgv.heightLeeway, mov.lgv.ledgeGrabbable)
			&& mov.wjv.currentWallNormal != null && mov.wjv.currentWallNormal != Vector3.zero)
		{
			if ((Vector3.Distance(hit.point, startPos) > mov.lgv.raycastDistance + mov.lgv.heightLeeway || Vector3.Distance(hit.point, startPos) < mov.lgv.raycastDistance - mov.lgv.heightLeeway)
				&& hit.normal.y > mov.gcv.minSlopeDotProduct)
				return;

			//perform a ledgegrab
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

		if (mov.lgv.ledgeGrabbing && mov.lgv.endLedgeGrab)//|| (mov.lgv.startedAnimation = true && m_CurrentClipInfo[0].clip.name != "RM_Munch|LedgeGrab")))
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
		//force the direction of the player towards the ledge, to make sure the animation plays properly
		mov.lgv.turnGravityOff = true;
		mov.playerVisual.forward = -new Vector3(mov.wjv.currentWallNormal.x, 0, mov.wjv.currentWallNormal.z);
		mov.lgv.ledgeGrabDurationTimer = mov.lgv.ledgeGrabDuration;
		mov.lgv.ledgeGrabbing = true;
		mov.lgv.ledgeGrabAnimation = true;
		mov.av.tj.turnOffTwirlJump = true;
		Debug.Log("DO Ledge Grab");
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
