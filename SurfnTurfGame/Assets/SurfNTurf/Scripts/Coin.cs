using System;
using System.Collections;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using Random = UnityEngine.Random;
using FMODUnity;


public class Coin : MonoBehaviour
{
    public bool initialImpulse = true;
    private bool isCollected = false;
    [SerializeField] private AnimationCurve returnCurve;
    [SerializeField] private AnimationCurve heightCurve;
    [SerializeField] private AnimationCurve speedCurve;
    private float spinSpeed = 100f;
    public float lifetime = 0.5f;
    private Vector3 startPos;
    //private bool sleep = false;


    IEnumerator Start()
    {
        yield return new WaitUntil(() => CloudSaveSystem.Instance != null && CloudSaveSystem.Instance.IsInitialized);
        startPos = transform.position;
        CoinSpawner.instance.coins.Add(this);
        if (!initialImpulse)
        {
            if (CloudSaveSystem.Instance.data.coinsCollected.Contains(GetKey()))
            {
                CoinSpawner.instance.coins.Remove(this);
                CoinSpawner.instance.SetGraphicsBuffer();
                Destroy(gameObject);
                yield break;
            }
        }
        CoinSpawner.instance.SetGraphicsBuffer();
        if (initialImpulse)
        {
            float angle = Random.Range(0f, Mathf.PI * 2); // Random angle in radians
            float radius = Random.Range(0f, 3f); // Random radius within the circle
            Vector3 random = new Vector3(Mathf.Cos(angle) * radius, 0.5f, Mathf.Sin(angle) * radius);
            StartCoroutine(SpawnSequance(random));
        }
    }


    void Update()
    {
        //rotate this object around y axis
        transform.Rotate(0, spinSpeed * Time.deltaTime, 0);

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(StartCollection(other.transform));
        }
    }
    public IEnumerator SpawnSequance(Vector3 random)
    {
        float elapsedTime = 0f;
        Vector3 startPos = transform.position;
        Vector3 targetPos = transform.position + random * 2f;
        float time = random.magnitude / 4f;
        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, targetPos, speedCurve.Evaluate(elapsedTime / time));
            transform.position = new Vector3(transform.position.x, transform.position.y + (heightCurve.Evaluate(elapsedTime / time) * random.magnitude), transform.position.z);
            if (isCollected) yield break; //if already collected, exit the coroutine
            yield return null;
        }
    }
    public IEnumerator StartCollection(Transform player)
    {
        if (isCollected) yield break; //if already collected, exit the coroutine
        isCollected = true;
        spinSpeed = 1000f;
        //apply a force in the oppisite direction of the player
        float elapsedTime = 0f;
        Vector3 targetPos = transform.position + (transform.position - player.position + transform.up).normalized * 2f;
        Vector3 startPos = transform.position;
        while (elapsedTime < 0.1f)
        {
            elapsedTime += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime / 0.1f);
            yield return null;
        }
        //lerp the coin to the player position
        float elapsedTime2 = 0f;
        while (elapsedTime2 < 0.5f)
        {
            elapsedTime2 += Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, player.position, returnCurve.Evaluate(elapsedTime2 / 0.5f));
            yield return null;
        }
        PlayerVFX.instance.pickUpCoin.SendEvent("OnPlay");
        CoinSpawner.instance.AddCoinToCounter();
        CoinSpawner.instance.coins.Remove(this);
        CoinSpawner.instance.SetGraphicsBuffer();
        if (!initialImpulse)
        {
            CloudSaveSystem.Instance.data.coinsCollected.Add(GetKey());
        }
        gameObject.GetComponent<StudioEventEmitter>().Play();
        Destroy(gameObject);
    }

    private String GetKey()
    {
        return nameof(isCollected) + this + startPos;
    }
}
