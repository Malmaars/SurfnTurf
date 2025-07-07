using UnityEngine;
[RequireComponent(typeof(FakeRigidbody))]
public class MovingPlatform : MonoBehaviour
{
	FakeRigidbody rb;
	public bool local;
	public Vector3 A, B;
	public float speed;

	public float lastDistance = 0;
	bool toA;

	private void Awake()
	{
		rb = GetComponent<FakeRigidbody>();

		if (local)
			transform.localPosition = A;
		else
			transform.position = A;
		toA = false;
	}
	private void Update()
	{
		if (toA)
		{
			if (local)
			{
				Vector3 direction = A - transform.localPosition;

				Vector3 velocity = direction.normalized * speed;
				transform.localPosition = (transform.localPosition + velocity * Time.deltaTime);

				rb.velocity = velocity;

				if (lastDistance != 0 && direction.magnitude > lastDistance)
				{
					toA = false;
					lastDistance = 0;
				}
				else
					lastDistance = direction.magnitude;
			}
			else
			{
				Vector3 direction = A - transform.position;

				Vector3 velocity = direction.normalized * speed;
				transform.position = (transform.position + velocity * Time.deltaTime);

				rb.velocity = velocity;

				if (lastDistance != 0 && direction.magnitude > lastDistance)
				{
					toA = false;
					lastDistance = 0;
				}
				else
					lastDistance = direction.magnitude;
			}
		}
		else
		{
			if (local)
			{

				Vector3 direction = B - transform.localPosition;

				Vector3 velocity = direction.normalized * speed;
				transform.localPosition = (transform.localPosition + velocity * Time.deltaTime);

				rb.velocity = velocity;

				if (lastDistance != 0 && direction.magnitude > lastDistance)
				{
					toA = true;
					lastDistance = 0;
				}
				else
					lastDistance = direction.magnitude;

			}
			else
			{

				Vector3 direction = B - transform.position;

				Vector3 velocity = direction.normalized * speed;
				transform.position = (transform.position + velocity * Time.deltaTime);

				rb.velocity = velocity;

				if (lastDistance != 0 && direction.magnitude > lastDistance)
				{
					toA = true;
					lastDistance = 0;
				}
				else
					lastDistance = direction.magnitude;
			}

		}
	}
}
