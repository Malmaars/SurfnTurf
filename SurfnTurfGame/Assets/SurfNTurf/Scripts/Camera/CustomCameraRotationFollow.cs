using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class CustomCameraRotationFollow : MonoBehaviour
{
	public bool gizmosOn;
	public bool deadzone;
	public Vector3 Deadzone;
	public Vector3 DeadzoneOffset;

	public Transform target;
	public TrackingTarget trackingTarget;

	public Camera cam;           // Assign your camera

	private void Awake()
	{
		transform.position = target.position;
		trackingTarget = FindAnyObjectByType<TrackingTarget>();
	}
	void LateUpdate()
	{
		if (cam == null)
			cam = Camera.main;

		Vector3 offset = cam.transform.up * DeadzoneOffset.y + cam.transform.right * DeadzoneOffset.x + cam.transform.forward * DeadzoneOffset.z;

		Vector3 topFrontLeft = cam.transform.position + cam.transform.up * (Deadzone.y * 0.5f) - cam.transform.right * (Deadzone.x * 0.5f) + cam.transform.forward * (Deadzone.z * 0.5f) + offset;
		Vector3 topFrontRight = cam.transform.position + cam.transform.up * (Deadzone.y * 0.5f) + cam.transform.right * (Deadzone.x * 0.5f) + cam.transform.forward * (Deadzone.z * 0.5f) + offset;
		Vector3 bottomFrontLeft = cam.transform.position - cam.transform.up * (Deadzone.y * 0.5f) - cam.transform.right * (Deadzone.x * 0.5f) + cam.transform.forward * (Deadzone.z * 0.5f) + offset;
		Vector3 bottomFrontRight = cam.transform.position - cam.transform.up * (Deadzone.y * 0.5f) + cam.transform.right * (Deadzone.x * 0.5f) + cam.transform.forward * (Deadzone.z * 0.5f) + offset;
		Vector3 topBackLeft = cam.transform.position + cam.transform.up * (Deadzone.y * 0.5f) - cam.transform.right * (Deadzone.x * 0.5f) - cam.transform.forward * (Deadzone.z * 0.5f) + offset;
		Vector3 topBackRight = cam.transform.position + cam.transform.up * (Deadzone.y * 0.5f) + cam.transform.right * (Deadzone.x * 0.5f) - cam.transform.forward * (Deadzone.z * 0.5f	) + offset;
		Vector3 bottomBackLeft = cam.transform.position - cam.transform.up * (Deadzone.y * 0.5f) - cam.transform.right * (Deadzone.x * 0.5f) - cam.transform.forward * (Deadzone.z * 0.5f) + offset;
		Vector3 bottomBackRight = cam.transform.position - cam.transform.up * (Deadzone.y * 0.5f) + cam.transform.right * (Deadzone.x * 0.5f) - cam.transform.forward * (Deadzone.z * 0.5f) + offset;

		Vector3[] boxCorners = new Vector3[]
		{
			bottomFrontLeft,
			bottomFrontRight,
			bottomBackLeft,
			bottomBackRight,
			topFrontLeft,
			topFrontRight,
			topBackLeft,
			topBackRight
		};
		//check if the player is withing the given area of points
		bool isTargetInBox = IsPointInsideBox(target.position, boxCorners);

		if (!isTargetInBox)
		{
			//turn off the orbital follow and rotation composer
			trackingTarget.FollowTarget();
		}
	}

	bool IsPointInsideBox(Vector3 point, Vector3[] corners)
	{
		// Assume corners are ordered properly:
		// 0: bottom-front-left
		// 1: bottom-front-right
		// 2: bottom-back-left
		// 3: bottom-back-right
		// 4�7: same as above but top

		Vector3 origin = corners[0];

		// Box axes
		Vector3 right = (corners[1] - origin).normalized;
		Vector3 up = (corners[4] - origin).normalized;
		Vector3 forward = (corners[2] - origin).normalized;

		// Box dimensions
		float width = Vector3.Distance(corners[0], corners[1]);
		float height = Vector3.Distance(corners[0], corners[4]);
		float depth = Vector3.Distance(corners[0], corners[2]);

		// Vector from origin to point
		Vector3 local = point - origin;

		// Project the point onto the box's axes
		float x = Vector3.Dot(local, right);
		float y = Vector3.Dot(local, up);
		float z = Vector3.Dot(local, forward);

		// Check if the point lies within the box bounds
		return (x >= 0 && x <= width &&
				y >= 0 && y <= height &&
				z >= 0 && z <= depth);
	}

	private void OnDrawGizmos()
	{
		if (!gizmosOn)
			return;
		if (cam == null)
			cam = Camera.main;
		if (cam == null)
			return;


		Vector3 offset = cam.transform.up * DeadzoneOffset.y + cam.transform.right * DeadzoneOffset.x + cam.transform.forward * DeadzoneOffset.z;

		Vector3 topFrontLeft = cam.transform.position + cam.transform.up * (Deadzone.y * 0.5f) - cam.transform.right * (Deadzone.x * 0.5f) + cam.transform.forward * (Deadzone.z * 0.5f) + offset;
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(topFrontLeft, 0.5f);
		Vector3 topFrontRight = cam.transform.position + cam.transform.up * (Deadzone.y * 0.5f) + cam.transform.right * (Deadzone.x * 0.5f) + cam.transform.forward * (Deadzone.z * 0.5f) + offset;
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(topFrontRight, 0.5f);
		Vector3 bottomFrontLeft = cam.transform.position - cam.transform.up * (Deadzone.y * 0.5f) - cam.transform.right * (Deadzone.x * 0.5f) + cam.transform.forward * (Deadzone.z * 0.5f) + offset;
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(bottomFrontLeft, 0.5f);
		Vector3 bottomFrontRight = cam.transform.position - cam.transform.up * (Deadzone.y * 0.5f) + cam.transform.right * (Deadzone.x * 0.5f) + cam.transform.forward * (Deadzone.z * 0.5f) + offset;
		Gizmos.color = Color.blue;
		Gizmos.DrawSphere(bottomFrontRight, 0.5f);
		Vector3 topBackLeft = cam.transform.position + cam.transform.up * (Deadzone.y * 0.5f) - cam.transform.right * (Deadzone.x * 0.5f) - cam.transform.forward * (Deadzone.z * 0.5f) + offset;
		Gizmos.color = Color.white;
		Gizmos.DrawSphere(topBackLeft, 0.5f);
		Vector3 topBackRight = cam.transform.position + cam.transform.up * (Deadzone.y * 0.5f) + cam.transform.right * (Deadzone.x * 0.5f) - cam.transform.forward * (Deadzone.z * 0.5f) + offset;
		Gizmos.color = Color.black;
		Gizmos.DrawSphere(topBackRight, 0.5f);
		Vector3 bottomBackLeft = cam.transform.position - cam.transform.up * (Deadzone.y * 0.5f) - cam.transform.right * (Deadzone.x * 0.5f) - cam.transform.forward * (Deadzone.z * 0.5f) + offset;
		Gizmos.color = Color.gray;
		Gizmos.DrawSphere(bottomBackLeft, 0.5f);
		Vector3 bottomBackRight = cam.transform.position - cam.transform.up * (Deadzone.y * 0.5f) + cam.transform.right * (Deadzone.x * 0.5f) - cam.transform.forward * (Deadzone.z * 0.5f) + offset;
		Gizmos.color = Color.cyan;
		Gizmos.DrawSphere(bottomBackRight, 0.5f);

		// Draw line from origin to forward direction
		Gizmos.color = Color.red;
		Gizmos.DrawLine(topFrontLeft, topBackLeft);
		Gizmos.DrawLine(topFrontRight, topBackRight);
		Gizmos.DrawLine(bottomFrontLeft, bottomBackLeft);
		Gizmos.DrawLine(bottomFrontRight, bottomBackRight);

		Vector3[] points = new Vector3[]
		{
			bottomFrontLeft,
			bottomFrontRight,
			topFrontRight,
			topFrontLeft,
			bottomBackLeft,
			bottomBackRight,
			topBackRight,
			topBackLeft
		};

		Mesh mesh = new Mesh();

		mesh.vertices = points;

		mesh.triangles = new int[]
		{
            // Bottom face (0, 1, 2, 3)
            0, 2, 1,  0, 3, 2,

            // Top face (4, 5, 6, 7)
            4, 5, 6,  4, 6, 7,

            // Front face (3, 2, 6, 7)
            3, 6, 2,  3, 7, 6,

            // Back face (0, 1, 5, 4)
            0, 5, 1,  0, 4, 5,

            // Left face (0, 3, 7, 4)
            0, 7, 3,  0, 4, 7,

            // Right face (1, 2, 6, 5)
            1, 2, 6,  1, 6, 5
		};

		mesh.RecalculateNormals();
		mesh.RecalculateBounds();

		Gizmos.DrawMesh(mesh);
	}
}
