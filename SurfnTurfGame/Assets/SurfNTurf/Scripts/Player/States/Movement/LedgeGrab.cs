using UnityEngine;

public class LedgeGrab : Ability
{
	public LedgeGrab(IMovement _mov) : base(_mov) { }

	public override void RunOnUpdateDuringSetVelocity()
	{
		CheckForLedge();
	}

	void CheckForLedge()
	{
		if (mov.LGV.ledgeGrabbing)
			return;

		//I want to check from a specific height if there's a ledge, if there is, grab onto it

		//the raycast will be cast downward from the desired distance and height

		Vector3 startPos = mov.RB.position + new Vector3(mov.LastInputDirection3D.x * mov.LGV.maxDistanceForward, mov.LGV.heightToCast, mov.LastInputDirection3D.z * mov.LGV.maxDistanceForward);

		RaycastHit hit;

		if(Physics.Raycast(startPos, Vector3.down, out hit, mov.LGV.raycastDistance + mov.LGV.heightLeeway, mov.LGV.ledgeGrabbable))
		{
			if(Vector3.Distance(hit.point, startPos) <= mov.LGV.raycastDistance + mov.LGV.heightLeeway && Vector3.Distance(hit.point, startPos) >= mov.LGV.raycastDistance - mov.LGV.heightLeeway)
			{
				//perform a ledgegrab
				DoLedgeGrab();
			}
		}
	}

	void HandleLedgeGrab()
	{
		if (mov.LGV.ledgeGrabDurationTimer > 0)
			mov.LGV.ledgeGrabDurationTimer -= Time.deltaTime;

		if(mov.LGV.ledgeGrabbing && mov.LGV.ledgeGrabDurationTimer <= 0)
		{
			//ledgegrab finished, teleport player;
			mov.RB.position = mov.RB.position + mov.LGV.teleportoffset;
			mov.LGV.ledgeGrabbing = true;
		}
	}

	void DoLedgeGrab()
	{
		//force the direction of the player towards the ledge, to make sure the animation plays properly

		mov.LGV.turnGravityOff = true;

		mov.LGV.ledgeGrabDurationTimer = mov.LGV.ledgeGrabDuration;
		mov.LGV.ledgeGrabbing = true;
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
