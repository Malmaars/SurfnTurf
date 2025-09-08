using UnityEngine;

public class DoubleJumpReset : MonoBehaviour
{
    private float timer = 0f;
    private float resetTime = 0.5f;
    public void Consume()
    {
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
        timer = resetTime;
    }

    private void Update()
    {
        if (timer > 0f)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                GetComponent<MeshRenderer>().enabled = true;
                GetComponent<Collider>().enabled = true;
            }
        }
    }
}
