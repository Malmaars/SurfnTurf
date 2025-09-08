using NaughtyAttributes;
using UnityEngine;
using Steamworks;
using System.Collections;
using System;
using System.Threading.Tasks;

public class SteamStats : MonoBehaviour
{
    private GameObject playerBody;
    private Vector3 playerPosition;

    async void Start()
    {
        playerBody = BlackBoard.playerBody.gameObject;
        if (!SteamManager.Initialized)
        {
            Debug.LogError("</color=red>Steamworks isn't initialized</color>");
        }

        playerPosition = playerBody.transform.position;
        if (SteamManager.Initialized)
        {
            await this.GetComponent<GoogleSheetsIntegration>().StoreSteamID(SteamUser.GetSteamID().ToString());
        }
    }
    //On aplication close store the stats
    void OnApplicationQuit()
    {
        if (SteamManager.Initialized)
        {
            SteamUserStats.StoreStats();
        }
    }

    public void SetSteamStat(string statName)
    {
        if (SteamManager.Initialized)
        {
            SteamUserStats.GetStat(statName, out int statValue);
            statValue++;
            SteamUserStats.SetStat(statName, statValue);
            SteamUserStats.StoreStats();
        }
    }

}
