using System.Collections.Generic;
using UnityEngine;

public class PushableObject : MonoBehaviour
{
    public List<Vector2Int> gridPositions = new List<Vector2Int>();
    public Vector2Int currentGridPosition;
    public float gridSize = 5f;
    public Vector3 center;
    public Vector3 targetPosition;
    private void Start()
    {
        center = transform.position;
        targetPosition = transform.position;
    }

    public Vector3 GridToWorld(Vector2Int cell)
    {
        // if not in playmode, use transform position as center
        if (!Application.isPlaying)
            return transform.position + new Vector3(cell.x * gridSize, 0, cell.y * gridSize);

        return center + new Vector3(cell.x * gridSize, 0, cell.y * gridSize);
    }
    private void Update()
    {
        //lerp to target location
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 5f);

    }

    public void Push()
    {
        //push the opistion in the direction the player is compared to the object
        Vector3 playerPosition = BlackBoard.playerBody.transform.position;
        Vector3 directionToPlayer = (playerPosition - transform.position).normalized;
        Vector2Int direction = Vector2Int.zero;
        if (Mathf.Abs(directionToPlayer.x) > Mathf.Abs(directionToPlayer.z))
        {
            if (directionToPlayer.x > 0)
                direction = Vector2Int.left;
            else
                direction = Vector2Int.right;
        }
        else
        {
            if (directionToPlayer.z > 0)
                direction = Vector2Int.down;
            else
                direction = Vector2Int.up;
        }
        if (gridPositions.Contains(currentGridPosition + direction))
        {
            //Debug.Log("Pushing object to " + (currentGridPosition + direction));
            currentGridPosition += direction;
            targetPosition = GridToWorld(currentGridPosition);
        }
    }
}



