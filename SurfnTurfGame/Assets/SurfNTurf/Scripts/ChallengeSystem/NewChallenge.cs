using UnityEngine;
using System.Collections;
using UnityEngine.Playables;
using NaughtyAttributes;
using UnityEngine.Events;

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
    public UnityEvent onCompleet;


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
    public void RespawnItems()
    {
        foreach (PickUpIngredient item in pickUps)
        {
            item.Initialize();
            item.isActive = false;
            item.gameObject.SetActive(true);
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
            pickUp.hasInteracted = false;
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
        BlackBoard.playerBody.GetComponent<MovementController>().animator.SetBool("CookingStation", false);
        failedDirector.Play();
        npc.currentNPCState = NPCState.afterBadChallenge;
    }

    public void OnFailedDirectorStopped(PlayableDirector director)
    {
        BlackBoard.playerManager.SwitchState(typeof(MovementController));
        BlackBoard.challengeManager.QuitChallenge();
        alreadyInitiated = true;
        npc.currentNPCState = NPCState.beforeChallenge;
        RespawnItems();
    }

    public void StartPresentingDirector()
    {
        BlackBoard.playerManager.SwitchState(typeof(CutsceneState));
        BlackBoard.playerBody.GetComponent<MovementController>().animator.SetBool("CookingStation", false);
        presentingDirector.Play();
        npc.currentNPCState = NPCState.afterGoodChallenge;
        npc.animator.SetTrigger("Eat");
        npc.vfx.SendEvent("OnPlay");
        onCompleet.Invoke();
    }

    public void OnPresentingDirectorStopped(PlayableDirector director)
    {
        BlackBoard.playerManager.SwitchState(typeof(MovementController));
        BlackBoard.challengeManager.QuitChallenge();
        alreadyInitiated = true;
        npc.currentNPCState = NPCState.beforeChallenge;
        RespawnItems();
    }
}
