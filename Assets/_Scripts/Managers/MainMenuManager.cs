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
    public GameObject confirmPanel;

    [Header("Button Text")]
    public TextMeshProUGUI[] profileNameTexts;
    public TextMeshProUGUI[] profileLoadTexts;
    public TextMeshProUGUI[] profileDeleteTexts;
    public TextMeshProUGUI confirmDeleteText;

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

            if (confirmDeleteText != null)
            {
                confirmDeleteText.text = "Delete " + playerName + "?";
                confirmDeleteText.color = Color.red;

 
            }

            confirmPanel.SetActive(true);
        }
        else
        {
            // STEP 2: The second click. Nuke the save file!
            if (profileIndex == PlayerStatsManager.Instance.currentProfileIndex)
                SceneManager.LoadScene("MainMenu");

            PlayerStatsManager.Instance.DeleteProfile(profileIndex);
            

            // Turn off the warning state
            isConfirmingDelete = false;


            // Refresh the UI to show the newly wiped empty slot
            UpdateButtonNames();
            confirmPanel.SetActive(true);
        }
    }

    public void CancelDelete()
    {
        if (isConfirmingDelete)
        {
            isConfirmingDelete = false;
            confirmPanel.SetActive(false);

        }
        UpdateButtonNames();
    }
}