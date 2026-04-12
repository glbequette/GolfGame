using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public class GolfClub
{
    public string clubName;
    public Sprite clubCardImage;
    public float powerModifier;
    public float maxVarianceAngle;
    public float loft;
    public float roughPenaltyMultiplier = 1f;
    public float sandPenaltyMultiplier = 1f;
    public float meterSpeedMultiplier = 1.0f;
    

    // --- NEW: Designer-friendly distance stat ---
    [Tooltip("Roughly how many yards this club hits at 100% power")]
    public float estimatedDistanceYards = 250f;
}

public class BallController : MonoBehaviour
{
    [Header("Aiming System & Minigame")]
    public SwingManager swingManager;    // Connect your new UI manager here!
    public float aimAngle = 0f;          // The current direction we are facing
    public float aimRotationSpeed = 80f; // How fast the aim line turns (degrees per second)

    [Header("Club Inventory")]
    public GolfClub[] availableClubs;
    public Image clubImageDisplay;
    private int currentClubIndex = 0;

    [Header("Physics Settings")]
    public Rigidbody2D rb;
    public float stopThreshold = 0.1f;

    [Header("Flight & Altitude (Fake 3D)")]
    public float currentHeight = 0f;
    public float verticalVelocity = 0f;
    public float gravity = 15f;
    public float heightVisualScale = 0.5f;
    private Vector3 originalScale;

    [Header("Bounce & Shadow")]
    public float bounciness = 0.4f;
    public float bounceThreshold = 2f;

    public Transform dropShadow;
    private Vector3 originalShadowScale;

    [Header("Terrain Friction")]
    public float normalDrag = 1.5f;
    public float roughDrag = 3.0f;
    public float sandDrag = 15.0f;
    public float airDrag = 0.5f;

    private int sandTouches = 0;
    private int roughTouches = 0;

    [Header("Score Tracking")]
    public int strokesTaken = 0;
    public TextMeshProUGUI strokeText;

    [Header("Distance Tracker")]
    public Transform targetHole;
    public float yardsPerUnit = 10f;
    public TextMeshProUGUI distanceText;

    private Vector2 shotStartPosition;
    private string lastClubUsed;

    [Header("Visuals")]
    public LineRenderer lineRenderer;
    public LineRenderer minimapLineRenderer;

    public Color aimLineColor = Color.white; // Simplified since power is handled by UI now

    public bool isMoving = false;
    private bool isSplashing = false;
    public bool isSinking = false;

    [Header("Driving Range Mode")]
    [Tooltip("Check this box ONLY in your Driving Range scene!")]
    public bool isDrivingRange = false;
    public TextMeshProUGUI drivenDistanceText; // The UI text to show your final distance

    void Start()
    {
        strokesTaken = 0;
        UpdateStrokeUI();
        UpdateClubUI();
        rb.linearDamping = normalDrag;

        originalScale = transform.localScale;
        if (dropShadow != null) originalShadowScale = dropShadow.localScale;
    }

    void Update()
    {
        // 1. HANDLE FAKE GRAVITY & FLIGHT
        if (currentHeight > 0 || verticalVelocity > 0)
        {
            currentHeight += verticalVelocity * Time.deltaTime;
            verticalVelocity -= gravity * Time.deltaTime;

            transform.localScale = originalScale * (1f + (currentHeight * heightVisualScale));

            if (dropShadow != null)
            {
                float shadowXOffset = 0.15f + (currentHeight * 0.1f);
                float shadowYOffset = -0.15f - (currentHeight * 0.1f);

                dropShadow.position = new Vector3(
                    transform.position.x + shadowXOffset,
                    transform.position.y + shadowYOffset,
                    transform.position.z
                );

                float shadowShrink = Mathf.Max(0.2f, 1f - (currentHeight * 0.15f));
                dropShadow.localScale = originalShadowScale * shadowShrink;
            }

            // Bounce Logic
            if (currentHeight <= 0)
            {
                currentHeight = 0;

                if (Mathf.Abs(verticalVelocity) > bounceThreshold)
                {
                    verticalVelocity = Mathf.Abs(verticalVelocity) * bounciness;
                }
                else
                {
                    verticalVelocity = 0;
                    transform.localScale = originalScale;
                    if (dropShadow != null) dropShadow.localScale = originalShadowScale;
                    UpdateFriction();
                }
            }
        }
        else
        {
            if (dropShadow != null)
            {
                dropShadow.position = new Vector3(
                    transform.position.x + 0.1f,
                    transform.position.y - 0.1f,
                    transform.position.z
                );
            }
        }

        // 2. STOP THE BALL IF IT'S ROLLING SLOWLY
        if (rb.linearVelocity.magnitude < stopThreshold && isMoving && currentHeight == 0)
        {
            StopBall();
        }

        // 3. ALLOW AIMING IF STOPPED
        if (!isMoving && !isSinking)
        {
            HandleClubSwitching();
            HandleInput();

            if (!isDrivingRange)
            {
                UpdateDistanceUI(); // Normal game targeting
            }
        }
    }

    public void ResetForNewHole()
    {
        // 1. Turn off the locks so the player can swing again!
        isMoving = false;

        // (If you have an isSinking variable, make sure to reset it here too)
        isSinking = false; 

        // 2. Reset physics variables
        currentHeight = 0f;
        verticalVelocity = 0f;

        // 3. Reset the stroke count
        strokesTaken = 0;
        UpdateStrokeUI();

        // 4. Force the swing manager back to Idle just in case
        if (swingManager != null)
        {
            swingManager.ResetSwingState();
        }
    }

    void HandleClubSwitching()
    {
        if (availableClubs.Length == 0) return;

        // Freeze club switching if the minigame is active
        if (swingManager != null && swingManager.currentState != SwingManager.SwingState.Idle) return;

        if (Mouse.current != null)
        {
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (scroll > 0f) CycleClub(1);
            else if (scroll < 0f) CycleClub(-1);
        }

        if (Gamepad.current != null)
        {
            if (Gamepad.current.rightShoulder.wasPressedThisFrame) CycleClub(1);
            if (Gamepad.current.leftShoulder.wasPressedThisFrame) CycleClub(-1);
        }
    }

    public GolfClub GetCurrentClub()
    {
        if (availableClubs.Length > 0)
        {
            return availableClubs[currentClubIndex];
        }
        return null;
    }

    void CycleClub(int direction)
    {
        currentClubIndex += direction;

        if (currentClubIndex >= availableClubs.Length) currentClubIndex = 0;
        else if (currentClubIndex < 0) currentClubIndex = availableClubs.Length - 1;

        UpdateClubUI();
    }

    void UpdateClubUI()
    {
        if (clubImageDisplay != null && availableClubs.Length > 0)
        {
            clubImageDisplay.sprite = availableClubs[currentClubIndex].clubCardImage;
        }
    }

    void HandleInput()
    {
        if (availableClubs.Length == 0) return;

        // Lock out the aiming controls if the Swing minigame is currently active
        if (swingManager != null && swingManager.currentState != SwingManager.SwingState.Idle)
        {
            if (lineRenderer != null) lineRenderer.enabled = false;
            if (minimapLineRenderer != null) minimapLineRenderer.enabled = false;
            return;
        }

        GolfClub currentClub = availableClubs[currentClubIndex];

        float powerLevel = 1f;
        if (PlayerStatsManager.Instance != null)
        {
            ClubSaveData clubData = PlayerStatsManager.Instance.GetClubStats(currentClub.clubName);
            powerLevel = 1f + 0.05f * clubData.level;
        }

        float baseDistanceInUnits = (currentClub.estimatedDistanceYards * powerLevel) / yardsPerUnit;

        if (sandTouches >= 1)
        {
            baseDistanceInUnits *= currentClub.sandPenaltyMultiplier;
        }
        else if (roughTouches >= 1)
        {
            baseDistanceInUnits *=  currentClub.roughPenaltyMultiplier;
        }
        
            

        float upgradedVisualDistance = baseDistanceInUnits;

        float rotationInput = 0f;

        // Keyboard Aiming (A/D or Arrows)
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) rotationInput = 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) rotationInput = -1f;
        }

        // Gamepad Aiming (Left Stick X-Axis)
        if (Gamepad.current != null && Mathf.Abs(Gamepad.current.leftStick.x.ReadValue()) > 0.1f)
        {
            rotationInput = -Gamepad.current.leftStick.x.ReadValue();
        }

        // Apply rotation
        aimAngle += rotationInput * aimRotationSpeed * Time.deltaTime;

        // Calculate the Aim Direction
        Vector2 aimDirection = Quaternion.Euler(0, 0, aimAngle) * Vector2.up;
        Vector2 endPoint = (Vector2)transform.position + (aimDirection * upgradedVisualDistance);

        // Pass the club name to the drawing function!
        DrawAimLines(endPoint, currentClub.clubName);
    }
    void DrawAimLines(Vector2 endPoint, string currentClubName)
    {
        // 1. ALWAYS draw the minimap line
        if (minimapLineRenderer != null)
        {
            minimapLineRenderer.enabled = true;
            minimapLineRenderer.startColor = aimLineColor;
            minimapLineRenderer.endColor = aimLineColor;

            minimapLineRenderer.SetPosition(0, new Vector3(transform.position.x, transform.position.y, -1f));
            minimapLineRenderer.SetPosition(1, new Vector3(endPoint.x, endPoint.y, -1f));
        }

        // 2. ONLY draw the main line if the club is a Putter
        if (lineRenderer != null)
        {
            // We use .ToLower() just in case you named it "putter" instead of "Putter"
            if (currentClubName.ToLower().Contains("putter"))
            {
                lineRenderer.enabled = true;
                lineRenderer.startColor = aimLineColor;
                lineRenderer.endColor = aimLineColor;

                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(1, endPoint);
            }
            else
            {
                // Make sure it turns off if we switch back to an iron or wood!
                lineRenderer.enabled = false;
            }
        }
    }

    // --- NEW: This is called ONLY by the SwingManager once the UI game is done ---
    public void ExecuteShotFromMinigame(float finalPowerPercentage, float accuracyMeterValue)
    {
        GolfClub club = availableClubs[currentClubIndex];


        float powerMultiplier = 1.0f;
        float accuracyMultiplier = 1.0f;

        // 3. Fetch the specific upgrades for THIS club!
        if (PlayerStatsManager.Instance != null)
        {
            ClubSaveData clubData = PlayerStatsManager.Instance.GetClubStats(club.clubName);

            // Example: Each level adds 5% more distance
            powerMultiplier = 1.0f + (clubData.level * 0.05f);

            // Example: Each level shrinks the random variance angle by 5%
            accuracyMultiplier = 1.0f - (clubData.level * 0.05f);

            // (Make sure accuracy multiplier never goes below 0!)
            accuracyMultiplier = Mathf.Max(0.1f, accuracyMultiplier);
        }

        // 1. Calculate final accuracy (accuracyMeterValue is between -1 and 1)
        float upgradedVariance = club.maxVarianceAngle * accuracyMultiplier;
        float actualAngleOffset = accuracyMeterValue * upgradedVariance;

        // 2. Calculate direction based on where we aimed + the meter's mistake
        Vector2 baseDirection = Quaternion.Euler(0, 0, aimAngle) * Vector2.up;
        Vector3 finalDirection = Quaternion.Euler(0, 0, actualAngleOffset) * baseDirection;

        // 3. Calculate loft 
        verticalVelocity = club.loft * finalPowerPercentage;
        if (verticalVelocity > 0)
        {
            currentHeight = 0.01f;
            UpdateFriction();
        }

        lastClubUsed = club.clubName;
        shotStartPosition = transform.position;

        // 4. Calculate hazard penalties
        float hazardMultiplier = 1f;
        if (sandTouches > 0)
        {
            hazardMultiplier = Mathf.Min(1f, club.sandPenaltyMultiplier);
        }
        else if (roughTouches > 0)
        {
            hazardMultiplier = Mathf.Min(1f, club.roughPenaltyMultiplier);
        }

        // 5. Apply the final physical force
        float finalForce = 7 * finalPowerPercentage * powerMultiplier * club.powerModifier * hazardMultiplier;

        Shoot(finalDirection * finalForce);
    }

    void Shoot(Vector2 force)
    {
        if (lineRenderer != null) lineRenderer.enabled = false;
        if (minimapLineRenderer != null) minimapLineRenderer.enabled = false;

        isMoving = true;
        strokesTaken++;
        UpdateStrokeUI();

        rb.AddForce(force, ForceMode2D.Impulse);
    }

    void StopBall()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0;
        isMoving = false;

        float distanceTraveled = Vector2.Distance(shotStartPosition, transform.position);
        int yardsTraveled = Mathf.RoundToInt(distanceTraveled * yardsPerUnit);

        Debug.Log("Shot Result: Hit the " + lastClubUsed + " for " + yardsTraveled + " yards.");

        if (isDrivingRange)
        {
            // 1. Leave the text on the screen!
            if (drivenDistanceText != null)
            {
                drivenDistanceText.text = lastClubUsed + " Hit\n" + yardsTraveled + " Yards";
            }

            // 2. Instantly teleport the ball back so they can swing again
            ResetToTee();
        }
        else
        {
            // Normal gameplay targeting
            UpdateDistanceUI();

            if (swingManager != null)
            {
                swingManager.ResetSwingState();
            }
        }
    }

    void UpdateStrokeUI()
    {
        if (strokeText != null) strokeText.text = "" + strokesTaken;
    }

    void UpdateDistanceUI()
    {
        if (targetHole != null && distanceText != null)
        {
            float rawDistance = Vector2.Distance(transform.position, targetHole.position);
            int yardsToPin = Mathf.RoundToInt(rawDistance * yardsPerUnit);
            distanceText.text = yardsToPin + " Yards Away";
        }
    }

    private void UpdateFriction()
    {
        if (currentHeight > 0)
        {
            rb.linearDamping = airDrag;
            return;
        }

        sandTouches = Mathf.Max(0, sandTouches);
        roughTouches = Mathf.Max(0, roughTouches);

        if (sandTouches > 0) rb.linearDamping = sandDrag;
        else if (roughTouches > 0) rb.linearDamping = roughDrag;
        else rb.linearDamping = normalDrag;
    }

    private IEnumerator SplashSequence()
    {
        isSplashing = true;
        Debug.Log("Splash! Water Hazard!");

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0;
        isMoving = false;

        currentHeight = 0;
        verticalVelocity = 0;

        if (lineRenderer != null) lineRenderer.enabled = false;
        if (minimapLineRenderer != null) minimapLineRenderer.enabled = false;

        yield return new WaitForSeconds(1.0f);

        strokesTaken++;
        UpdateStrokeUI();

        transform.position = shotStartPosition;
        UpdateFriction();

        if (swingManager != null) swingManager.ResetSwingState();

        isSplashing = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Sand")) sandTouches++;
        else if (other.CompareTag("Rough")) roughTouches++;

        if (currentHeight <= 0) UpdateFriction();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Water") && currentHeight <= 0 && !isSplashing)
        {
            StartCoroutine(SplashSequence());
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Sand")) sandTouches--;
        else if (other.CompareTag("Rough")) roughTouches--;

        if (currentHeight <= 0) UpdateFriction();
    }

    // --- NEW: Driving Range Reset ---
    public void ResetToTee()
    {
        if (!isDrivingRange) return;

        // 1. Teleport back to the exact spot we swung from
        transform.position = shotStartPosition;

        // 2. Kill all momentum and physics
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        currentHeight = 0f;
        verticalVelocity = 0f;
        isMoving = false;

        // REMOVED the line that clears the drivenDistanceText here!

        // 3. Update friction and unlock the aim lines
        UpdateFriction();
        if (swingManager != null) swingManager.ResetSwingState();
    }
}