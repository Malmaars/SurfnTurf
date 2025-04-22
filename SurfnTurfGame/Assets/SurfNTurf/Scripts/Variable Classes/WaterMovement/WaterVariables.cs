using NaughtyAttributes;
using UnityEngine;

public class WaterVariables : VariableClass
{
	public LayerMask waterLayerMask;

	[ReadOnly]
	[AllowNesting]
	public int waterContactCount;

	[ReadOnly]
	[AllowNesting]
	public bool onWater;

	[ReadOnly]
	[AllowNesting]
	public Vector3 contactNormal;

	[ReadOnly]
	[AllowNesting]
	public Vector3[] allContactNormals;
}
