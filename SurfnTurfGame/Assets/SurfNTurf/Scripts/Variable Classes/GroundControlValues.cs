using NaughtyAttributes;
using UnityEngine;

[System.Serializable]
public class GroundControlValues
{

	public bool gizmosOn;

	public bool eightWayDirectionInput;
	public bool SlowWalkingOn;

	[SerializeField, Range(0f, 100f)]
	public float maxSpeed = 10f;

	[SerializeField, Range(0f, 500f)]
	public float maxAcceleration = 10f;

	[SerializeField, Range(0f, 90f)]
	public float maxGroundAngle = 25f;
	[ReadOnly]
	[AllowNesting]
	public float minGroundDotProduct;

	[SerializeField, Range(0f, 90f)]
	public float minSlopeAngle = 25f;
	[ReadOnly]
	[AllowNesting]
	public float minSlopeDotProduct;

	public LayerMask groundedLayerMask;

	[ReadOnly]
	[AllowNesting]
	public int groundContactCount;

	[ReadOnly]
	[AllowNesting]
	public bool grounded, onSlope;

	[SerializeField, Range(0f, 100f)]
	public float maxSnapSpeed = 100f;

	[SerializeField, Min(0f)]
	public float groundSnapProbeDistance = 1f;

	[ReadOnly]
	[AllowNesting]
	public Vector3 contactNormal;

	[SerializeField, Range(0f, 100f)]
	public float slopeGlideStrength;

	[SerializeField, Range(0f, 100f)]
	public float maxSlopeAcceleration = 1f;

	[SerializeField, Range(0f, 10f)]
	public float forwardRaysDistance = 2f;
}