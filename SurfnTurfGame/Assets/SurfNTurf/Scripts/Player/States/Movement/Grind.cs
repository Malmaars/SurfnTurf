using UnityEngine;
using UnityEngine.Analytics;
using static UnityEditor.FilePathAttribute;
using UnityEngine.UIElements;

public class Grind : Ability
{
	public Grind(MovementController _mov) : base(_mov) { }

	public override void RunOnUpdateDuringSetVelocity()
	{
		HandleGrind();
	}

	void HandleGrind()
	{
		mov.av.gv.grinding = CheckForRails();

		if (mov.av.gv.grinding)
		{
			DoGrind();
		}
		else
		{
			mov.av.gv.grindGravityOff = false;
			PlayerVFX.instance.RailGrind.SendEvent("OnStop");
		}
	}

	void DoGrind()
	{
		PlayerVFX.instance.RailGrind.SendEvent("OnPlay");
		//let the player grind the rail

		//check if the player is facing forward or backward to the rail

		Vector3 railForward = mov.av.gv.currentRail.transform.up;

		if (Vector3.Dot(railForward, mov.playerVisual.transform.forward) > Vector3.Dot(-railForward, mov.playerVisual.transform.forward))
			mov.av.gv.grindDirection = railForward;

		else
			mov.av.gv.grindDirection = -railForward;
	}

	bool CheckForRails()
	{
		//check if there's a rail below the player
		if (!mov.suv.surfing)
			return false;

		Collider[] cols = Physics.OverlapBox(mov.rb.position + Vector3.down * mov.av.gv.offsetFromPlayer, new Vector3(mov.av.gv.checkSize.x, mov.av.gv.checkSize.y, mov.av.gv.checkSize.z), mov.playerVisual.rotation);

		foreach (Collider col in cols)
		{
			if (col.tag != "Rail")
				continue;

			mov.av.gv.currentRail = col.transform;

			if (!mov.av.gv.grinding)
			{
				Vector3 railForward = mov.av.gv.currentRail.transform.up;

				if (Vector3.Dot(railForward, mov.playerVisual.transform.forward) > Vector3.Dot(-railForward, mov.playerVisual.transform.forward))
					mov.av.gv.grindDirection = railForward;

				else
					mov.av.gv.grindDirection = -railForward;


				mov.av.gv.grindSpeed = mov.av.gv.baseGrindSpeed;
				mov.StopVelocity();

				float distanceFromCenter = Vector3.Distance(mov.rb.position, mov.av.gv.currentRail.transform.position);

				Vector3 directionFromRail = (mov.rb.position - mov.av.gv.currentRail.transform.position).normalized;
				if (Vector3.Dot(mov.av.gv.currentRail.up, directionFromRail) > Vector3.Dot(-mov.av.gv.currentRail.up, directionFromRail))
					directionFromRail = mov.av.gv.currentRail.up;
				else
					directionFromRail = -mov.av.gv.currentRail.up;

				Vector3 closestPointToRail = mov.av.gv.currentRail.transform.position + directionFromRail * distanceFromCenter;

				mov.rb.position = new Vector3(closestPointToRail.x, closestPointToRail.y + (mov.av.gv.currentRail.forward * mov.av.gv.offsetFromPole).y, closestPointToRail.z);
				mov.av.gv.grindGravityOff = true;

				mov.velocity = mov.av.gv.grindDirection * mov.av.gv.grindSpeed;
			}
			return true;
		}
		mov.av.gv.currentRail = null;
		return false;
	}

	public override void RunOnDrawGizmos()
	{

		if (!mov.av.gv.gizmosOn)
			return;

		Gizmos.color = Color.yellow;
        Matrix4x4 prevMatrix = Gizmos.matrix;
		Vector3 position = mov.rb.position + Vector3.down * mov.av.gv.offsetFromPlayer;

        Gizmos.matrix = Matrix4x4.TRS(position, mov.playerVisual.rotation, Vector3.one);
        Gizmos.DrawCube(Vector3.zero, mov.av.gv.checkSize/2);
        Gizmos.matrix = prevMatrix;
    }
}
