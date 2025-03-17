using UnityEngine;

public enum InteractionTypes
{
    talking,
    pickup
}

[RequireComponent (typeof(Collider))]
public class Interactible : MonoBehaviour
{
    public bool interacting;
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

    }


    public virtual void RemoveHighlight()
    {

    }
}
