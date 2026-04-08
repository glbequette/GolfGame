using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialManager : MonoBehaviour
{
    [Header("Tutorial Content")]
    public GameObject[] tutorialPanels;
    private int currentPanelIndex = 0;

    private bool isTutorialActive = false;


    void Start()
    {
        // 1. When the Main Menu loads, check the active save file
        if (PlayerStatsManager.Instance != null && PlayerStatsManager.Instance.HasSeenTutorial())
        {
            // They already know how to play. Hide UI and let them click the menu!
            return;
        }

        // 2. This is a brand new save! Show the UI.
        tutorialPanels[0].SetActive(true);
        isTutorialActive = true;
    }

    private void Update()
    {
        if (!isTutorialActive)
        {
            // 1. When the Main Menu loads, check the active save file
            if (PlayerStatsManager.Instance != null && PlayerStatsManager.Instance.HasSeenTutorial())
            {
                // They already know how to play. Hide UI and let them click the menu!
                return;
            }

            // 2. This is a brand new save! Show the UI.
            tutorialPanels[0].SetActive(true);
            isTutorialActive = true;
        }
        bool keyboardContinue = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        bool gamepadContinue = Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame;
        bool mouseContinue = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

        if (keyboardContinue || gamepadContinue || mouseContinue && isTutorialActive)
        {
            ShowNextMessage();
        }
    }

    public void ShowNextMessage()
    {
        if (currentPanelIndex + 1 < tutorialPanels.Length)
        {
            tutorialPanels[currentPanelIndex].SetActive(false);
            currentPanelIndex++;
            tutorialPanels[currentPanelIndex].SetActive(true);
        }
        else
        {
            FinishTutorial();
        }
    }

    void FinishTutorial()
    {
        // Hide the UI
        tutorialPanels[currentPanelIndex].SetActive(false);

        // Permanently save to the hard drive that they finished it
        if (PlayerStatsManager.Instance != null)
        {
            PlayerStatsManager.Instance.MarkTutorialAsSeen();
        }

        // Destroy this manager so it's gone forever
        isTutorialActive = false;
    }
}