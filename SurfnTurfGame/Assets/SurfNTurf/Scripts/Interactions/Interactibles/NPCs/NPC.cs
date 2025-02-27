using System;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class NPC : Interactible
{
    [SerializeField]
    Canvas inputPromptCanvas;
    RectTransform textBubble;

    TalkingUI talkingUi;
    
    public Conversation convo;
    int convoIndex;
    bool talking;


	public override void Initialize()
	{
		base.Initialize();
        talkingUi = FindFirstObjectByType<TalkingUI>();
	}
	public override bool InteractWith()
	{
        //spawn a text bubble if

        if (convoIndex == 0)
            SpawnTextBubble();
        else if (convoIndex < convo.sentences.Length - 1)
        {
            convoIndex++;
			talkingUi.SetText(convo.sentences[convoIndex]);
		}
		else
            DespawnTextBubble();
        
            return true;
	}
	void SpawnTextBubble()
    {
        talking = true;
        talkingUi.SetTitle(convo.myName);
        talkingUi.SetText(convo.sentences[convoIndex]);
        StartCoroutine(talkingUi.SpawnTextBubble(TextBubbleType.sweet));
    }

    void DespawnTextBubble()
    {
        talking = false;
		StartCoroutine(talkingUi.DeSpawnTextBubble());
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
