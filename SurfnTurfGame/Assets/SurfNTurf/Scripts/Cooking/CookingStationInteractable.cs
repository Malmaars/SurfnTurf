using UnityEngine;

public class CookingStationInteractable : MonoBehaviour
{
    public CookingStationInteractable left;
    public CookingStationInteractable right;
    public CookingStationInteractable up;
    public CookingStationInteractable down;

    public enum CookingInteractableType { Grid, Button, Plate, Trash, Book }
    public CookingInteractableType interactionType;
}
