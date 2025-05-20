using UnityEngine;

public class WaveTrigger : MonoBehaviour
{
    [SerializeField] private WaveController waveController;
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
