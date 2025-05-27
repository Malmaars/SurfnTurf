using UnityEngine;
using UnityEngine.Events;

public class EventTrigger : MonoBehaviour
{
    public string colliderTagName;
    public UnityEvent OnEnter;
    public UnityEvent OnExit;


    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == colliderTagName)
        {
            OnEnter.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.tag == colliderTagName)
        {
            OnExit.Invoke();
        }
    }
}
