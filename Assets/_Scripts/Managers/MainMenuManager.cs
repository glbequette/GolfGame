using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement; // Required for loading levels!

public class MainMenuManager : MonoBehaviour
{
    [Header("Level Settings")]
    [Tooltip("Type the exact name of your first gameplay scene here.")]
    public TMP_InputField nameInputField;

    [Header("Profile Panels")]
    public GameObject[] profilePanels;

    [Header("Button Text")]
    public TextMeshProUGUI[] profileNameTexts;
    public TextMeshProUGUI[] profileLoadTexts;
    public TextMeshProUGUI[] profileDeleteTexts;

    private bool isConfirmingDelete = false;

    public void Start()
    {
        UpdateButtonNames();
        SelectProfile(PlayerStatsManager.Instance.currentProfileIndex);
        profileLoadTexts[PlayerStatsManager.Instance.currentProfileIndex].text = "Current Profile";
        profileDeleteTexts[PlayerStatsManager.Instance.currentProfileIndex].text = "Reset Profile";
    }

    public void PlayGame()
    {
        Debug.Log("Starting a new run...");

        // --- THE ROGUELIKE RESET ---
        // We must wipe the player's static stats clean so they don't carry over from a previous death/win!
        LevelManager.currentMoney = 0;
        LevelManager.globalAccuracyMultiplier = 1f;
        LevelManager.globalPowerMultiplier = 1f;
        LevelManager.globalHazardBonus = 0f;
        LevelManager.globalIncomeMultiplier = 1f;

        // Reset the shop prices back to their default starting costs
        ShopManager.powerCost = 50;
        ShopManager.accuracyCost = 50;
        ShopManager.hazardCost = 50;
        ShopManager.incomeCost = 50;

        SceneManager.LoadScene("CourseSelection");
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");

        // This closes the game if you build it for PC/Mac
        Application.Quit();
    }

    public void UpdateButtonNames()
    {
        if (PlayerStatsManager.Instance == null) return;

        if (profileNameTexts[0] != null)
        {
            profileNameTexts[0].text = PlayerStatsManager.Instance.GetProfileName(0);
            profileNameTexts[0].color = Color.white;
        }
        if (profileNameTexts[1] != null)
        {
            profileNameTexts[1].text = PlayerStatsManager.Instance.GetProfileName(1);
            profileNameTexts[1].color = Color.white;
        }
        if (profileNameTexts[2] != null)
        {
            profileNameTexts[2].text = PlayerStatsManager.Instance.GetProfileName(2);
            profileNameTexts[2].color = Color.white;

        }
    }

    public void UpdateNameFromInput(int profileIndex)
    {
        CancelDelete();
        if (nameInputField != null && !string.IsNullOrEmpty(nameInputField.text))
        {
            // Tell the save system to update the name
            PlayerStatsManager.Instance.RenameProfile(profileIndex, nameInputField.text);

            // Refresh the UI to show the new name
            UpdateButtonNames();

            // Optional: Clear the input box after they hit save
            nameInputField.text = "";
        }
    }

    public void LoadProfile(int profileNumber)
    {
        CancelDelete();
        PlayerStatsManager.Instance.LoadProfile(profileNumber);
        SceneManager.LoadScene("MainMenu");
    }

    public void SelectProfile(int profileNumber)
    {
        if (profileNumber == 0)
        {
            profilePanels[0].SetActive(true);
            profilePanels[1].SetActive(false);
            profilePanels[2].SetActive(false);
        }
        else if (profileNumber == 1) 
        {
            profilePanels[0].SetActive(false);
            profilePanels[1].SetActive(true);
            profilePanels[2].SetActive(false);
        }
        else if (profileNumber == 2)
        {
            profilePanels[0].SetActive(false);
            profilePanels[1].SetActive(false);
            profilePanels[2].SetActive(true);
        }
    }

    public void DeleteProfile(int profileIndex)
    {
        if (PlayerStatsManager.Instance == null) return;

        if (!isConfirmingDelete)
        {
            // STEP 1: The first click. Turn on the warning!
            isConfirmingDelete = true;
            string playerName = PlayerStatsManager.Instance.GetProfileName(profileIndex);

            if (profileNameTexts[profileIndex] != null)
            {
                profileNameTexts[profileIndex].text = "Delete " + playerName + "?";
                profileNameTexts[profileIndex].color = Color.red;

                // Optional: You can also change the text color to red here to make it obvious!
                // currentProfileText.color = Color.red; 
            }
        }
        else
        {
            // STEP 2: The second click. Nuke the save file!
            if (profileIndex == PlayerStatsManager.Instance.currentProfileIndex)
                SceneManager.LoadScene("MainMenu");

            PlayerStatsManager.Instance.DeleteProfile(profileIndex);
            

            // Turn off the warning state
            isConfirmingDelete = false;

            // Optional: If you changed the color to red above, change it back to white/black here!
            // if (currentProfileText != null) currentProfileText.color = Color.white;

            // Refresh the UI to show the newly wiped empty slot
            UpdateButtonNames();
        }
    }

    public void CancelDelete()
    {
        if (isConfirmingDelete)
        {
            isConfirmingDelete = false;

        }
        UpdateButtonNames();
    }
}