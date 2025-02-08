using UnityEngine;
using TMPro;

public class CollectionManager : MonoBehaviour
{
    public int collectionCount;

    public Animator animator;
    public TMP_Text text;

    public void UpdateScore(int amount)
    {
        collectionCount += amount;

        animator.Play("CandyCollected", -1, 0f);

        text.SetText(collectionCount.ToString());
    }
}
