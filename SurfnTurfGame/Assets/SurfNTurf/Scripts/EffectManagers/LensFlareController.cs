using UnityEngine;
using UnityEngine.Rendering;

public class LensFlareController : MonoBehaviour
{
    [SerializeField] private LensFlareComponentSRP lensFlare;
    [SerializeField] private float maxIntensity = 1.3f;
    [SerializeField] private float minIntensity = 0.0f;
    [SerializeField] private float fadeSpeed = 1.0f;

    private void Update()
    {
        if (IsSunVisible())
        {
            lensFlare.intensity = Mathf.Lerp(lensFlare.intensity, maxIntensity, Time.deltaTime * fadeSpeed);
        }
        else
        {
            lensFlare.intensity = Mathf.Lerp(lensFlare.intensity, minIntensity, Time.deltaTime * fadeSpeed);
        }
    }

    private bool IsSunVisible()
    {
        //send a raycast from the camera into the direction of the main light
        Ray ray = new Ray(Camera.main.transform.position, -transform.forward * 500);
        return !Physics.Raycast(ray, out RaycastHit hit);
    }

    private void OnDrawGizmos()
    {
        if (IsSunVisible())
        {
            Gizmos.color = Color.red;
        }
        else
        {
            Gizmos.color = Color.green;
        }
        Gizmos.DrawRay(Camera.main.transform.position, -transform.forward * 500);
    }
}
