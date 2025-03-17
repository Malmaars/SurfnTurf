using UnityEngine;

public class MoveWithWorld : MonoBehaviour
{
    private Vector3 originalPosition;
    void Start()
    {
        originalPosition = transform.position;
    }

    void Update()
    {
        float distance = Vector2.Distance(Camera.main.transform.position, originalPosition);

        distance = Mathf.Pow(distance, 2);
        float multiplier = -(10 * 1E-05f);
        distance = multiplier * distance;

        transform.position = new Vector3(transform.position.x, originalPosition.y + distance, transform.position.z);
    }
}
