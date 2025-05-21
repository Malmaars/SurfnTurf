using NaughtyAttributes;
using System;
using UnityEngine;

[Serializable]
public class WaveRideVariables : VariableClass
{
	[SerializeField, Range(0f, 10f)]
	public float waveCheckSize;

	[SerializeField, Range(0f, 3f)]
	public float strafeSpeed;

	[ReadOnly]
	[AllowNesting]
	public Vector3 currentWaveVelocity;

	public Vector3 previousWavePosition;

	[ReadOnly]
	[AllowNesting]
	public bool onWave;

	[ReadOnly]
	[AllowNesting]
	public WaveTrigger currentWave;

	public LayerMask waveLayer;
}
