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
	public Vector2 spawnedPosition, despawnedPosition;
	RectTransform activeBubble;
	public TextMeshProUGUI title, talkText;

	public RectTransform[] bubblePresets;

	public float speed;

	public void SetTitle(string _newTitle)
	{
		title.text = _newTitle;
	}
	
	public void SetText(string newText)
	{
		talkText.text = newText;
	}
	public IEnumerator SpawnTextBubble(TextBubbleType textbubbleType)
	{
		RectTransform toSpawn = null;
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

		if (activeBubble != null && toSpawn != activeBubble)
			StartCoroutine(DeSpawnTextBubble());

		while (activeBubble != null && toSpawn != activeBubble)
			yield return null;

		activeBubble = toSpawn;
		while (activeBubble.anchoredPosition != spawnedPosition)
			activeBubble.anchoredPosition = Vector2.MoveTowards(activeBubble.anchoredPosition, spawnedPosition, speed * Time.deltaTime);

	}

	public IEnumerator DeSpawnTextBubble()
	{
		while (activeBubble != null && activeBubble.anchoredPosition != despawnedPosition)
		{
			activeBubble.anchoredPosition = Vector2.MoveTowards(activeBubble.anchoredPosition, despawnedPosition, speed * Time.deltaTime);
			yield return new WaitForEndOfFrame();
		}
			activeBubble = null;

		yield return null;
	}
}
