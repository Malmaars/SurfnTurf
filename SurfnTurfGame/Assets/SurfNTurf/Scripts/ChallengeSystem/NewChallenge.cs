using UnityEngine;
using System.Collections;
using UnityEngine.Playables;
using NaughtyAttributes;

public class NewChallenge : MonoBehaviour
{
    [Header("Challenge Settings")]
    public float startTime;
    public float timeLeftCriteria;

    [Header("Sequence Settings")]
    public PlayableDirector initiationDirector;
    public PlayableDirector failedDirector;
    public PlayableDirector presentingDirector;

    private void OnEnable()
    {
        initiationDirector.stopped += OnInitiationDirectorStopped;
        failedDirector.stopped += OnFailedDirectorStopped;
        presentingDirector.stopped += OnPresentingDirectorStopped;
    }

    private void OnDisable()
    {
        initiationDirector.stopped -= OnInitiationDirectorStopped;
        failedDirector.stopped -= OnFailedDirectorStopped;
        presentingDirector.stopped -= OnPresentingDirectorStopped;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.transform.tag == "Player")
        {
            BlackBoard.challengeManager.RetrieveChallenge(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.tag == "Player")
        {
            BlackBoard.challengeManager.DiscardChallenge(this);
        }
    }
    [Button("Start Initiation Director")]
    public void StartInitiationDirector()
    {
        BlackBoard.playerManager.SwitchState(typeof(CutsceneState));
        initiationDirector.Play();
    }

    public void OnInitiationDirectorStopped(PlayableDirector director)
    {
        BlackBoard.playerManager.SwitchState(typeof(MovementController));
        BlackBoard.challengeManager.StartChallenge();
    }

    public void StartFailedDirector()
    {
        BlackBoard.playerManager.SwitchState(typeof(CutsceneState));
        failedDirector.Play();
    }

    public void OnFailedDirectorStopped(PlayableDirector director)
    {
        BlackBoard.playerManager.SwitchState(typeof(MovementController));
        BlackBoard.challengeManager.QuitChallenge();
    }

    public void StartPresentingDirector()
    {
        BlackBoard.playerManager.SwitchState(typeof(CutsceneState));
        presentingDirector.Play();
    }

    public void OnPresentingDirectorStopped(PlayableDirector director)
    {
        BlackBoard.playerManager.SwitchState(typeof(MovementController));
        BlackBoard.challengeManager.QuitChallenge();
    }
}
