using System.Collections;
using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private float spawnInterval = 0.1f; // Time in seconds between coin spawns
    [SerializeField] private int maxCoins = 10; // Maximum number of coins to spawn
    private int currentCoinCount = 0; // Current number of coins spawned
    //singlton
    public static CoinSpawner instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public IEnumerator SpawnCoins(Transform t)
    {
        while (currentCoinCount < maxCoins)
        {
            GameObject coin = Instantiate(coinPrefab, t.position+t.up, Quaternion.identity);
            coin.transform.SetParent(transform);
            currentCoinCount++;
            yield return new WaitForSeconds(spawnInterval);

        }
        currentCoinCount = 0;
    }
}
