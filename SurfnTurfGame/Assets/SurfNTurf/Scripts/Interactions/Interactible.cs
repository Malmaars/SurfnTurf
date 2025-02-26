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
    public virtual void InteractWith()
    {

    }

    public virtual void Highlight()
    {

    }


    public virtual void RemoveHighlight()
    {

    }
}
