using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class SwingManager : MonoBehaviour
{
    // The State Machine
    public enum SwingState { Idle, SettingAccuracy, SettingPower, BallInMotion }
    public SwingState currentState = SwingState.Idle;

    [Header("UI References")]
    public GameObject mainGameplayUI;   // Drag your normal UI here
    public GameObject swingMeterPanel;  // Drag the new Swing Panel here
    public Slider accuracySlider;
    public Slider powerSlider;

    [Header("Meter Speeds")]
    public float accuracySpeed = 2f;    // How fast the horizontal bar moves
    public float powerSpeed = 1.5f;     // How fast the vertical bar moves

    [Header("Golf Ball & Physics")]
    public BallController ballController; // <-- NEW: Swap Rigidbody2D for BallController!
    public float maxHitForce = 800f;    // The absolute max power of a club
    public float maxShotAngle = 45f;    // The max degrees off-center a completely missed accuracy click will send you

    // Internal tracking
    private float finalAccuracy;
    private float finalPower;
    private int powerDirection = 1;     // 1 for going up, -1 for coming down

    void Start()
    {
        ResetSwingState();
    }

    void Update()
    {
        // Change spaceKey to whatever input you want for the action button
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            HandleActionInput();
        }

        AnimateMeters();
    }

    void HandleActionInput()
    {
        switch (currentState)
        {
            case SwingState.Idle:
                // 1. Enter Focus Mode
                mainGameplayUI.SetActive(false);
                swingMeterPanel.SetActive(true);
                currentState = SwingState.SettingAccuracy;
                break;

            case SwingState.SettingAccuracy:
                // 2. Lock Accuracy, immediately start Power
                finalAccuracy = accuracySlider.value;
                currentState = SwingState.SettingPower;
                break;

            case SwingState.SettingPower:
                // 3. Lock Power and hit the ball!
                finalPower = powerSlider.value;
                HitBall();
                break;
        }
    }

    void AnimateMeters()
    {
        // --- NEW: Ask the BallController for the current club's multiplier! ---
        float clubSpeedMultiplier = 1f;
        if (ballController != null && ballController.GetCurrentClub() != null)
        {
            clubSpeedMultiplier = ballController.GetCurrentClub().meterSpeedMultiplier;
        }

        // Apply the multiplier to your base speeds
        float finalAccuracySpeed = accuracySpeed * clubSpeedMultiplier;
        float finalPowerSpeed = powerSpeed * clubSpeedMultiplier;


        if (currentState == SwingState.SettingAccuracy)
        {
            // Use the new finalAccuracySpeed
            accuracySlider.value = Mathf.PingPong(Time.time * finalAccuracySpeed, 2f) - 1f;
        }
        else if (currentState == SwingState.SettingPower)
        {
            // Use the new finalPowerSpeed
            powerSlider.value += finalPowerSpeed * powerDirection * Time.deltaTime;

            if (powerSlider.value >= 1f)
            {
                powerSlider.value = 1f;
                powerDirection = -1;
            }
            else if (powerSlider.value <= 0f && powerDirection == -1)
            {
                powerSlider.value = 0f;
                finalPower = 0f;
                HitBall();
            }
        }
    }

    void HitBall()
    {
        currentState = SwingState.BallInMotion;
        swingMeterPanel.SetActive(false);
        mainGameplayUI.SetActive(true);

        // --- UPDATED: Talk directly to the ballController reference! ---
        if (ballController != null)
        {
            if (finalPower < 0.1f)
                ballController.ExecuteShotFromMinigame(0.1f, finalAccuracy);
            else
                ballController.ExecuteShotFromMinigame(finalPower, finalAccuracy);
        }
    }

    public void ResetSwingState()
    {
        currentState = SwingState.Idle;
        accuracySlider.value = 0f;
        powerSlider.value = 0f;
        powerDirection = 1;
    }
}