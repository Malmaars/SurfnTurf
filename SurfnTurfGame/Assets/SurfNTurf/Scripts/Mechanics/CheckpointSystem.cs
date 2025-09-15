using System.Collections.Generic;
using UnityEngine;

public class CheckpointSystem : MonoBehaviour
{
    [SerializeField] private List<Transform> checkpoints;
    private int currentCheckpointIndex = 0;
    public float checkpointRadius = 5f;
    private Transform player;
    private LineRenderer lineRenderer;

    private void Start()
    {
        //turn off all checkpoint visuals at start
        foreach (var checkpoint in checkpoints)
        {
            checkpoint.gameObject.SetActive(false);
        }
        checkpoints[currentCheckpointIndex].gameObject.SetActive(true);
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        if (currentCheckpointIndex < checkpoints.Count)
        {
            Transform currentCheckpoint = checkpoints[currentCheckpointIndex];
            if (Vector3.Distance(BlackBoard.playerBody.position, currentCheckpoint.position) <= checkpointRadius)
            {
                currentCheckpointIndex++;
                checkpoints[currentCheckpointIndex - 1].gameObject.SetActive(false);
                if (currentCheckpointIndex < checkpoints.Count)
                {
                    checkpoints[currentCheckpointIndex].gameObject.SetActive(true);
                }
                else
                {
                    Debug.Log("All checkpoints reached!");
                }
                Debug.Log("Checkpoint reached: " + currentCheckpointIndex);
            }
        }
        //use line rendered to draw line between player and next checkpoint
        if (lineRenderer != null && currentCheckpointIndex < checkpoints.Count)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, BlackBoard.playerBody.position);
            lineRenderer.SetPosition(1, checkpoints[currentCheckpointIndex].position);
        }
        else if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        foreach (var checkpoint in checkpoints)
        {
            if (checkpoint != null)
            {
                Gizmos.DrawWireSphere(checkpoint.position, checkpointRadius);
            }
        }

        //draw lines between checkpoints
        Gizmos.color = Color.green;
        for (int i = 0; i < checkpoints.Count - 1; i++)
        {
            if (checkpoints[i] != null && checkpoints[i + 1] != null)
            {
                Gizmos.DrawLine(checkpoints[i].position, checkpoints[i + 1].position);
            }
        }
    }
}
    
