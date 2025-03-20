using NaughtyAttributes;
using UnityEngine;
using Steamworks;
using System.Collections;
using System;
using System.Threading.Tasks;

public class SteamStats : MonoBehaviour
{
    [SerializeField] private GameObject playerBody;
    private Vector3 playerPosition;
    [ReadOnly] public float playtime;
    [ReadOnly] public float distance;
    [ReadOnly] public int items_collected;
    [ReadOnly] public float height_reached;
    [ReadOnly] public float time_spent_in_water_movement_state;
    [ReadOnly] public float time_spent_in_movement_state;
    [ReadOnly] public float time_spent_airborne;
    [ReadOnly] public float time_spent_grounded;
    [ReadOnly] public float time_spent_cooking;
    [ReadOnly] public int jumps;
    [ReadOnly] public int wall_jumps;
    [ReadOnly] public int dashes;
    [ReadOnly] public int leaps;
    [ReadOnly] public int waves_hit;
    [ReadOnly] public int area_a_reached;
    [ReadOnly] public int area_b_reached;
    [ReadOnly] public int ingredient_moved;
    [ReadOnly] public int ingredient_swapped;
    [ReadOnly] public int ingredient_turned;

    async void Start()
    {
        GetStats();
        if (!SteamManager.Initialized)
        {
            Debug.LogError("</color=red>Steamworks isn't initialized</color>");
        }

        StartCoroutine(PlaytimeTimer());
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
    private IEnumerator PlaytimeTimer()
    {
        while (true)
        {
            SteamUserStats.GetStat("playtime", out playtime);
            playtime++;
            SteamUserStats.SetStat("playtime", playtime);
            SteamUserStats.StoreStats();
            yield return new WaitForSeconds(1);
        }
    }


    private void Update()
    {
#if UNITY_EDITOR
        GetStats();
#endif
        SetDistance();
        SetHeightReached();
    }
    private void SetDistance()
    {
        if (SteamManager.Initialized && playerBody != null)
        {
            if(Vector3.Distance(playerBody.transform.position, playerPosition) > 1f)
            {
                SteamUserStats.GetStat("distance", out distance);
                distance++;
                SteamUserStats.SetStat("distance", distance);
                SteamUserStats.StoreStats();
                playerPosition = playerBody.transform.position;
            }

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
    private void SetHeightReached()
    {
        if (SteamManager.Initialized && playerBody != null)
        {
            SteamUserStats.GetStat("height_reached", out height_reached);
            if (playerBody.transform.position.y > height_reached)
            {
                height_reached = playerBody.transform.position.y;
                SteamUserStats.SetStat("height_reached", height_reached);
                SteamUserStats.StoreStats();
            }
        }
    }

    private void GetStats()
    {
        if (SteamManager.Initialized)
        {
            SteamUserStats.GetStat("playtime", out playtime);
            SteamUserStats.GetStat("distance", out distance);
            SteamUserStats.GetStat("items_collected", out items_collected);
            SteamUserStats.GetStat("height_reached", out height_reached);
            SteamUserStats.GetStat("time_spent_in_water_movement_state", out time_spent_in_water_movement_state);
            SteamUserStats.GetStat("time_spent_in_movement_state", out time_spent_in_movement_state);
            SteamUserStats.GetStat("time_spent_airborne", out time_spent_airborne);
            SteamUserStats.GetStat("time_spent_grounded", out time_spent_grounded);
            SteamUserStats.GetStat("time_spent_cooking", out time_spent_cooking);
            SteamUserStats.GetStat("jumps", out jumps);
            SteamUserStats.GetStat("wall_jumps", out wall_jumps);
            SteamUserStats.GetStat("dashes", out dashes);
            SteamUserStats.GetStat("leaps", out leaps);
            SteamUserStats.GetStat("waves_hit", out waves_hit);
            SteamUserStats.GetStat("area_a_reached", out area_a_reached);
            SteamUserStats.GetStat("area_b_reached", out area_b_reached);
            SteamUserStats.GetStat("ingredient_moved", out ingredient_moved);
            SteamUserStats.GetStat("ingredient_swapped", out ingredient_swapped);
            SteamUserStats.GetStat("ingredient_turned", out ingredient_turned);
        }
    }

    [Button("Reset Stats", EButtonEnableMode.Playmode)]
    public void ResetStats()
    {
        if (SteamManager.Initialized)
        {
            SteamUserStats.ResetAllStats(true);
            GetStats();
        }
    }
}
