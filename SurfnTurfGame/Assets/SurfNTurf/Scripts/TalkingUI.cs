using System.Collections;
using UnityEngine;
using TMPro;

public enum TextBubbleType
{
	sweet,
	sour,
	spicy
}
public class TalkingUI : MonoBehaviour
{
	RectTransform activeBubble;
	public TextMeshProUGUI title, talkText;

	public RectTransform[] bubblePresets;

	public float scaleSpeed, moveSpeed, posSpringAmount, scaleSpringAmount, talkingWidth, endingWith, heightOffset, heightDifference;
	float newXPos, newYPos, newWidth, targetWidth;
	Vector2 targetPos;
	public Vector2 talkingPos, endingPos;

	bool activating, ending;

	public void SetTitle(string _newTitle)
	{
		title.text = _newTitle;
	}

	public void SetText(string newText)
	{
		talkText.text = newText;
	}

	public void Initialize()
	{
		foreach(RectTransform rt in bubblePresets)
		{
			rt.anchoredPosition = endingPos;
			rt.localScale = Vector3.zero;
		}
	}

	public void SpawnTextBubble(TextBubbleType textbubbleType)
	{
		if (activating)
			return;

		StartCoroutine(SpawnTextBubbleRoutine(textbubbleType));
	}

	public void DespawnTextBubble()
	{
		if (ending)
			return;
		
		StartCoroutine(DeSpawnTextBubbleRoutine());
	}
	public IEnumerator SpawnTextBubbleRoutine(TextBubbleType textbubbleType)
	{
		RectTransform toSpawn = null;
		ending = false;
		switch (textbubbleType)
		{
			//spawn the sweet bubble
			case TextBubbleType.sweet:
				toSpawn = bubblePresets[0];
				break;

			case TextBubbleType.sour:
				break;

			case TextBubbleType.spicy:
				break;
		}

		title = toSpawn.GetChild(0).GetComponent<TextMeshProUGUI>();
		talkText = toSpawn.GetChild(1).GetComponent<TextMeshProUGUI>();

		if (activeBubble != null && toSpawn != activeBubble)
			StartCoroutine(DeSpawnTextBubbleRoutine());

		while (activeBubble != null && toSpawn != activeBubble)
			yield return null;

		activeBubble = toSpawn;

		activating = true;
		ending = false;

		targetPos = talkingPos;
		targetWidth = talkingWidth;
		//animate the bubble
		while (activating)
		{
			float _scaleSpeed = scaleSpeed;
            float _moveSpeed = moveSpeed;

			Debug.Log(activeBubble);
            newXPos = Mathf.Lerp(newXPos, (targetPos.x - activeBubble.anchoredPosition.x) * posSpringAmount, Time.deltaTime * _moveSpeed);
            newYPos = Mathf.Lerp(newYPos, (targetPos.y - activeBubble.anchoredPosition.y) * posSpringAmount, Time.deltaTime * _moveSpeed);

            activeBubble.anchoredPosition += new Vector2(newXPos, newYPos);

            newWidth = Mathf.Lerp(newWidth, (targetWidth - activeBubble.localScale.x) * scaleSpringAmount, Time.deltaTime * _scaleSpeed);
            activeBubble.localScale += new Vector3(newWidth, newWidth, newWidth);

			activeBubble.sizeDelta = new Vector2(activeBubble.sizeDelta.x, heightOffset + (heightDifference * talkText.textInfo.lineCount));

            if (activeBubble.localScale.x < 0)
                activeBubble.localScale *= -1;

			if (activeBubble.anchoredPosition.x == newXPos && activeBubble.anchoredPosition.y == newYPos && activeBubble.localScale.x == targetWidth)
				activating = false;

			yield return new WaitForEndOfFrame();
		}
	}

	public IEnumerator DeSpawnTextBubbleRoutine()
	{
		activating = false;
		ending = true;

		targetPos = endingPos;
		targetWidth = endingWith;
		while (ending)
		{
			float _scaleSpeed = scaleSpeed;
			float _moveSpeed = moveSpeed;

			newXPos = Mathf.Lerp(newXPos, (targetPos.x - activeBubble.anchoredPosition.x) * posSpringAmount, Time.deltaTime * _moveSpeed);
			newYPos = Mathf.Lerp(newYPos, (targetPos.y - activeBubble.anchoredPosition.y) * posSpringAmount, Time.deltaTime * _moveSpeed);

			activeBubble.anchoredPosition += new Vector2(newXPos, newYPos);

			newWidth = Mathf.Lerp(newWidth, (targetWidth - activeBubble.localScale.x) * scaleSpringAmount, Time.deltaTime * _scaleSpeed);
			activeBubble.localScale += new Vector3(newWidth, newWidth, newWidth);

			if (activeBubble.localScale.x < 0)
				activeBubble.localScale *= -1;

			if (activeBubble.localScale.x < 0.001f)
			{
				ending = false;
				activeBubble = null;
			}
			yield return new WaitForEndOfFrame();
		}

		yield return null;
	}
}
