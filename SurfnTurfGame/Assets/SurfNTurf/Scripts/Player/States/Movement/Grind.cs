using UnityEngine;
using UnityEngine.Analytics;

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

		if(mov.av.gv.grinding)
			DoGrind();

		else
			mov.av.gv.grindGravityOff = false;
	}

	void DoGrind()
	{
		//let the player grind the rail

		//check if the player is facing forward or backward to the rail

		//grind above the rail
		/*		if (mov.av.gv.grindDirection.y >= 0)
					mov.av.gv.grindSpeed -= Time.deltaTime * mov.av.gv.grindDamping;

				if(mov.velocity.magnitude < mov.av.gv.maxGrindSpeed && mov.av.gv.grindDirection.y < 0)
					mov.av.gv.grindSpeed += Time.deltaTime * mov.av.gv.grindSpeedUp;

				if (mov.av.gv.grindSpeed <= 0)
					mov.av.gv.grindSpeed = 0;

				mov.velocity = mov.velocity.normalized * mov.av.gv.grindSpeed;*/

		Vector3 railForward = mov.av.gv.currentRail.transform.up;

		if (Vector3.Dot(railForward, mov.playerVisual.transform.forward) > Vector3.Dot(-railForward, mov.playerVisual.transform.forward))
			mov.av.gv.grindDirection = railForward;

		else
			mov.av.gv.grindDirection = -railForward;

		float distanceFromCenter = Vector3.Distance(mov.rb.position, mov.av.gv.currentRail.transform.position);

		Vector3 directionFromRail = (mov.rb.position - mov.av.gv.currentRail.transform.position).normalized;
		if (Vector3.Dot(mov.av.gv.currentRail.up, directionFromRail) > Vector3.Dot(-mov.av.gv.currentRail.up, directionFromRail))
			directionFromRail = mov.av.gv.currentRail.up;
		else
			directionFromRail = -mov.av.gv.currentRail.up;

		Vector3 closestPointToRail = mov.av.gv.currentRail.transform.position + directionFromRail * distanceFromCenter;

		/*if (mov.rb.position.y <= (closestPointToRail.y + (mov.av.gv.currentRail.forward * mov.av.gv.offsetFromPole).y))
			mov.av.gv.grindGravityOff = true;
		else
			mov.av.gv.grindGravityOff = false;
*/

	}

	bool CheckForRails()
	{
		//check if there's a rail below the player
		if (!mov.suv.surfing)
			return false;

		Collider[] cols = Physics.OverlapSphere(mov.rb.position + Vector3.down * mov.av.gv.offsetFromPlayer, mov.av.gv.checkSize);

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
				if(Vector3.Dot(mov.av.gv.currentRail.up, directionFromRail) > Vector3.Dot(-mov.av.gv.currentRail.up, directionFromRail))
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
		Gizmos.DrawSphere(mov.rb.position + Vector3.down * mov.av.gv.offsetFromPlayer, mov.av.gv.checkSize);
	}
}
