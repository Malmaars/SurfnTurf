using UnityEngine;

public class Coin : MonoBehaviour
{
    public bool initialImpulse = true;
    private void Start()
    {
        if (initialImpulse)
        {
            //give impuls to random up direction like a fountain
            Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), Random.Range(0.5f, 1f), Random.Range(-1f, 1f)).normalized;
            float randomForce = Random.Range(5f, 10f);
            GetComponent<Rigidbody>().AddForce(randomDirection * randomForce, ForceMode.Impulse);
        }
    }
    void Update()
    {
        //rotate this object around y axis
        transform.Rotate(0, 100 * Time.deltaTime, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerVFX.instance.pickUpCoin.SendEvent("OnPlay");
            Destroy(gameObject);
        }
    }

}
