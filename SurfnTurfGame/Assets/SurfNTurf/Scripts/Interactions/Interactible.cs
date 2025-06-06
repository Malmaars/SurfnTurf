using UnityEngine;

public enum InteractionTypes
{
    talking,
    pickup
}

[RequireComponent(typeof(Collider))]
public class Interactible : MonoBehaviour
{
    [Header("Interaction Settings")]
    public bool interacting;
    public bool inputPromptOn;
    public GameObject inputPromptCanvas;
    public Vector3 basePromptOffset;
    public bool isActive = true;
    private void Start()
    {
        Initialize();
    }
    public virtual void Initialize()
    {
        RemoveHighlight();
    }
    private void OnEnable()
    {
        inputPromptCanvas.gameObject.SetActive(false);
    }

    //return true if you are interacting, return false if you are done interacting;
    public virtual bool InteractWith()
    {
        if (!isActive)
        {
            inputPromptCanvas.gameObject.SetActive(false);
            return false;
        }
        return false;
    }

    public virtual bool Exit()
    {
        return true;
    }

    public virtual void Highlight()
    {
        if (!isActive)
            return;

        if (inputPromptOn)
        {
            if (!inputPromptCanvas.gameObject.activeSelf)
                inputPromptCanvas.gameObject.SetActive(true);

            inputPromptCanvas.transform.position = transform.position + basePromptOffset + Vector3.up;

            inputPromptCanvas.transform.forward = (inputPromptCanvas.transform.position - Camera.main.transform.position).normalized;

        }

    }
    public virtual void Update()
    {
        GetComponent<Collider>().enabled = isActive;
    }


    public virtual void RemoveHighlight()
    {
        if (!isActive)
            return;
        inputPromptCanvas.gameObject.SetActive(false);
    }
}
