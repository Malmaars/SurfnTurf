using UnityEngine;

public class TutorialRestrictionZone : MonoBehaviour
{
    public Restriction restriction;
    private MovementController playerMovement;
    private WaterMovementController playerWaterMovement;
    public Color gizmoColor = Color.red;
    public bool showTutorial = true;
    public bool dontShowTutorial = false;
    public bool enableCompass = false;
    public TutorialUIPart tutorialPart;
    private void Start()
    {
        playerMovement = BlackBoard.playerBody.GetComponent<MovementController>();
        playerWaterMovement = BlackBoard.playerBody.GetComponent<WaterMovementController>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            restriction.ApplyRestriction(restriction, playerMovement, playerWaterMovement);
            // Show the tutorial text
            if (!dontShowTutorial)
            {
                UIManager.instance.tutorialPart = tutorialPart;
                UIManager.instance.ShowTutorial(showTutorial);
            }

            if (enableCompass)
                BlackBoard.compass.gameObject.SetActive(true);
            else
				BlackBoard.compass.gameObject.SetActive(false);
		}

	}

    private void OnDrawGizmos()
    {
        //draw the trigger box as a transparent box
        Gizmos.color = gizmoColor;
        Gizmos.DrawCube(GetComponent<BoxCollider>().bounds.center, GetComponent<BoxCollider>().bounds.size);
    }
}
