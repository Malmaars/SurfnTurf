using System;
using System.Collections;
using UnityEngine;

public class GridCursor : MonoBehaviour
{
    public bool visible;
    public GameObject visual;

    public Vector3 worldPosition;
    public Quaternion worldRotation;
    public bool canMove;
    public float moveTime;
    public float moveSlowDelay;
    public float moveFastDelay;

    private void Awake()
    {
        canMove = true;
    }

    public void Visible(bool _visible)
    {
        visible = _visible;
        visual.SetActive(_visible);
    }

    public void SetPosition(Transform newTransform, bool moveFast)
    {
        if (!canMove)
            return;

        canMove = false;
        worldPosition = newTransform.position;
        worldRotation = newTransform.rotation;
        StartCoroutine(MoveCursor(moveFast));
    }

    IEnumerator MoveCursor(bool moveFast)
    {
        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        while (elapsedTime < moveTime)
        {
            float t = elapsedTime / moveTime;
            transform.position = Vector3.Lerp(startPosition, worldPosition, t);
            transform.rotation = Quaternion.Lerp(startRotation, worldRotation, t);
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        transform.position = worldPosition;
        transform.rotation = worldRotation;

        elapsedTime = 0f;
        if (moveFast)
        {
            while (elapsedTime < moveFastDelay && InputDistributor.playerInputActions.Cooking.DirectionalInput.ReadValue<Vector2>() != Vector2.zero)
            {
                elapsedTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            while (elapsedTime < moveSlowDelay && InputDistributor.playerInputActions.Cooking.DirectionalInput.ReadValue<Vector2>() != Vector2.zero)
            {
                elapsedTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }
        

        canMove = true;
    }
}
