using UnityEngine;

public class TrackingTarget : MonoBehaviour
{
    public Transform target;
	public float damping;

	private void Awake()
	{
		transform.position = target.position;
	}

	public void FollowTarget()
	{
		transform.position = Vector3.Lerp(transform.position, target.position, damping * Time.deltaTime);
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawSphere(transform.position, 0.5f);
	}
}
