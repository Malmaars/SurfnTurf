using UnityEngine;

public class SequencedPlatformsManager : MonoBehaviour
{
    public SequencedPlatform[] platforms;

    [Header("Sequence Settings")]
    public float timePerLoop;
    private float loopTimer;
    [Range(1,100)]
    public int loopAmount = 1;
    private int loopCounter;
    public bool loopInfinitely;
    public bool activeOnStart;

    private bool isLooping;

    private void Start()
    {
        if (activeOnStart)
            StartLoop();
    }

    private void Update()
    {
        if (!isLooping) return;

        loopTimer -= Time.deltaTime;

        if (loopTimer <= 0)
        {
            isLooping = false;
            if (!loopInfinitely && loopCounter >= loopAmount)
            {
                StopLoop();
            }
            else if(loopInfinitely || loopCounter < loopAmount)
            {
                StartLoop();
            }
        }
    }

    public void StartLoop()
    {
        if (platforms == null || platforms.Length == 0 || isLooping)
            return;

        isLooping = true;
        loopTimer = timePerLoop;
        loopCounter += 1;

        TriggerPlatforms();
    }

    private void TriggerPlatforms()
    {
        foreach (var platform in platforms)
        {
            platform.Prime();
        }
    }

    public void StopLoop()
    {
        isLooping = false;
        loopCounter = 0;
    }
}
