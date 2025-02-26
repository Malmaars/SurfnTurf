using UnityEngine;

public class NPC : Interactible
{
    [SerializeField]
    Canvas inputPromptCanvas;
    
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
