using UnityEngine;

public class TrackingTarget : MonoBehaviour
{
    public Transform target;
	public Rigidbody targetRB;

	public float damping;

	private void Awake()
	{
		transform.position = target.position;
	}

	public void FollowTarget(float _speed)
	{
		transform.position = Vector3.Lerp(transform.position, target.position, _speed * Time.deltaTime);
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawSphere(transform.position, 0.5f);
	}
}