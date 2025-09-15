using System.Collections.Generic;
using UnityEngine;

public class PushableObject : MonoBehaviour
{
    public List<Vector2Int> gridPositions = new List<Vector2Int>();
    public float gridSize = 5f;
    public Vector3 center;
    private void Start()
    {
        center = transform.position;
    }

    public Vector3 GridToWorld(Vector2Int cell)
    {
        // if not in playmode, use transform position as center
        if (!Application.isPlaying)
            return transform.position + new Vector3(cell.x * gridSize, 0, cell.y * gridSize);

        return center + new Vector3(cell.x * gridSize, 0, cell.y * gridSize);
    }

    public void Push()
    {
        
    }
}



