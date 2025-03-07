using UnityEngine;
using TMPro;
using UnityEngine.VFX;

public class CollectionManager : MonoBehaviour
{
    public int collectionCount;

    public Animator animator;
    public TMP_Text text;
    public VisualEffect vfx;

    public void UpdateScore(int amount)
    {
        collectionCount += amount;

        animator.Play("CandyCollected", -1, 0f);

        text.SetText(collectionCount.ToString());

        for(int i = 0; i < amount; i++)
        {
            vfx.SendEvent("OnPlay");
        }
    }
}
