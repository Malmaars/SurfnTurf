using UnityEngine;

public class WaveTrigger : MonoBehaviour
{
    public WaveController waveController;
   private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (waveController != null)
            {
                waveController.playerOnWave = true;
                waveController.ResetPlayerVelocity();
            }
        }
    }

}
