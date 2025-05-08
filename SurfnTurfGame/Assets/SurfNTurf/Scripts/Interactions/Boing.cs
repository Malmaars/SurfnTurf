using System;
using System.Collections;
using UnityEngine;

public class Boing : MonoBehaviour
{
    Animator animator;
    public int coinCount = 5;

    IEnumerator Start()
    {
        yield return new WaitUntil(() => CloudSaveSystem.Instance != null && CloudSaveSystem.Instance.IsInitialized);
        if (!CloudSaveSystem.Instance.data.boings.Contains(GetKey()))
        {
            CloudSaveSystem.Instance.data.boings.Add(GetKey());
            CloudSaveSystem.Instance.data.boingsCount.Add(coinCount);
        }
        animator = GetComponent<Animator>();
    }

    private string GetKey()
    {
        return gameObject.name + transform.position.ToString() + "Boing";
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.relativeVelocity.magnitude < 30) return; 
            animator.SetTrigger("Boing");
            int index = CloudSaveSystem.Instance.data.boings.IndexOf(GetKey());
            if (CloudSaveSystem.Instance.data.boingsCount[index] > 0)
            {
                Vector3 newPosition = transform.position + new Vector3(0, 4, 0);
                CoinSpawner.instance.StartCoroutine(CoinSpawner.instance.SpawnCoins(newPosition, 1,true));
                CloudSaveSystem.Instance.data.boingsCount[index]--;
            }
        }
    }
}

