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
    //singlton
    public static CoinSpawner instance;
    [SerializeField] private VisualEffect vfxGraph;
    public List<Coin> coins = new List<Coin>();
    private GraphicsBuffer buffer;
    private List<GraphicsBuffer> buffers = new List<GraphicsBuffer>();
    Vector3[] coinPositions;
    [SerializeField] private AnimationCurve coinCurve;
    [SerializeField] private AnimationCurve coinCounterCurve;
    [SerializeField] private Transform hudCoin;
    bool isAnimating = false;
    public GameObject player;

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
    IEnumerator Start()
    {
        yield return new WaitUntil(() => CloudSaveSystem.Instance != null && CloudSaveSystem.Instance.IsInitialized);

        hudCoin = UIManager.instance.hudCoin.transform;
        TMPGui = UIManager.instance.coinCounter;
        TMPGui.text = CloudSaveSystem.Instance.data.coinsCollectedCount.ToString();
        SetGraphicsBuffer(); // Set the buffer every 0.1 seconds
        player = BlackBoard.playerBody.gameObject;
    }
    void Update()
    {
        if (coins.Count == 0) return;

        buffer.SetData(coinPositions);
        vfxGraph.SetGraphicsBuffer("CoinPositionBuffer", buffer);
        vfxGraph.SetInt("CoinCount", coinPositions.Length);
    }
    public void SetGraphicsBuffer()
    {
        if (coins.Count == 0) return;
        coinPositions = GetCoinPositions(); // however you get them
        foreach (GraphicsBuffer buffer in buffers)
        {
            buffer.Dispose();
        }
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
    public IEnumerator SpawnCoins(Vector3 position, int coinCount = 100, bool collectOnSpawn = false)
    {
        int localCoinCount = 0; // Reset the current coin count for each spawn
        float localSpawnInterval = spawnInterval;
        while (localCoinCount < coinCount)
        {
            GameObject coin = Instantiate(coinPrefab, position, Quaternion.identity);
            coin.transform.SetParent(transform);
            Coin coinComponent = coin.GetComponent<Coin>();
            coinComponent.lifetime = coinComponent.lifetime * spawnIntervalModifier;
            if (collectOnSpawn)
            {
                coinComponent.StartCoroutine(coinComponent.StartCollection(player.transform));
            }
            localCoinCount++;
            localSpawnInterval *= spawnIntervalModifier;
            yield return new WaitForSeconds(localSpawnInterval);

        }
    }

    public void AddCoinToCounter(int coinCount = 1)
    {
        CloudSaveSystem.Instance.data.coinsCollectedCount += coinCount;
        TMPGui.text = CloudSaveSystem.Instance.data.coinsCollectedCount.ToString();
        StartCoroutine(UpdateCoinCounter());
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

    private IEnumerator UpdateCoinCounter()
    {
        if (isAnimating) yield break; // If already animating, exit the coroutine
        isAnimating = true;
        float elapsedTime = 0f;
        float duration = 0.25f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime / 1f; // Duration of the animation
            TMPGui.transform.localEulerAngles = new Vector3(0f, 0f, coinCounterCurve.Evaluate(elapsedTime / duration));
            hudCoin.localEulerAngles = new Vector3(0f, coinCurve.Evaluate(elapsedTime / duration), 0f);
            yield return null;
        }
        isAnimating = false;
    }
}
