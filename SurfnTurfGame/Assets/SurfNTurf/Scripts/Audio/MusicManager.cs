using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [HideInInspector] public static MusicManager instance;
    public EventReference ambienceLoop;
    private EventInstance ambience;
    public EventReference musicLoop;
    private EventInstance music;
    public EventReference challengeMusicLoop;
    private EventInstance challengeMusic;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        ambience = RuntimeManager.CreateInstance(ambienceLoop);
        challengeMusic = RuntimeManager.CreateInstance(challengeMusicLoop);
        music = RuntimeManager.CreateInstance(musicLoop);

        ambience.start();
        challengeMusic.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        music.start();
    }

    public void StartChallengeMusic()
    {
        ambience.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        challengeMusic.start();
        music.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        
    }
    public void StopChallengeMusic()
    {
        ambience.start();
        challengeMusic.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        music.start();
    }
}
