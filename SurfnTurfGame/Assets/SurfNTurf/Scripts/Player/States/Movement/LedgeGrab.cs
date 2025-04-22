using SurfnTurf;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class LedgeGrab : Ability
{
	public LedgeGrab(IMovement _mov) : base(_mov) { }
	
	public override void RunOnUpdateDuringSetVelocity()
	{
		if (!mov.LGV.active)
			return;
		CheckForLedge();
		HandleLedgeGrab();
	}

	void CheckForLedge()
	{
		if (mov.LGV.ledgeGrabbing)
			return;

		//I want to check from a specific height if there's a ledge, if there is, grab onto it

		//the raycast will be cast downward from the desired distance and height

		Vector3 startPos = mov.RB.transform.position + new Vector3(mov.LastInputDirection3D.x * mov.LGV.maxDistanceForward, mov.LGV.heightToCast, mov.LastInputDirection3D.z * mov.LGV.maxDistanceForward);

		RaycastHit hit;

		if (mov.RB.linearVelocity.y <= 0
			&& Physics.Raycast(startPos, Vector3.down, out hit, mov.LGV.raycastDistance + mov.LGV.heightLeeway, mov.LGV.ledgeGrabbable)
			&& mov.WJV.currentWallNormal != null && mov.WJV.currentWallNormal != Vector3.zero)
		{
			if (Vector3.Distance(hit.point, startPos) > mov.LGV.raycastDistance + mov.LGV.heightLeeway || Vector3.Distance(hit.point, startPos) < mov.LGV.raycastDistance - mov.LGV.heightLeeway)
				return;

			//perform a ledgegrab
			DoLedgeGrab();
		}
	}

	void HandleLedgeGrab()
	{
		AnimatorClipInfo[] m_CurrentClipInfo = mov.PlayerAnimator.GetCurrentAnimatorClipInfo(0);

		if (m_CurrentClipInfo[0].clip.name == "RM_Munch|LedgeGrab")
			mov.LGV.ledgeGrabDurationTimer = mov.LGV.ledgeGrabDurationTimer.TimerCountdown();
		else if (mov.LGV.ledgeGrabDurationTimer < mov.LGV.ledgeGrabDuration)
			mov.LGV.ledgeGrabDurationTimer = 0;
		if(mov.LGV.endLedgeGrabAnimation)
		{
			mov.LGV.endLedgeGrabAnimation = false;
			mov.RB.isKinematic = false;
		}

		if (mov.LGV.ledgeGrabbing && mov.LGV.endLedgeGrab)//|| (mov.LGV.startedAnimation = true && m_CurrentClipInfo[0].clip.name != "RM_Munch|LedgeGrab")))
		{
			//ledgegrab finished, teleport player;
			EndLedgeGrab();
		}
	}

	public void EndLedgeGrab()
	{
		mov.RB.isKinematic = true;
		mov.RB.transform.position = mov.RB.transform.position + mov.PlayerVisual.forward * mov.LGV.teleportoffset.x + Vector3.up * mov.LGV.teleportoffset.y;
		mov.Velocity = Vector3.zero;
		mov.LGV.ledgeGrabbing = false;
		mov.LGV.turnGravityOff = false;
		mov.LGV.ledgeGrabDurationTimer = 0;
		mov.LGV.startedAnimation = false;
		mov.LGV.ledgeGrabAnimation = false;
		mov.LGV.endLedgeGrab = false;
		mov.LGV.endLedgeGrabAnimation = true;

		mov.ResetValues();
		mov.PlayerAnimator.SetTrigger("EndLedgeGrab");
	}

	void DoLedgeGrab()
	{
		//force the direction of the player towards the ledge, to make sure the animation plays properly
		mov.LGV.turnGravityOff = true;
		mov.PlayerVisual.forward = -new Vector3(mov.WJV.currentWallNormal.x, 0, mov.WJV.currentWallNormal.z);
		mov.LGV.ledgeGrabDurationTimer = mov.LGV.ledgeGrabDuration;
		mov.LGV.ledgeGrabbing = true;
		mov.LGV.ledgeGrabAnimation = true;
		Debug.Log("DO Ledge Grab");
	}

	public override void UpdateAnimator()
	{
		if (mov.LGV.ledgeGrabAnimation)
		{
			mov.LGV.ledgeGrabAnimation = false;
			mov.PlayerAnimator.SetTrigger("LedgeGrab");
		}
	}

	public override void RunOnDrawGizmos()
	{
		if (!mov.LGV.gizmosOn)
			return;
		Vector3 startPos = mov.RB.position + new Vector3(mov.LastInputDirection3D.x * mov.LGV.maxDistanceForward, mov.LGV.heightToCast, mov.LastInputDirection3D.z * mov.LGV.maxDistanceForward);

		Gizmos.color = Color.red;
		Gizmos.DrawLine(startPos, startPos + Vector3.down * mov.LGV.raycastDistance);

		Gizmos.color = Color.blue;
		Gizmos.DrawLine(startPos + Vector3.down * mov.LGV.raycastDistance, (startPos + Vector3.down * mov.LGV.raycastDistance) + Vector3.up *mov.LGV.heightLeeway);
		Gizmos.DrawLine(startPos + Vector3.down * mov.LGV.raycastDistance, (startPos + Vector3.down * mov.LGV.raycastDistance) + Vector3.down *mov.LGV.heightLeeway);
	}
}
