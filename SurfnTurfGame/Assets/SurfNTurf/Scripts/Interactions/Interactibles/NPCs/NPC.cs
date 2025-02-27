using System;
using UnityEngine;

public class NPC : Interactible
{
    [SerializeField]
    Canvas inputPromptCanvas;

    public Conversation convo;
    int convoIndex;


	public override bool InteractWith()
	{
        //spawn a text bubble

        return true;
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
    public string[] sentences;
}
