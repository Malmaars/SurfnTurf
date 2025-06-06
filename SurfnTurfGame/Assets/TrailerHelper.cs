using UnityEngine;
using UnityEngine.Events;

public class TrailerHelper : MonoBehaviour
{
    // Update is called once per frame
    public KeyCode keycode;
    public UnityEvent action;
    void Update()
    {
        if (Input.GetKeyDown(keycode))
        {
            action.Invoke();
            //GetComponent<Animator>().SetTrigger("Eat");
        }
    }
}
