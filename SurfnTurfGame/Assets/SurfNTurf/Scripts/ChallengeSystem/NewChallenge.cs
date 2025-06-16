using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Playables;
using NaughtyAttributes;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System;
using SurfnTurf;

public class NewChallenge : MonoBehaviour
{
    [Header("Challenge Settings")]
    public string challengeName;
    public float startTime;
    public float timeLeftCriteria;

    [Header("Sequence Settings")]
    public PlayableDirector initiationDirector;
    public PlayableDirector failedDirector;
    [Header("Pickups")]
    public PickUpIngredient[] pickUps;
    [Header("NPC")]
    public NPC npc;
    private bool alreadyInitiated;
    public UnityEvent onCompleet;
    [Header("Cooking Spot")]
    public CookingSpot cookingSpot;
    [Header("Coin Reward")]
    public int[] coinCountPerCriteria;
    [HideInInspector] public bool[] toGiveCoins;
    IEnumerator Start()
    {
        yield return new WaitUntil(() => CloudSaveSystem.Instance != null && CloudSaveSystem.Instance.IsInitialized);
        if (CloudSaveSystem.Instance.data.challengeDatas.Find(item => item.key == challengeName) != null) { }
        else
        {
            ChallengeData localChallengeData = new ChallengeData();
            localChallengeData.key = challengeName;
            CloudSaveSystem.Instance.data.challengeDatas.Add(localChallengeData);
        }
        UpdateCookingSpotData();
    }
    private void OnEnable()
    {
        initiationDirector.stopped += OnInitiationDirectorStopped;
        failedDirector.stopped += OnFailedDirectorStopped;
    }

    private void OnDisable()
    {
        initiationDirector.stopped -= OnInitiationDirectorStopped;
        failedDirector.stopped -= OnFailedDirectorStopped;
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

    public IEnumerator StartPresenting()
    {
        BlackBoard.playerManager.SwitchState(typeof(CutsceneState));
        BlackBoard.playerBody.GetComponent<MovementController>().animator.SetBool("CookingStation", false);
        BlackBoard.cameraController.SwitchToCamera(npc.npcCamera, npc.cameraSwitchTime);

        yield return new WaitForSeconds(npc.cameraSwitchTime + 0.5f);

        npc.GiveDish();

        SaveChallengeData();

        npc.currentNPCState = NPCState.afterGoodChallenge;
        npc.animator.SetTrigger("Eat");
        npc.vfx.SendEvent("OnPlay");

        yield return new WaitForSeconds(3f);

        ChallengeUI.instance.RunScoreScreen();
        
        onCompleet.Invoke();

        yield return new WaitForSeconds(2f);
        InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Interactions.Talk, ExitPresenting);
    }

    public void ExitPresenting(InputAction.CallbackContext context)
    {
        InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Interactions.Talk, ExitPresenting);
        ChallengeUI.instance.CloseScoreScreen();
        StartCoroutine(BlackBoard.cookingManager.CookingStationVisual(false));

        BlackBoard.playerManager.SwitchState(typeof(MovementController));
        BlackBoard.challengeManager.QuitChallenge();
        alreadyInitiated = true;
        npc.currentNPCState = NPCState.beforeChallenge;
        RespawnItems();
        UpdateCookingSpotData();
    }

    //saving functions
    public void UpdateCookingSpotData()
    {
        ChallengeData data = CloudSaveSystem.Instance.data.challengeDatas.Find(item => item.key == challengeName);
        cookingSpot.stars[0].SetActive(data.enoughScore);
        cookingSpot.stars[1].SetActive(data.satisfiedNPC);
        cookingSpot.stars[2].SetActive(data.inTime);
        cookingSpot.time.text = "HighScore: " + data.time.SecondsToTime();
        toGiveCoins = new bool[3];
    }

    public void SaveChallengeData()
    {
        ChallengeData data = CloudSaveSystem.Instance.data.challengeDatas.Find(item => item.key == challengeName);
        if (BlackBoard.challengeManager.totalTime < data.time)
            data.time = BlackBoard.challengeManager.totalTime;
        if (BlackBoard.challengeManager.enoughScore && !data.enoughScore)
        {
            data.enoughScore = true;
            toGiveCoins[0] = true;
        }
        if (BlackBoard.challengeManager.satisfiedNPC && !data.satisfiedNPC)
        {
            data.satisfiedNPC = true;
            toGiveCoins[1] = true;
        }
        if (BlackBoard.challengeManager.inTime && !data.inTime)
        {
            data.inTime = true;
            toGiveCoins[2] = true;
        }
    }

    public void GiveCoins(int value)
    {
        CoinSpawner.instance.StartCoroutine(CoinSpawner.instance.SpawnCoins(BlackBoard.playerBody.position, coinCountPerCriteria[value], true));
    }
}

[Serializable]
public class ChallengeData
{
    public string key;
    public bool enoughScore = false;
    public bool satisfiedNPC = false;
    public bool inTime = false;
    public float time = float.MaxValue;
}