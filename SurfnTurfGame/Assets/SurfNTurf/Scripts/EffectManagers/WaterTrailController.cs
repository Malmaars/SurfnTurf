using UnityEngine;
using UnityEngine.VFX;

public class WaterTrailController : MonoBehaviour
{
    [SerializeField] private Rigidbody rigidbody;
    [SerializeField] private VisualEffect visualEffect;
    private bool isPlaying;
    [SerializeField] private float threshold;

    // Update is called once per frame
    void Update()
    {
        //if velocity is greater than 0.1, set the visual effect to play
        if (rigidbody.linearVelocity.magnitude > threshold)
        {
            if (!isPlaying)
            {
                isPlaying = true;
                visualEffect.Play();
            }
        }
        else
        {
            visualEffect.Stop();
            isPlaying = false;
        }
    }
}
