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
    [Header("Pickups")]
    public PickUpIngredient[] pickUps;
    [Header("NPC")]
    public NPC npc;
    private bool alreadyInitiated;

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
        if (!alreadyInitiated)
        {
            npc.currentNPCState = NPCState.initialBeforeChallenge;
        }
        else
        {
            npc.currentNPCState = NPCState.beforeChallenge;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.tag == "Player")
        {
            BlackBoard.challengeManager.DiscardChallenge(this);
        }
    }
    public void StartInitiationDirector()
    {
        BlackBoard.playerManager.SwitchState(typeof(CutsceneState));
        initiationDirector.Play();
        foreach (var pickUp in pickUps)
        {
            pickUp.gameObject.SetActive(true);
            pickUp.isActive = true;
        }
        npc.currentNPCState = NPCState.duringChallenge;
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
        npc.currentNPCState = NPCState.afterBadChallenge;
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
        npc.currentNPCState = NPCState.afterGoodChallenge;
    }

    public void OnPresentingDirectorStopped(PlayableDirector director)
    {
        BlackBoard.playerManager.SwitchState(typeof(MovementController));
        BlackBoard.challengeManager.QuitChallenge();
    }
}
