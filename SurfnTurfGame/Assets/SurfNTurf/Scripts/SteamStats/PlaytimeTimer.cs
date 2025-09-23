using UnityEngine;
using UnityEngine.SceneManagement;

public class PlaytimeTimer : MonoBehaviour
{
    private float playtime;
    void Update()
    {
        playtime += Time.deltaTime;
    }

    //ondisable is called when the object is disabled or destroyed
    void OnDisable()
    {
        GoogleSheetsIntegration.instance.StoreStat("Time in " + SceneManager.GetActiveScene().name,  FloatToSecondsMinutesHoures(playtime));
    }
    void OnApplicationQuit()
    {
        GoogleSheetsIntegration.instance.StoreStat("Time in " + SceneManager.GetActiveScene().name,  FloatToSecondsMinutesHoures(playtime));
    }

    private string FloatToSecondsMinutesHoures(float time)
    {
        int intTime = Mathf.FloorToInt(time);
        int seconds = intTime % 60;
        int minutes = (intTime / 60) % 60;
        int hours = intTime / 3600;
        return hours + ":" + minutes + ":" + seconds;
    }


}
