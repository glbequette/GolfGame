using TMPro;
using UnityEngine;

public class StatsDisplay : MonoBehaviour
{
    [Header("UI Text Elements")]
    public TextMeshProUGUI birdiesText;
    public TextMeshProUGUI eaglesText;
    public TextMeshProUGUI holeInOnesText;
    public TextMeshProUGUI parsText;

    void Start()
    {
        // We update the stats as soon as the stats scene loads
        UpdateStatsUI();
    }

    public void UpdateStatsUI()
    {
        // A quick safety check to prevent the NullReferenceException we saw earlier!
        if (PlayerStatsManager.Instance == null)
        {
            Debug.LogError("PlayerStatsManager is not in the scene. Make sure you loaded from the Main Menu!");
            return;
        }

        // Fetch the loaded JSON data from the Singleton
        LifetimeStatsData currentStats = PlayerStatsManager.Instance.GetCurrentStats();

        // Update the TMP text components (ToString converts the integer to text)
        // We check if they are != null just in case you haven't assigned them all in the inspector yet
        if (birdiesText != null) birdiesText.text = "Birdies: " + currentStats.totalBirdies.ToString();
        if (eaglesText != null) eaglesText.text = "Eagles+: " + currentStats.totalEagles.ToString();
        if (holeInOnesText != null) holeInOnesText.text = " Hole In Ones: " + currentStats.totalHoleInOnes.ToString();
        if (parsText != null) parsText.text = "Pars: " + currentStats.totalPars.ToString();
    }
}