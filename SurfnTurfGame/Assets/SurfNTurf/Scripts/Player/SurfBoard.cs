using System;
using System.Collections;
using UnityEngine;

public class SurfBoard : MonoBehaviour
{
    public string surfBoardName = "Surfboard";
    public int price;
    public bool isUnlocked;
    public bool isEquipped;

    IEnumerator Start()
    {
        yield return new WaitUntil(() => CloudSaveSystem.Instance != null && CloudSaveSystem.Instance.IsInitialized);

        if (isUnlocked == true)
        {
            if (!CloudSaveSystem.Instance.data.surfboardsUnlocked.Contains(GetKey()))
            {
                CloudSaveSystem.Instance.data.surfboardsUnlocked.Add(GetKey());
            }
        }

        if (!CloudSaveSystem.Instance.data.surfboardsUnlocked.Contains(GetKey()))
        {
            isUnlocked = false;
        }
        else
        {
            isUnlocked = true;
        }
    }

    private string GetKey()
    {
        return surfBoardName;
    }

    public void Unlock()
    {
        if (!isUnlocked)
        {
            isUnlocked = true;
            CloudSaveSystem.Instance.data.surfboardsUnlocked.Add(GetKey());
        }
    }
    public bool IsUnlocked()
    {
        return CloudSaveSystem.Instance.data.surfboardsUnlocked.Contains(GetKey());
    }
}


