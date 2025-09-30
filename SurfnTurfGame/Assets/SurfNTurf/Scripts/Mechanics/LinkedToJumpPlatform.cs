using UnityEngine;

public class LinkedToJumpPlatform : MonoBehaviour
{
    public GameObject[] platformSections;
    private int platformIndex;

    public Material onColor;
    public Material offColor;

    public void SwitchPlatform()
    {
        platformIndex++;
        if (platformIndex >= platformSections.Length)
            platformIndex = 0;

        for (int i = 0; i < platformSections.Length; i++)
        {
            if(i == platformIndex)
            {
                platformSections[i].GetComponent<Collider>().enabled = true;
                platformSections[i].GetComponent<MeshRenderer>().material = onColor;
            }
            else
            {
                platformSections[i].GetComponent<Collider>().enabled = true;
                platformSections[i].GetComponent<MeshRenderer>().material = offColor;
            }
        }
    }
}
