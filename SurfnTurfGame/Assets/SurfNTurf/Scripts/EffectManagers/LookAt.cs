using UnityEngine;

public class LookAt : MonoBehaviour
{
    public bool reversed;
    Camera mainCamera;
    void Start()
    {
        //grab mainCamera
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if(!reversed)
            transform.forward = (transform.position - mainCamera.transform.position).normalized;
        else
			transform.forward = (mainCamera.transform.position - transform.position).normalized;

	}
}
