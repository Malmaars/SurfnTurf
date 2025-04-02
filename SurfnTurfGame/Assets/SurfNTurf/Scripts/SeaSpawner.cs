using UnityEngine;

public class SeaSpawner : MonoBehaviour
{
    public GameObject chunkPrefab; // The prefab for the chunk
    public Vector3 centerPoint = Vector3.zero; // The center point where chunks will spawn around
    public float chunkSize = 10f; // The size of each chunk
    public int gridSize = 5; // The grid size (e.g., how many chunks outwards from center)
    public float spawnHeight = 0f; // The height at which chunks will be spawned

    void Awake()
    {
        SpawnChunks();
    }

    void SpawnChunks()
    {
        // Loop through a grid of positions around the center point
        for (int x = -gridSize; x <= gridSize; x++)
        {
            for (int z = -gridSize; z <= gridSize; z++)
            {
                Vector3 spawnPosition = centerPoint + new Vector3(x * chunkSize, spawnHeight, z * chunkSize);
                Instantiate(chunkPrefab, spawnPosition, Quaternion.identity, transform);
            }
        }
    }
}
