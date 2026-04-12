using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CourseSelectManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Image courseImageDisplay;
    public TextMeshProUGUI courseTitleText;
    public TextMeshProUGUI courseBestScoreText;

    private int currentIndex = 0; // Starts at 0 (the first course)
    private List<GolfCourse> golfCourses;

    void Start()
    {
        if (CourseManager.Instance != null && CourseManager.Instance.database != null)
        {
            golfCourses = CourseManager.Instance.database.allCourses;
        }
        // Update the text to show the first course when the screen loads
        UpdateUI();
    }

    // Called by the Right Arrow Button
    public void NextCourse()
    {
        // Add 1 to the index
        currentIndex++;

        // If we go past the end of the list, loop back to 0
        if (currentIndex >= golfCourses.Count)
        {
            currentIndex = 0;
        }

        UpdateUI();
    }

    // Called by the Left Arrow Button
    public void PreviousCourse()
    {
        // Add 1 to the index
        currentIndex--;

        // If we go past the end of the list, loop back to 0
        if (currentIndex < 0)
        {
            currentIndex = golfCourses.Count - 1;
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        if (courseTitleText != null) courseTitleText.text = golfCourses[currentIndex].courseName;
        if (courseImageDisplay != null) courseImageDisplay.sprite = golfCourses[currentIndex].courseSprite;
        if (courseBestScoreText != null && PlayerStatsManager.Instance != null)
        {
            if (PlayerStatsManager.Instance.GetBestScoreForCourse(golfCourses[currentIndex].courseName) == 0)
            {
                courseBestScoreText.text = "Course Record: N/A";
            }
            else if (PlayerStatsManager.Instance.GetBestScoreForCourse(golfCourses[currentIndex].courseName) == CourseManager.GetCoursePar(golfCourses[currentIndex].courseName))
            {
                courseBestScoreText.text = "Course Record: " + PlayerStatsManager.Instance.GetBestScoreForCourse(golfCourses[currentIndex].courseName)
                    + " (E)";
            }
            else if (PlayerStatsManager.Instance.GetBestScoreForCourse(golfCourses[currentIndex].courseName) < CourseManager.GetCoursePar(golfCourses[currentIndex].courseName))
            {
                courseBestScoreText.text = "Course Record: " + PlayerStatsManager.Instance.GetBestScoreForCourse(golfCourses[currentIndex].courseName)
                    + " (" + (PlayerStatsManager.Instance.GetBestScoreForCourse(golfCourses[currentIndex].courseName) - CourseManager.GetCoursePar(golfCourses[currentIndex].courseName)) + ")";
            }
            else if (PlayerStatsManager.Instance.GetBestScoreForCourse(golfCourses[currentIndex].courseName) > CourseManager.GetCoursePar(golfCourses[currentIndex].courseName))
            {
                courseBestScoreText.text = "Course Record: " + PlayerStatsManager.Instance.GetBestScoreForCourse(golfCourses[currentIndex].courseName)
                    + " (+" + (PlayerStatsManager.Instance.GetBestScoreForCourse(golfCourses[currentIndex].courseName) - CourseManager.GetCoursePar(golfCourses[currentIndex].courseName)) + ")";
            }
        }
    }

    // Called by the Play Button
    public void PlaySelectedCourse()
    {
        // --- THE NEW ARCHITECTURE ---
        // 2. Tell the LevelManager which course index we are looking at in the UI
        LevelManager.selectedCourseIndex = currentIndex;

        // 3. ALWAYS load the exact same "Gameplay" scene!
        SceneManager.LoadScene("GolfCourse");
    }
}