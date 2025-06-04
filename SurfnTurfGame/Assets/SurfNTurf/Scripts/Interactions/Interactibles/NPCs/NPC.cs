using System;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Cinemachine;
using Google.Apis.Util;
using NaughtyAttributes;
using FMODUnity;

public enum NPCState
{
    initialBeforeChallenge = 0,
    beforeChallenge = 1,
    duringChallenge = 2,
    afterBadChallenge = 3,
    afterGoodChallenge = 4
}

public class NPC : Interactible, IDishGetter
{
    [Header("Refs"),
    SerializeField]
    CinemachineCamera npcCamera;

    [Header("Chatbox Settings")]
    public TalkingUI talkingUi;
    public bool TextBubbleLooksAtCamera;

    [Header("Sound Refs")]
    public EventReference startTalkingSound;
    public EventReference nextDialogSound;

    [Header("Conversation Settings")]
    public NPCState currentNPCState;
    public string myName;
    public Conversations conversations;

    int convoIndex;
    bool talking;
    bool servingManagerOpened;
    bool dontSetCam = true;

    public PlateHolder currentDish { get; private set; }

    public override void Initialize()
    {
        base.Initialize();
        talkingUi.Initialize();
    }
    [Button("Set Initial State")]
    public virtual void VirtualInteract()
    {
        dontSetCam = false;
        InteractWith();
        dontSetCam = true;
    }
    public override bool InteractWith()
    {
        string[] currentConvo = conversations.beforeChallengeConversation.sentences;
        currentConvo = conversations.GetConvoFromState(currentNPCState).sentences;

        if (convoIndex == 0)
        {
            if (!talking)
            {
                SpawnTextBubble();
                RuntimeManager.PlayOneShot(startTalkingSound);
                if (dontSetCam)
                    BlackBoard.cameraController.SwitchToCamera(npcCamera, 0.5f);
            }
            talkingUi.SetTitle(myName);
        }

        if (convoIndex >= currentConvo.Length)
        {
            DespawnTextBubble();
            return false;
        }

        if (convoIndex < currentConvo.Length)
        {
            RuntimeManager.PlayOneShot(nextDialogSound);
            talkingUi.SetText(currentConvo[convoIndex]);
            convoIndex++;
        }

        return true;
    }

    public override void Update()
    {
        base.Update();
        if (talking && TextBubbleLooksAtCamera)
            talkingUi.transform.forward = new Vector3((talkingUi.transform.position - Camera.main.transform.position).x, 0, (talkingUi.transform.position - Camera.main.transform.position).z).normalized;
    }

    public override bool Exit()
    {
        ServingManager.instance.CloseServingMenu();
        DespawnTextBubble();
        return false;
    }
    void SpawnTextBubble()
    {
        talking = true;
        talkingUi.SpawnTextBubble(TextBubbleType.sweet);
    }

    void DespawnTextBubble()
    {
        talking = false;
        convoIndex = 0;
        servingManagerOpened = false;
        talkingUi.DespawnTextBubble();
    }


    public override void Highlight()
    {
        base.Highlight();
    }

    public override void RemoveHighlight()
    {
        base.RemoveHighlight();
    }

    public bool GiveDish(PlateHolder _dish)
    {
        return false;
    }

    //IDishGetter Values

}

[Serializable]
public class Conversation
{
    public string[] sentences;

}
[Serializable]
public class Conversations
{
    public Conversation initialBeforeChallengeConversation;
    public Conversation beforeChallengeConversation;
    public Conversation duringChallengeConversation;
    public Conversation afterBadChallengeConversation;
    public Conversation afterGoodChallengeConversation;

    public Conversation GetConvoFromState(NPCState state)
    {
        switch (state)
        {
            case NPCState.initialBeforeChallenge:
                return initialBeforeChallengeConversation;
            case NPCState.beforeChallenge:
                return beforeChallengeConversation;
            case NPCState.duringChallenge:
                return duringChallengeConversation;
            case NPCState.afterBadChallenge:
                return afterBadChallengeConversation;
            case NPCState.afterGoodChallenge:
                return afterGoodChallengeConversation;
            default:
                return null;
        }
    }
}
