using UnityEngine;

public class LookAt : MonoBehaviour
{
    Camera camera;
    void Start()
    {
        //grab camera
        camera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(camera.transform.position, Vector3.up);
    }
}
