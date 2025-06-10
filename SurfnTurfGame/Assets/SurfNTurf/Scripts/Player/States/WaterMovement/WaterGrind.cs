using UnityEngine;

public class WaterGrind : WaterAbility
{
	public WaterGrind(WaterMovementController _mov) : base(_mov) { }

	public override void RunOnUpdateDuringSetVelocity()
	{
		HandleGrind();
	}

	void HandleGrind()
	{
		mov.gv.grinding = CheckForRails();

		if (mov.gv.grinding)
			DoGrind();

		else
			mov.gv.grindGravityOff = false;
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

		Vector3 railForward = mov.gv.currentRail.transform.up;

		if (Vector3.Dot(railForward, mov.playerVisual.transform.forward) > Vector3.Dot(-railForward, mov.playerVisual.transform.forward))
			mov.gv.grindDirection = railForward;

		else
			mov.gv.grindDirection = -railForward;

		float distanceFromCenter = Vector3.Distance(mov.rb.position, mov.gv.currentRail.transform.position);

		Vector3 directionFromRail = (mov.rb.position - mov.gv.currentRail.transform.position).normalized;
		if (Vector3.Dot(mov.gv.currentRail.up, directionFromRail) > Vector3.Dot(-mov.gv.currentRail.up, directionFromRail))
			directionFromRail = mov.gv.currentRail.up;
		else
			directionFromRail = -mov.gv.currentRail.up;

		Vector3 closestPointToRail = mov.gv.currentRail.transform.position + directionFromRail * distanceFromCenter;

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

        Collider[] cols = Physics.OverlapBox(mov.rb.position + Vector3.down * mov.gv.offsetFromPlayer, new Vector3(mov.gv.checkSize.x, mov.gv.checkSize.y, mov.gv.checkSize.z), mov.playerVisual.rotation);

        foreach (Collider col in cols)
		{
			if (col.tag != "Rail")
				continue;

			mov.gv.currentRail = col.transform;

			if (!mov.gv.grinding)
			{
				Vector3 railForward = mov.gv.currentRail.transform.up;

				if (Vector3.Dot(railForward, mov.playerVisual.transform.forward) > Vector3.Dot(-railForward, mov.playerVisual.transform.forward))
					mov.gv.grindDirection = railForward;

				else
					mov.gv.grindDirection = -railForward;


				mov.gv.grindSpeed = mov.gv.baseGrindSpeed;
				mov.StopVelocity();

				float distanceFromCenter = Vector3.Distance(mov.rb.position, mov.gv.currentRail.transform.position);

				Vector3 directionFromRail = (mov.rb.position - mov.gv.currentRail.transform.position).normalized;
				if (Vector3.Dot(mov.gv.currentRail.up, directionFromRail) > Vector3.Dot(-mov.gv.currentRail.up, directionFromRail))
					directionFromRail = mov.gv.currentRail.up;
				else
					directionFromRail = -mov.gv.currentRail.up;

				Vector3 closestPointToRail = mov.gv.currentRail.transform.position + directionFromRail * distanceFromCenter;

				mov.rb.position = new Vector3(closestPointToRail.x, closestPointToRail.y + (mov.gv.currentRail.forward * mov.gv.offsetFromPole).y, closestPointToRail.z);
				mov.gv.grindGravityOff = true;

				mov.velocity = mov.gv.grindDirection * mov.gv.grindSpeed;
			}
			return true;
		}
		mov.gv.currentRail = null;
		return false;
	}

	public override void RunOnDrawGizmos()
	{

		if (!mov.gv.gizmosOn)
			return;

		Gizmos.color = Color.yellow;
        Gizmos.DrawCube(mov.rb.position + Vector3.down * mov.gv.offsetFromPlayer, mov.gv.checkSize / 2);

    }
}
