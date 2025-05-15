using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using FMODUnity;

[RequireComponent(typeof(Selectable))]
public class FMODUISoundHandeler : MonoBehaviour, IPointerEnterHandler, ISelectHandler, IPointerClickHandler, ISubmitHandler
{
    public EventReference HighlightEventReference;
    public EventReference PressedEventReference;

    public void OnPointerEnter(PointerEventData eventData)
    {
        RuntimeManager.PlayOneShot(HighlightEventReference, transform.position);
    }

    public void OnSelect(BaseEventData eventData)
    {
        RuntimeManager.PlayOneShot(HighlightEventReference, transform.position);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        RuntimeManager.PlayOneShot(PressedEventReference, transform.position);
    }

    public void OnSubmit(BaseEventData eventData)
    {
        RuntimeManager.PlayOneShot(PressedEventReference, transform.position);
    }
}