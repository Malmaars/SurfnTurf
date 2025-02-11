using UnityEngine;

public class MoveWithPlayer : MonoBehaviour
{
    public Transform player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float snapValue = 10.0f;
        float snapX = Mathf.Floor(player.position.x / snapValue) * snapValue;
        float snapZ = Mathf.Floor(player.position.z / snapValue) * snapValue;
        transform.position = new Vector3(snapX, transform.position.y, snapZ);
    }
}
