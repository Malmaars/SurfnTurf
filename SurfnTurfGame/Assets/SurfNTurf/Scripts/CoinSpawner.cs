using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

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
    [SerializeField] private VisualEffect vfxGraph;
    public List<Coin> coins = new List<Coin>();
    private GraphicsBuffer buffer;
    private List<GraphicsBuffer> buffers = new List<GraphicsBuffer>();
    Vector3[] coinPositions;

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
    private void Start()
    {
        SetGraphicsBuffer(); // Set the buffer every 0.1 seconds
    }
    void Update()
    {
        buffer.SetData(coinPositions);
        vfxGraph.SetGraphicsBuffer("CoinPositionBuffer", buffer);
        vfxGraph.SetInt("CoinCount", coinPositions.Length);   
    }
    public void SetGraphicsBuffer()
    {
        if (coins.Count == 0) return;
        coinPositions = GetCoinPositions(); // however you get them
        buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, coinPositions.Length, sizeof(float) * 3);
        buffers.Add(buffer);
    }
    private void OnDestroy()
    {
        foreach (GraphicsBuffer buffer in buffers)
        {
            buffer.Dispose();
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
            Coin coinComponent = coin.GetComponent<Coin>();
            coinComponent.lifetime = coinComponent.lifetime * spawnIntervalModifier;
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
    private Vector3[] GetCoinPositions()
    {
        Vector3[] coinPositions = new Vector3[coins.Count];
        for (int i = 0; i < coinPositions.Length; i++)
        {
            coinPositions[i] = coins[i].transform.position; 
        }
        return coinPositions;
    }
}
