using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class PlatformingButton : MonoBehaviour
{
    public enum ButtonType { OneTime, Toggle, Holding }
    public ButtonType buttonType;
    public bool active;

    public UnityEvent onActivate;
    public UnityEvent onDeactivate;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.transform.tag);
        if (collision.transform.tag != "Player")
            return;
        Debug.Log(collision.transform.tag);

        switch (buttonType)
        {
            case ButtonType.OneTime:
                active = true;
                onActivate.Invoke();
                animator.Play("PlatformingButtonDown");
                break;
            case ButtonType.Toggle:
                active = !active;
                if (active)
                {
                    onActivate.Invoke();
                    animator.Play("PlatformingButtonDown");
                }
                else
                {
                    onDeactivate.Invoke();
                    animator.Play("PlatformingButtonUp");
                }
                break;
            case ButtonType.Holding:
                active = true;
                onActivate.Invoke();
                animator.Play("PlatformingButtonDown");
                break;
            default:
                break;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.transform.tag != "Player")
            return;

        if (buttonType == ButtonType.Holding)
        {
            active = false;
            onDeactivate.Invoke();
            animator.Play("PlatformingButtonUp");
        }
    }

    public void ButtonReset()
    {
        onDeactivate.Invoke();
        animator.Play("PlatformingButtonUp");
    }
}
