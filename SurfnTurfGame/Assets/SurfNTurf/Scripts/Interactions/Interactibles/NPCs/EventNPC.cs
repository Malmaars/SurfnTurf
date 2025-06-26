using System;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Cinemachine;
using Google.Apis.Util;
using NaughtyAttributes;
using FMODUnity;
using UnityEngine.VFX;
using UnityEngine.Events;

public class EventNPC : Interactible
{
    [Header("Refs"),
    SerializeField]
    public CinemachineCamera npcCamera;
    public float cameraSwitchTime;
    public Animator animator;
    public VisualEffect vfx;

    [Header("Chatbox Settings")]
    public TalkingUI talkingUi;
    public bool TextBubbleLooksAtCamera;

    [Header("Sound Refs")]
    public EventReference startTalkingSound;
    public EventReference nextDialogSound;

    [Header("Conversation Settings")]
    public string myName;
    public Convo[] conversations;

    int convoIndex;
    int sentenceIndex;
    bool talking;
    bool dontSetCam = true;

    private Vector3 basePos;
    private Quaternion baseRot;
    private Vector3 baseSca;

    public void Start()
    {
        basePos = talkingUi.transform.localPosition;
        baseRot = talkingUi.transform.localRotation;
        baseSca = talkingUi.transform.localScale;
    }

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
        Sentence[] currentSentences = conversations[convoIndex].sentences;

        if (sentenceIndex == 0)
        {
            if (!talking)
            {
                conversations[convoIndex].eventAtBeginConvo.Invoke();
                SpawnTextBubble();
                RuntimeManager.PlayOneShot(startTalkingSound);
                if (dontSetCam)
                    BlackBoard.cameraController.SwitchToCamera(npcCamera, 0.5f);
            }
            talkingUi.SetTitle(myName);
        }

        if (sentenceIndex >= currentSentences.Length)
        {
            DespawnTextBubble();
            conversations[convoIndex].eventAtEndConvo.Invoke();
            return false;
        }

        if (sentenceIndex < currentSentences.Length)
        {
            RuntimeManager.PlayOneShot(nextDialogSound);
            talkingUi.SetText(ParceCellDataColors(currentSentences[sentenceIndex].sentence));
            currentSentences[sentenceIndex].eventAtSentence.Invoke();
            sentenceIndex++;
        }

        return true;
    }
    private string ParceCellDataColors(string sentence)
    {
        foreach (CellData cellData in BlackBoard.cookingDatabase.cellDatas)
        {
            if (sentence.Contains(cellData.cellName, StringComparison.OrdinalIgnoreCase))
            {
                string colorHex = UnityEngine.ColorUtility.ToHtmlStringRGB(cellData.color);
                string coloredText = $"<color=#{colorHex}>{cellData.cellName}</color>";
                sentence = sentence.Replace(cellData.cellName, coloredText, StringComparison.OrdinalIgnoreCase);
            }
        }
        return sentence;
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
        sentenceIndex = 0;
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

    public void SwitchConversation(int index)
    {
        if (conversations.Length > index)
            convoIndex = index;
    }

    public void ShowText(int _convoIndex, int _sentenceIndex)
    {
        if (conversations.Length <= _convoIndex)
            return;
        if (conversations[_convoIndex].sentences.Length <= _sentenceIndex)
            return;
        SpawnTextBubble();
        talkingUi.SetTitle(myName);

        RuntimeManager.PlayOneShot(nextDialogSound);
        talkingUi.SetText(ParceCellDataColors(conversations[_convoIndex].sentences[_sentenceIndex].sentence));
        conversations[_convoIndex].sentences[_sentenceIndex].eventAtSentence.Invoke();
    }

    public void HideText()
    {
        DespawnTextBubble();
    }

    public void MoveTalkBubble(Vector3 localPosition, Vector3 localRotation, Vector3 localScale)
    {
        Transform uiTransform = talkingUi.transform;
        uiTransform.localPosition = localPosition;
        uiTransform.localRotation = Quaternion.Euler(localRotation.x, localRotation.y, localRotation.z);
        uiTransform.localScale = localScale;
    }

    public void ResetTalkBubbleTransform()
    {
        Transform uiTransform = talkingUi.transform;
        uiTransform.localPosition = basePos;
        uiTransform.localRotation = baseRot;
        uiTransform.localScale = baseSca;
    }
}

[Serializable]
public class Convo
{
    public string convoName;
    public Sentence[] sentences;
    public UnityEvent eventAtBeginConvo;
    public UnityEvent eventAtEndConvo;
}

[Serializable]
public class Sentence
{
    public string sentence;
    public UnityEvent eventAtSentence;
}

