using UnityEngine;
using UnityEngine.Events;

public class CollsionEvent : MonoBehaviour
{
    public UnityEvent OnHit;

    void OnCollisionEnter(Collision collision)
    {
        OnHit.Invoke();
    }
}
