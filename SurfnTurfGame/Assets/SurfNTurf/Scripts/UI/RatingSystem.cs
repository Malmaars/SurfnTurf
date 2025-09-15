using Steamworks;
using TMPro;
using UnityEngine;

public class RatingSystem : MonoBehaviour
{
    [SerializeField] private string[] ratings;
    [SerializeField] private TMP_Text ratingsText;
    [SerializeField] private int ratingCount = 0;
    private int currentRatingIndex = 0;
    [SerializeField] private int maxRatingIndex = 0;
    [SerializeField] private string achievement;

    public void Rate(int rating)
    {
        if (SteamManager.Initialized)
        {
            GoogleSheetsIntegration.instance.StoreStat(ratings[currentRatingIndex], rating);
        }

        UIManager.instance.SetVisibleUI(true);
        gameObject.GetComponent<Canvas>().enabled = false;
        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        ratingCount++;

        if (ratingCount >= maxRatingIndex)
        {
            Debug.Log("All done with ratings");
            if (SteamManager.Initialized)
            {
                SteamUserStats.SetAchievement(achievement);
                SteamUserStats.StoreStats();
            }
        }
    }

    public void OpenRating(int index)
    {
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        UIManager.instance.SetVisibleUI(false);
        gameObject.GetComponent<Canvas>().enabled = true;
        currentRatingIndex = index;
        ratingsText.text = "What would you rate " + ratings[currentRatingIndex].Replace("_", " ");
    }
}
