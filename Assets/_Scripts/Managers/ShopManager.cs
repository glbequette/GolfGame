using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ShopManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI walletText;

    [Header("Upgrade Texts")]
    public TextMeshProUGUI driverButtonText;
    public TextMeshProUGUI ironButtonText;
    public TextMeshProUGUI wedgeButtonText;
    public TextMeshProUGUI putterButtonText;

    [Header("Shop Economy")]
    // We make these static so the price remembers how high it got between holes!
    public static int baseCost = 50;



    void Start()
    {
        UpdateUI();
    }

    public void UpgradeDriver()
    {
        ClubSaveData clubData = PlayerStatsManager.Instance.GetClubStats("Driver");
        if (clubData != null && clubData.level < 10)
        {
            if (PlayerStatsManager.Instance.GetCurrentStats().wallet >= baseCost * Mathf.Pow(clubData.level + 1, 2))
            {
                PlayerStatsManager.Instance.RemoveMoney(Mathf.RoundToInt(baseCost * Mathf.Pow(clubData.level + 1, 2)));
                PlayerStatsManager.Instance.UpgradeClub("Driver");
            }
        }
        UpdateUI();
    }
    public void UpgradeIron()
    {
        ClubSaveData clubData = PlayerStatsManager.Instance.GetClubStats("Iron");
        if (clubData != null && clubData.level < 10)
        {
            if (PlayerStatsManager.Instance.GetCurrentStats().wallet >= baseCost * Mathf.Pow(clubData.level + 1, 2))
            {
                PlayerStatsManager.Instance.RemoveMoney(Mathf.RoundToInt(baseCost * Mathf.Pow(clubData.level + 1, 2)));
                PlayerStatsManager.Instance.UpgradeClub("Iron");
            }
        }
        UpdateUI();
    }

    public void UpgradeWedge()
    {
        ClubSaveData clubData = PlayerStatsManager.Instance.GetClubStats("Wedge");
        if (clubData != null && clubData.level < 10)
        {
            if (PlayerStatsManager.Instance.GetCurrentStats().wallet >= baseCost * Mathf.Pow(clubData.level + 1, 2))
            {
                PlayerStatsManager.Instance.RemoveMoney(Mathf.RoundToInt(baseCost * Mathf.Pow(clubData.level + 1, 2)));
                PlayerStatsManager.Instance.UpgradeClub("Wedge");
            }
        }
        UpdateUI();
    }

    public void UpgradePutter()
    {
        ClubSaveData clubData = PlayerStatsManager.Instance.GetClubStats("Putter");
        if (clubData != null && clubData.level < 10)
        {
            if (PlayerStatsManager.Instance.GetCurrentStats().wallet >= baseCost * Mathf.Pow(clubData.level + 1, 2))
            {
                PlayerStatsManager.Instance.RemoveMoney(Mathf.RoundToInt(baseCost * Mathf.Pow(clubData.level + 1, 2)));
                PlayerStatsManager.Instance.UpgradeClub("Putter");
            }
        }
        UpdateUI();
    }


    void UpdateUI()
    {
        if (PlayerStatsManager.Instance.GetCurrentStats() != null)
        {
            if (walletText != null) walletText.text = "$" + PlayerStatsManager.Instance.GetCurrentStats().wallet;

            if (driverButtonText != null)
            {
                ClubSaveData clubData = PlayerStatsManager.Instance.GetClubStats("Driver");
                if (clubData.level < 10)
                {
                    driverButtonText.text = "$" + (baseCost * Mathf.Pow(clubData.level + 1, 2));
                }
                else
                {
                    driverButtonText.text = "Max Level";
                }
            }
            if (ironButtonText != null)
            {
                ClubSaveData clubData = PlayerStatsManager.Instance.GetClubStats("Iron");
                if (clubData.level < 10)
                {
                    ironButtonText.text = "$" + (baseCost * Mathf.Pow(clubData.level + 1, 2));
                }
                else
                {
                    ironButtonText.text = "Max Level";
                }
            }
            if (wedgeButtonText != null)
            {
                ClubSaveData clubData = PlayerStatsManager.Instance.GetClubStats("Wedge");
                if (clubData.level < 10)
                {
                    wedgeButtonText.text = "$" + (baseCost * Mathf.Pow(clubData.level + 1, 2));
                }
                else
                {
                    wedgeButtonText.text = "Max Level";
                }
            }
            if (putterButtonText != null)
            {
                ClubSaveData clubData = PlayerStatsManager.Instance.GetClubStats("Putter");
                if (clubData.level < 10)
                {
                    putterButtonText.text = "$" + (baseCost * Mathf.Pow(clubData.level + 1, 2));
                }
                else
                {
                    putterButtonText.text = "Max Level";
                }
            }
        }
        
    }


}