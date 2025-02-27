using UnityEngine;

[RequireComponent (typeof(Collider))]
public class Interactible : MonoBehaviour
{
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

    public virtual void Highlight()
    {

    }


    public virtual void RemoveHighlight()
    {

    }
}
