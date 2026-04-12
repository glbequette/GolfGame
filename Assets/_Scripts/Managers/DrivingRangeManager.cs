using UnityEngine;
using UnityEngine.InputSystem;

public class DrivingRangeManager : MonoBehaviour
{
    [Header("References")]
    public SwingManager swingManager;
    public BallController ballController;

    [Tooltip("Drag your Golf Ball here")]
    public Transform golfBall;

    [Tooltip("Drag the 3 empty GameObjects representing your tee boxes here")]
    public Transform[] teePositions;


    private int currentTeeIndex = 0;

    void Start()
    {
        // Move to the first tee immediately when the scene loads
        if (teePositions != null && teePositions.Length > 0)
        {
            MoveBallToTee(0);
        }
    }

    void Update()
    {
        // We use booleans to combine Keyboard and Gamepad checks cleanly
        bool moveLeft = false;
        bool moveRight = false;

        // --- 1. Keyboard Checks ---
        if (Keyboard.current != null)
        {
            if (Keyboard.current.qKey.wasPressedThisFrame) moveLeft = true;
            if (Keyboard.current.eKey.wasPressedThisFrame) moveRight = true;
        }

        // --- 2. Gamepad Checks (Triggers) ---
        if (Gamepad.current != null)
        {
            // The New Input System treats analog triggers as buttons that "press" when pulled past a threshold
            if (Gamepad.current.leftTrigger.wasPressedThisFrame) moveLeft = true;
            if (Gamepad.current.rightTrigger.wasPressedThisFrame) moveRight = true;
        }

        // --- 3. Execute the Move ---
        if (swingManager.currentState != SwingManager.SwingState.SettingAccuracy && swingManager.currentState != SwingManager.SwingState.SettingPower && moveLeft && !ballController.isMoving)
        {
            currentTeeIndex--;
            // If we go past the first tee, loop back to the last one
            if (currentTeeIndex < 0) currentTeeIndex = teePositions.Length - 1;

            MoveBallToTee(currentTeeIndex);
        }
        else if (swingManager.currentState != SwingManager.SwingState.SettingAccuracy && swingManager.currentState != SwingManager.SwingState.SettingPower && moveRight && !ballController.isMoving)
        {
            currentTeeIndex++;
            // If we go past the last tee, loop back to the first one
            if (currentTeeIndex >= teePositions.Length) currentTeeIndex = 0;

            MoveBallToTee(currentTeeIndex);
        }
    }

    private void MoveBallToTee(int index)
    {
        if (golfBall == null || teePositions.Length == 0) return;

        // 1. Teleport the ball
        golfBall.position = teePositions[index].position;

        // 2. Kill the physics! (Crucial if they switch tees while the ball is rolling/flying)
        Rigidbody2D rb = golfBall.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // 3. Reset the ball's brain (Uncomment this if you are using your BallController's reset method!)
        // BallController ballController = golfBall.GetComponent<BallController>();
        // if (ballController != null)
        // {
        //     ballController.ResetForNewHole();
        // }

        Debug.Log($"Moved to Tee Box {index + 1}");
    }
}