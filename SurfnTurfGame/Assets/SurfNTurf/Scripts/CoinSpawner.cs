using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CoinSpawner : MonoBehaviour
{
    //tmp for coin counter
    [SerializeField] private TMP_Text TMPGui; // Maximum number of coins to spawn
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private float spawnInterval = 0.1f; // Time in seconds between coin spawns
    [SerializeField] private float spawnIntervalModifier = 0.95f;
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
    public IEnumerator SpawnCoins(Vector3 position)
    {
        int localCoinCount = 0; // Reset the current coin count for each spawn
        float localSpawnInterval = spawnInterval;
        while (localCoinCount < maxCoins)
        {
            GameObject coin = Instantiate(coinPrefab, position, Quaternion.identity);
            coin.transform.SetParent(transform);
            localCoinCount++;
            localSpawnInterval *= spawnIntervalModifier;
            yield return new WaitForSeconds(localSpawnInterval);

        }
    }

    public void AddCoinToCounter()
    {
        currentCoinCount++;
        TMPGui.text = currentCoinCount.ToString();
    }
}
