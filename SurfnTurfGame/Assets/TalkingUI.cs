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

	public float scaleSpeed, moveSpeed, posSpringAmount, scaleSpringAmount, targetWidth;
	float newXPos, newYPos, newWidth;
	public Vector2 targetPos;

	public void SetTitle(string _newTitle)
	{
		title.text = _newTitle;
	}
	
	public void SetText(string newText)
	{
		talkText.text = newText;
	}

	public void SpawnTextBubble(TextBubbleType textbubbleType)
	{
		StartCoroutine(SpawnTextBubbleRoutine(textbubbleType));
	}

	public void DespawnTextBubble()
	{
		StartCoroutine(DeSpawnTextBubbleRoutine());
	}
	public IEnumerator SpawnTextBubbleRoutine(TextBubbleType textbubbleType)
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
			StartCoroutine(DeSpawnTextBubbleRoutine());

		while (activeBubble != null && toSpawn != activeBubble)
			yield return null;

		activeBubble = toSpawn;

		//animate the bubble
		//while ()
		//{
            float _scaleSpeed = scaleSpeed;
            float _moveSpeed = moveSpeed;


            newXPos = Mathf.Lerp(newXPos, (targetPos.x - activeBubble.anchoredPosition.x) * posSpringAmount, Time.deltaTime * _moveSpeed);
            newYPos = Mathf.Lerp(newYPos, (targetPos.y - activeBubble.anchoredPosition.y) * posSpringAmount, Time.deltaTime * _moveSpeed);

            activeBubble.anchoredPosition += new Vector2(newXPos, newYPos);

            newWidth = Mathf.Lerp(newWidth, (targetWidth - activeBubble.localScale.x) * scaleSpringAmount, Time.deltaTime * _scaleSpeed);
            activeBubble.localScale += new Vector3(newWidth, newWidth, newWidth);

            if (activeBubble.localScale.x < 0)
                activeBubble.localScale *= -1;
        //}
	}

	public IEnumerator DeSpawnTextBubbleRoutine()
	{
		while (activeBubble != null && activeBubble.anchoredPosition != despawnedPosition)
		{
			//animate the bubble;
			yield return new WaitForEndOfFrame();
		}
			activeBubble = null;

		yield return null;
	}
}
