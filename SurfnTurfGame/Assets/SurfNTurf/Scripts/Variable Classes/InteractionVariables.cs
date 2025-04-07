
using NaughtyAttributes;
using UnityEngine;

[System.Serializable]
public class InteractionVariables
{
	public bool gizmosOn;

	[ReadOnly]
	[AllowNesting]
	public Interactible currentInteractible;

	[ReadOnly]
	[AllowNesting]
	public bool interacting;

	[SerializeField, Range(0f, 10f)]
	public float measuringDistance;
}