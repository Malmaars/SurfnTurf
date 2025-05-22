using UnityEngine;
using System.Collections;

[System.Serializable]
public class Spoon : MonoBehaviour
{
    public string spoonName;
    public int durability;
    public Coroutine coroutine;
    public AnimationCurve animationCurve;

    public bool Use()
    {
        if(durability > 0)
        {
            durability--;
        }

        return durability <= 0;
    }

    public void SetPosition(Vector3 newPosition)
    {
        Vector3 oldPosition = transform.localPosition;
        if(coroutine == null)
        {
            coroutine = StartCoroutine(MoveSpoon(oldPosition, newPosition));
        }
        else
        {
            StopCoroutine(coroutine);
            coroutine = StartCoroutine(MoveSpoon(oldPosition, newPosition));
        }
    }

    IEnumerator MoveSpoon(Vector3 oldPos, Vector3 newPos)
    {
        float totalTime = 1f;
        float timer = 0f;
        while (timer < totalTime)
        {
            Vector3 betweenPosition = Vector3.Lerp(oldPos, newPos, animationCurve.Evaluate(timer / totalTime));
            transform.localPosition = betweenPosition;
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }
}