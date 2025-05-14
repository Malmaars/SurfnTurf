using UnityEngine;

public class TrackingTarget : MonoBehaviour
{
	public bool gizmosOn;
	public bool instantFollow;

    public Transform target;
	public Rigidbody targetRB;
	Vector3 targetPosition;

	public float yOffset;
	public float yOffsetMargin;

	public float damping;

	public LayerMask raycastLayers;

	private void Awake()
	{
		transform.position = target.position;
	}

	public void FollowTarget(float _speed)
	{
		targetPosition = target.position;
		AddHeightOffset();

		float distance = Vector3.Distance(targetPosition, transform.position);

		if(instantFollow)
			transform.position = targetPosition;
		else
			transform.position = Vector3.Lerp(transform.position, targetPosition, distance * damping * Time.deltaTime);
		//CheckForCollisions();
	}

	private void OnDrawGizmos()
	{
		if (!gizmosOn)
			return;
		Gizmos.color = Color.blue;
		Gizmos.DrawSphere(transform.position, 0.5f);
	}

	void CheckForCollisions()
	{
		//send a raycast from the camera to the target, if it hits something, move there

		RaycastHit hit;

		Physics.Raycast(Camera.main.transform.position, transform.position - Camera.main.transform.position, out hit, Vector3.Distance(Camera.main.transform.position, transform.position), raycastLayers);
		if (hit.point == null || hit.collider == null)
			return;

		transform.position = hit.point + (Camera.main.transform.position - transform.position).normalized * 0.01f;
	}

	void AddHeightOffset()
	{
		//shoot a raycast up to see if there's space for
		RaycastHit hit;

		bool didTheRayHit = Physics.Raycast(target.position, Vector3.up, out hit, yOffset + yOffsetMargin, raycastLayers);
		if (!didTheRayHit)
		{
			targetPosition += Vector3.up * yOffset;
			return;
		}
		targetPosition = new Vector3(hit.point.x, hit.point.y - yOffsetMargin, hit.point.z);
	}
}