using System.Collections;
using UnityEngine;

public class Coin : MonoBehaviour
{
    public bool initialImpulse = true;
    Rigidbody rb;
    private bool isCollected = false;
    [SerializeField] private AnimationCurve curve;
    private float spinSpeed = 100f;
    public float lifetime = 0.5f; 
    private void Start()
    {
        CoinSpawner.instance.coins.Add(this);
        CoinSpawner.instance.SetGraphicsBuffer();
        if (initialImpulse)
        {
            //give impuls to random up direction like a fountain
            Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), Random.Range(0.5f, 1f), Random.Range(-1f, 1f)).normalized;
            float randomForce = Random.Range(5f, 10f);
            rb = GetComponent<Rigidbody>();
            rb.AddForce(randomDirection * randomForce, ForceMode.Impulse);
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

    private IEnumerator StartCollection(Transform player)
    {
        if (isCollected) yield break; //if already collected, exit the coroutine
        isCollected = true;
        spinSpeed = 1000f;
        //apply a force in the oppisite direction of the player
        rb = GetComponent<Rigidbody>();
        rb.AddForce((transform.position - player.position + transform.up).normalized * 50f, ForceMode.Impulse);
        rb.linearDamping = 15f;
        yield return new WaitForSeconds(lifetime/2f);
        rb.linearVelocity = Vector3.zero;
        //lerp the coin to the player position
        float elapsedTime = 0f;
        Vector3 startPos = transform.position;
        while (elapsedTime < lifetime/2f)
        {
            elapsedTime += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, player.position, curve.Evaluate(elapsedTime / (lifetime/2f)));
            yield return null;
        }
        PlayerVFX.instance.pickUpCoin.SendEvent("OnPlay");
        CoinSpawner.instance.AddCoinToCounter();
        CoinSpawner.instance.coins.Remove(this);
        Destroy(gameObject);
    }
}
