using UnityEngine;
using Steamworks;

public class SetSteamStatTrigger : MonoBehaviour
{
    private bool area_reached = false;
    public bool isTrigger = true;
    [SerializeField] private string statName = "area_a_reached";
    //Ontrigger Enter set the stat
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if (SteamManager.Initialized)
            {
                if (area_reached == true) return;
                area_reached = true;
                SteamUserStats.SetStat(statName, area_reached ? 1 : 0);
                SteamUserStats.StoreStats();
            }
        }
    }
    private void OnCollisionEnter(Collision other)
    {
        if (isTrigger) return;

        if (other.collider.tag == "Player")
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
    private void Start()
    {
        if (SteamManager.Initialized)
        {
            SteamUserStats.GetStat(statName, out int statValue);
            area_reached = statValue == 1;
        }   
    }

}
