using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class ChallengeStamp : MonoBehaviour
{
    public Animator animator;
    public Transform container;
    public TextMeshProUGUI text;
    public Image image;
    public Color passedColor;
    public Color failedColor;
    public float maxRotation;

    public void PlaceStamp(bool passed)
    {
        gameObject.SetActive(true);
        container.gameObject.SetActive(true);
        if (passed)
        {
            text.text = "PASSED";
            text.color = passedColor;
            image.color = passedColor;
        }
        else
        {
            text.text = "FAILED";
            text.color = failedColor;
            image.color = failedColor;
        }
        animator.Play("Stamp");
        StartCoroutine(AnimateStamp());
    }

    public void ResetStamp()
    {
        gameObject.SetActive(false);
        container.gameObject.SetActive(false);
    }

    IEnumerator AnimateStamp()
    {
        float timer = 0;
        Color color = image.color;
        float startRotation = Random.Range(-maxRotation, maxRotation);
        float endRotation = Random.Range(0, maxRotation);
        if (startRotation >= 0f)
            endRotation *= -1;

        while (timer < 0.25f)
        {
            float value = timer / 0.25f;
            Vector3 rotation = Vector3.Lerp(new Vector3(0, 0, startRotation), new Vector3(0, 0, endRotation), value);
            container.eulerAngles = rotation;
            timer += Time.deltaTime;
            color.a = value;
            image.color = color;
        }

        color.a = 1f;
        image.color = color;

        yield return null;
    }
}
