using UnityEngine;
using UnityEngine.SceneManagement;

public class PlaytimeTimer : MonoBehaviour
{
    //singlton
    public static PlaytimeTimer instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public float playtime;
    private void Start()
    {
        InvokeRepeating("RepeatingFunction", 10f, 10f); //call every 10 seconds
    }
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
    public void RepeatingFunction()
    {
        GoogleSheetsIntegration.instance.StoreStat("Time in " + SceneManager.GetActiveScene().name,  FloatToSecondsMinutesHoures(playtime));
    }

    public string FloatToSecondsMinutesHoures(float time)
    {
        int intTime = Mathf.FloorToInt(time);
        int seconds = intTime % 60;
        int minutes = (intTime / 60) % 60;
        int hours = intTime / 3600;
        return hours + ":" + minutes + ":" + seconds;
    }


}
