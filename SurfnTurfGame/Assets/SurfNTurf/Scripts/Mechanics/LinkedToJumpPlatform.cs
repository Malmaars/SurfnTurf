using UnityEngine;

public class LinkedToJumpPlatform : MonoBehaviour
{
    public GameObject[] platformSections;
    private int platformIndex;

    public void SwitchPlatform()
    {
        platformIndex++;
        if (platformIndex >= platformSections.Length)
            platformIndex = 0;

        for (int i = 0; i < platformSections.Length; i++)
        {
            if(i == platformIndex)
            {
                platformSections[i].SetActive(true);
            }
            else
            {
                platformSections[i].SetActive(false);
            }
        }
    }
}
