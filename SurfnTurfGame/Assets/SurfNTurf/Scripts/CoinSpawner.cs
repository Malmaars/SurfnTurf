using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CoinSpawner : MonoBehaviour
{
    //tmp for coin counter
    [SerializeField] private int coinCounter = 0; // Current number of coins spawned
    [SerializeField] private TMP_Text TMPGui; // Maximum number of coins to spawn
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
    public IEnumerator SpawnCoins(Vector3 position)
    {
        while (currentCoinCount < maxCoins)
        {
            GameObject coin = Instantiate(coinPrefab, position, Quaternion.identity);
            coin.transform.SetParent(transform);
            currentCoinCount++;
            yield return new WaitForSeconds(spawnInterval);

        }
        currentCoinCount = 0;
    }

    public void AddCoinToCounter()
    {
        currentCoinCount++;
        TMPGui.text = currentCoinCount.ToString();
    }
}
