using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // Required for the new Input System

public class PauseManager : MonoBehaviour
{
    [Header("UI References")]
    public UISlider pauseMenuSlider; // Change this from GameObject to our new script!

    public static bool isPaused = false;
    public string mainMenuSceneName = "MainMenu";

    void Update()
    {
        // Safety check to ensure a keyboard is connected, then check if Escape was pressed
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        pauseMenuSlider.SlideIn(); // Animate it up!
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pauseMenuSlider.SlideOut(); // Animate it down!
    }

    public void ExitToMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}