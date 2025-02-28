using System;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class NPC : Interactible
{
    [SerializeField]
    Canvas inputPromptCanvas;
    RectTransform textBubble;

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
        //spawn a text bubble if

        if (convoIndex == 0)
        {
            SpawnTextBubble();
            convoIndex++;
        }
        else if (convoIndex < convo.sentences.Length - 1)
        {
            convoIndex++;
            talkingUi.SetText(convo.sentences[convoIndex]);
        }
        else
        {
            DespawnTextBubble();
            return false;
        }
        return true;
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
