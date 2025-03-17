using System;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Cinemachine;

public class NPC : Interactible
{
    [SerializeField]
    Canvas inputPromptCanvas;
    RectTransform textBubble;
    [SerializeField]
	CinemachineCamera npcCamera;

    public bool TextBubbleLooksAtCamera;

    public TalkingUI talkingUi;
    
    public Conversation convo;
    int convoIndex;
    bool talking;


	public override void Initialize()
	{
		base.Initialize();
        talkingUi.Initialize();
	}
	public override bool InteractWith()
	{
        if (convoIndex == 0)
        {
            SpawnTextBubble();
            BlackBoard.cameraController.SwitchToCamera(npcCamera, 0.5f);
            convoIndex++;
        }
        else if (convoIndex < convo.sentences.Length)
        {
            talkingUi.SetText(convo.sentences[convoIndex]);
			convoIndex++;
		}
		else
        {
            DespawnTextBubble();
            return false;
        }
        return true;
	}

	private void Update()
	{
		if(talking && TextBubbleLooksAtCamera)
            talkingUi.transform.forward = new Vector3((talkingUi.transform.position - Camera.main.transform.position).x, 0, (talkingUi.transform.position - Camera.main.transform.position).z).normalized;
		
	}

    public override bool Exit()
    {
        DespawnTextBubble();
        return false;
    }
    void SpawnTextBubble()
    {
        talking = true;
		talkingUi.SpawnTextBubble(TextBubbleType.sweet);
		talkingUi.SetTitle(convo.myName);
        talkingUi.SetText(convo.sentences[convoIndex]);
    }

    void DespawnTextBubble()
    {
        talking = false;
        convoIndex = 0;
		talkingUi.DespawnTextBubble();
	}


	public override void Highlight()
    {
        base.Highlight();
        //show a little button above the character that indicates interaction
        if(!inputPromptCanvas.gameObject.activeSelf)
            inputPromptCanvas.gameObject.SetActive(true);

        inputPromptCanvas.transform.forward = (inputPromptCanvas.transform.position - Camera.main.transform.position).normalized;
    }

    public override void RemoveHighlight()
    {
        base.RemoveHighlight();
        inputPromptCanvas.gameObject.SetActive(false);
    }
}

[Serializable]
public class Conversation
{
    public string myName;
    public string[] sentences;
}
