using TMPro; // Make sure to include this for TextMeshPro
using UnityEngine;
using UnityEngine.InputSystem; // Required for the new Input System

public class ScorecardManager : MonoBehaviour
{
    [Header("UI Elements")]
    public UISlider scorecardPanelSlider;

    [Header("Course Info UI")]
    public TextMeshProUGUI courseNameText;
    public TextMeshProUGUI totalCourseParText;
    public TextMeshProUGUI totalCourseScoreText;
    public TextMeshProUGUI totalParText;
    public TextMeshProUGUI totalScoreText;

    // Arrays to hold your UI text elements
    public TextMeshProUGUI[] parTexts;
    public TextMeshProUGUI[] scoreTexts;

    private bool isScorecardActive = false;



    void Update()
    {
        // Toggle the scorecard when Tab is pressed using the new Input System
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            if (isScorecardActive)
            {
                scorecardPanelSlider.SlideOut();
            }
            else
            {
                scorecardPanelSlider.SlideIn();
            }
            isScorecardActive = !isScorecardActive;
        }
    }

    public void InitScorecard(GolfCourse course, int[] scoresHistory)
    {
        if (courseNameText != null) courseNameText.text = course.courseName;

        int totalPar = 0;
        int totalScore = 0;
        int currentPar = 0;

        for (int i = 0; i < parTexts.Length; i++)
        {
            if (i < course.holePrefabs.Length)
            {
                HoleData data = course.holePrefabs[i].GetComponent<HoleData>();
                parTexts[i].text = data.par.ToString();
                totalPar += data.par;
            }

            // CHECK HISTORY: If we have a recorded score for this hole, display it. 
            // Otherwise, display the blank "-"
            if (i < scoresHistory.Length && scoresHistory[i] > 0)
            {
                HoleData data = course.holePrefabs[i].GetComponent<HoleData>();
                scoreTexts[i].text = scoresHistory[i].ToString();
                totalScore += scoresHistory[i];
                currentPar += data.par;
            }
            else
            {
                scoreTexts[i].text = "";
            }
        }

        if (totalParText != null) totalParText.text = totalPar.ToString();
        if (totalCourseParText != null) totalCourseParText.text = "Par " + totalPar.ToString();
        if (totalScoreText != null) totalScoreText.text = totalScore.ToString();
        if (totalCourseScoreText != null)
        {
            if (totalScore - currentPar == 0)
            {
                totalCourseScoreText.text = "E";
            }
            else if (totalScore - currentPar > 0)
            {
                totalCourseScoreText.text = "+" + (totalScore - currentPar).ToString();
            }
            else
            {
                totalCourseScoreText.text = (totalScore - currentPar).ToString();
            }
               
        }
    }

}