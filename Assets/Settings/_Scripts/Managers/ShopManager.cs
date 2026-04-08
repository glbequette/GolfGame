using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ShopManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI walletText;

    [Header("Upgrade Texts")]
    public TextMeshProUGUI powerButtonText;
    public TextMeshProUGUI accuracyButtonText;
    public TextMeshProUGUI hazardButtonText;
    public TextMeshProUGUI incomeButtonText;

    [Header("Shop Economy")]
    // We make these static so the price remembers how high it got between holes!
    public static int powerCost = 50;
    public static int accuracyCost = 50;
    public static int hazardCost = 50;
    public static int incomeCost = 50;

    // How much the stats improve per purchase (0.05f = 5%)
    public float upgradeAmount = 0.02f;

    [Header("Scene Routing")]
    public string gameplaySceneName = "Gameplay";

    void Start()
    {
        UpdateUI();
    }

    public void BuyPowerUpgrade()
    {
        if (LevelManager.currentMoney >= powerCost)
        {
            // 1. Take the money
            LevelManager.currentMoney -= powerCost;

            // 2. Increase the Max Power by 5%
            LevelManager.globalPowerMultiplier += upgradeAmount;

            // 3. Make the next upgrade cost $25 more
            powerCost += 25;

            UpdateUI();
        }
    }

    public void BuyAccuracyUpgrade()
    {
        if (LevelManager.currentMoney >= accuracyCost)
        {
            LevelManager.currentMoney -= accuracyCost;

            // Accuracy gets BETTER when the variance angle gets SMALLER, so we subtract!
            LevelManager.globalAccuracyMultiplier -= upgradeAmount;

            // Put a hard limit so accuracy never goes below 0 (which would invert your shots)
            LevelManager.globalAccuracyMultiplier = Mathf.Max(0.1f, LevelManager.globalAccuracyMultiplier);

            accuracyCost += 25;

            UpdateUI();
        }
    }

    public void BuyHazardUpgrade()
    {
        if (LevelManager.currentMoney >= hazardCost)
        {
            LevelManager.currentMoney -= hazardCost;
            LevelManager.globalHazardBonus += 0.10f; // Regain 10% power out of hazards!
            hazardCost += 25;
            UpdateUI();
        }
    }

    public void BuyIncomeUpgrade()
    {
        if (LevelManager.currentMoney >= incomeCost)
        {
            LevelManager.currentMoney -= incomeCost;
            LevelManager.globalIncomeMultiplier += 0.20f; // Earn 20% more money permanently!
            incomeCost += 50; // Scales up fast
            UpdateUI();
        }
    }

    void UpdateUI()
    {
        if (walletText != null) walletText.text = "$" + LevelManager.currentMoney;

        if (powerButtonText != null) powerButtonText.text = "$" + powerCost;
        if (accuracyButtonText != null) accuracyButtonText.text = "$" + accuracyCost;
        if (hazardButtonText != null) hazardButtonText.text = "$" + hazardCost;
        if (incomeButtonText != null) incomeButtonText.text = "$" + incomeCost;
    }

    public void GoToNextHole()
    {
        SceneManager.LoadScene(gameplaySceneName);
    }
}