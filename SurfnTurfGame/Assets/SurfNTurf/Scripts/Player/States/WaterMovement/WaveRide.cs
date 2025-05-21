using Steamworks;
using Unity.VisualScripting;
using UnityEngine;

public class WaveRide : WaterAbility
{
	public WaveRide(WaterMovementController _mov) : base(_mov)
	{
	}

	public override void RunOnUpdateDuringSetVelocity()
	{
		CheckForWaves();
		HandleWaveRide();
	}

	void CheckForWaves()
	{
		//cast a physics sphere to check for waves
		Collider[] cols = Physics.OverlapSphere(mov.rb.transform.position, mov.wrv.waveCheckSize, mov.wrv.waveLayer);
	
		foreach(Collider c in cols)
		{
			if(c.GetComponent<WaveTrigger>() != null)
			{
				//we are on a wave
				mov.wrv.onWave = true;
				mov.wrv.currentWave = c.GetComponent<WaveTrigger>();
				mov.wrv.previousWavePosition = mov.wrv.currentWave.transform.position;
				mov.rb.linearVelocity = Vector3.zero;
				mov.velocity = Vector3.zero;
				return;
			}
		}

		if (mov.wrv.onWave)
		{
			mov.wrv.onWave = false;
			mov.wrv.currentWave = null;
			mov.velocity = mov.wrv.currentWaveVelocity;
			mov.lastInputDirection3D = new Vector3(mov.wrv.currentWaveVelocity.normalized.x, 0, mov.wrv.currentWaveVelocity.z);
		}
	}

	void HandleWaveRide()
	{
		if (!mov.wrv.onWave || !mov.suv.surfing)
			return;

		mov.rb.linearVelocity = Vector3.zero;
		mov.velocity = Vector3.zero;

		if (mov.wrv.previousWavePosition != null)
			mov.wrv.currentWaveVelocity = mov.wrv.currentWave.waveController.surfLocation.position - mov.wrv.previousWavePosition;

		mov.wrv.previousWavePosition = mov.wrv.currentWave.waveController.surfLocation.position;
		mov.rb.transform.position = mov.wrv.currentWave.waveController.surfLocation.position; 
	}

	public override void RunOnDrawGizmos()
	{
		if (!mov.wrv.gizmosOn)
			return;
		Gizmos.color = Color.cyan;
		Gizmos.DrawSphere(mov.rb.transform.position, mov.wrv.waveCheckSize);
	}
}
