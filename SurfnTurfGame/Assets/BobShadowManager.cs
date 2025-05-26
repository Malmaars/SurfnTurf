using UnityEngine;

public class BobShadowManager : MonoBehaviour
{
    public Transform trackingTarget;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = trackingTarget.position - Vector3.up * 0.5f;
        transform.rotation = Quaternion.Euler(90, 0, 0);
    }
}
