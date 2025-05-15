using UnityEngine;

public enum InteractionTypes
{
    talking,
    pickup
}

[RequireComponent(typeof(Collider))]
public class Interactible : MonoBehaviour
{
    public bool interacting;
    public bool inputPromptOn;
    public GameObject inputPromptCanvas;
    public Vector3 basePromptOffset;
    private void Start()
    {
        Initialize();
    }
    public virtual void Initialize()
    {
        RemoveHighlight();
    }

    //return true if you are interacting, return false if you are done interacting;
    public virtual bool InteractWith()
    {
        return false;
    }

    public virtual bool Exit()
    {
        return true;
    }

    public virtual void Highlight()
    {
        if (inputPromptOn)
        {
            if (!inputPromptCanvas.gameObject.activeSelf)
                inputPromptCanvas.gameObject.SetActive(true);

            inputPromptCanvas.transform.position = transform.position + basePromptOffset + Vector3.up;

            inputPromptCanvas.transform.forward = (inputPromptCanvas.transform.position - Camera.main.transform.position).normalized;

        }

    }


    public virtual void RemoveHighlight()
    {
        inputPromptCanvas.gameObject.SetActive(false);
    }
}
