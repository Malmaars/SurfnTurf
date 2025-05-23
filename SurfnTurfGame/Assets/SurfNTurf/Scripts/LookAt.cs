using UnityEngine;

public class LookAt : MonoBehaviour
{
    Camera mainCamera;
    void Start()
    {
        //grab mainCamera
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(mainCamera.transform.position, Vector3.up);
    }
}
