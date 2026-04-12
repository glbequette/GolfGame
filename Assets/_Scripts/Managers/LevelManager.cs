using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[System.Serializable]
public class GolfCourse
{
    public string courseName;
    public GameObject[] holePrefabs; // The 9 or 18 holes that make up this specific course
    public Sprite courseSprite;
}

public class LevelManager : MonoBehaviour
{

    [Header("Scorecard Data")]
    public static int[] playerScores;
    private List<GolfCourse> allCourses;

    // The "Memory" variable. The Main Menu will change this number before loading the scene.
    public static int selectedCourseIndex = 0;

    [Header("References")]
    public Transform golfBall;
    public Camera minimapCamera;
    public CameraController mainCameraController;

    [Header("Minimap Settings")]
    [Tooltip("Extra space around the edges so the hole doesn't touch the screen borders.")]
    public float minimapPadding = 15f;

    public TextMeshProUGUI holeNumberText; // <-- NEW: Slot for your UI Text
    public TextMeshProUGUI holeParText;
    public TextMeshProUGUI holeDistanceText;

    // --- STATIC GAME STATE ---
    public static int currentHoleIndex = 0;
    public static int currentMoney = 0;

    private GameObject currentSpawnedHole;

    public ScorecardManager scorecardManager;


    void Start()
    {
        if (CourseManager.Instance != null && CourseManager.Instance.database != null)
        {
            allCourses = CourseManager.Instance.database.allCourses;
        }
        SpawnCurrentHole();
    }

    void SpawnCurrentHole()
    {
        GolfCourse currentCourse = allCourses[selectedCourseIndex];

        if (currentHoleIndex == 0 || playerScores == null || playerScores.Length != currentCourse.holePrefabs.Length)
        {
            playerScores = new int[currentCourse.holePrefabs.Length];
        }

        scorecardManager.InitScorecard(currentCourse, playerScores);

        // 1. Check if we beat the whole course
        if (currentHoleIndex >= currentCourse.holePrefabs.Length)
        {
            int totalPlayerScore = 0;
            for (int i = 0; i < currentCourse.holePrefabs.Length; i++)
            {
                totalPlayerScore += playerScores[i];
            }
            OnRunCompleted(currentCourse.courseName, totalPlayerScore);
            Debug.Log("Course Complete! You win!");
            SceneManager.LoadScene("MainMenu"); // Send them back to the menu
            currentHoleIndex = 0; // Reset for the next run
            return;
        }

        // 2. Spawn the correct prefab exactly at 0,0,0
        currentSpawnedHole = Instantiate(currentCourse.holePrefabs[currentHoleIndex], Vector3.zero, Quaternion.identity);

        // 3. Get the blueprint data from the spawned hole
        HoleData data = currentSpawnedHole.GetComponent<HoleData>();

        // --- NEW: Pass the target hole and the scale to the ball! ---
        BallController ball = golfBall.GetComponent<BallController>();
        ball.targetHole = data.holeLocation;
        ball.yardsPerUnit = data.yardsPerUnit; // Keeps the math perfectly synced!

        // 4. Move the ball to the Tee
        golfBall.position = data.teeSpawnPoint.position;

        // Safety: kill any weird momentum the ball might have had
        Rigidbody2D rb = golfBall.GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // 5. Setup the Minimap
        minimapCamera.transform.position = new Vector3(data.minimapCenter.position.x, data.minimapCenter.position.y, -10f);
        minimapCamera.orthographicSize = data.minimapCameraSize;

        // 6. Tell the Main Camera where the new flag is
        mainCameraController.hole = data.holeLocation;

        // --- NEW: Update the UI Text ---
        // We add 1 to the index because programmers count from 0, but players count from 1!
        if (holeNumberText != null)
        {
            holeNumberText.text = $"{currentHoleIndex + 1}";
            holeParText.text = $"Par {data.par}";
            holeDistanceText.text = $"{data.yards} Yards";
        }

        ball.ResetForNewHole();
    }

    public void ProcessHoleScore(int par, int strokes)
    {
        if (strokes == 1)
        {
            PlayerStatsManager.Instance.AddHoleInOne();
        }
        else if (strokes!= 1 && strokes <= par - 2)
        {
            PlayerStatsManager.Instance.AddEagle(); // Or better (Albatross, etc.)
        }
        else if (strokes == par - 1)
        {
            PlayerStatsManager.Instance.AddBirdie();
        }
        else if (strokes == par)
        {
            PlayerStatsManager.Instance.AddPar();
        }
    }

    // We will call this when the ball enters the cup
    public void FinishHole()
    {
        // 1. Grab the data from the current hole and the player's ball
        HoleData currentHoleData = currentSpawnedHole.GetComponent<HoleData>();
        BallController ball = golfBall.GetComponent<BallController>();

        int par = currentHoleData.par;
        int strokes = ball.strokesTaken;

        // 2. Calculate the Score (Negative is under par/good, Positive is over par/bad)
        ProcessHoleScore(par, strokes);
        int score = strokes - par;
        int moneyEarned = 0;


        if (ball != null)
        {
            playerScores[currentHoleIndex] = ball.strokesTaken;
        }

            // 3. The Payout Structure
            if (score <= -3) moneyEarned = 150;      // Albatross (or better)
        else if (score == -2) moneyEarned = 100; // Eagle
        else if (score == -1) moneyEarned = 50;  // Birdie
        else if (score == 0) moneyEarned = 25;   // Par
        else if (score == 1) moneyEarned = 10;   // Bogey
        else moneyEarned = 5;                    // Double Bogey or worse

        // 4. Add it to the permanent wallet
        PlayerStatsManager.Instance.AddMoney(moneyEarned);
        Debug.Log($"You shot a {score}! Earned ${moneyEarned}. Total Wallet: ${currentMoney}");

        // 5. Move to the next hole and load the shop
        currentHoleIndex++;
        if (currentSpawnedHole != null)
        {
            Destroy(currentSpawnedHole);
        }
        SpawnCurrentHole();

    }

    public void OnRunCompleted(string courseID, int totalRunScore)
    {
        PlayerStatsManager.Instance.UpdateCourseBestScore(courseID, totalRunScore);
    }

}