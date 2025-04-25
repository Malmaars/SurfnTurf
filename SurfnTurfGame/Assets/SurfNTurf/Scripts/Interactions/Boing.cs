using UnityEngine;

public class Boing : MonoBehaviour
{
    Animator animator;
    public int coinCount = 5;
    [SerializeField] private SavedProperty<int> isEmptyProperty;
    void Awake()
    {
        isEmptyProperty = new SavedProperty<int>(nameof(isEmptyProperty) + this + transform.position, coinCount);
    }
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.relativeVelocity.magnitude < 30) return; 
            animator.SetTrigger("Boing");
            if (isEmptyProperty.Value > 0)
            {
                Vector3 newPosition = transform.position + new Vector3(0, 4, 0);
                CoinSpawner.instance.StartCoroutine(CoinSpawner.instance.SpawnCoins(newPosition, 1,true));
                isEmptyProperty.Value--;
            }
        }
    }
}

