using UnityEngine;

public class CourseManager : MonoBehaviour
{
    // The Singleton instance
    public static CourseManager Instance { get; private set; }

    [Header("The Master Database")]
    // Drag your MainCourseDatabase file here in the inspector
    public CourseDatabase database;

    private void Awake()
    {
        // Set up the Singleton so it survives scene loads
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // A global shortcut to get a course from anywhere
    public static GolfCourse GetCourse(string courseName)
    {
        if (Instance != null && Instance.database != null)
        {
            return Instance.database.GetCourseByName(courseName);
        }
        return null;
    }

    public static int GetCoursePar(string courseName)
    {
        int totalPar = 0;
        GolfCourse golfCourse = GetCourse(courseName);

        for (int i = 0; i < golfCourse.holePrefabs.Length; i++)
        {
            HoleData data = golfCourse.holePrefabs[i].GetComponent<HoleData>();
            totalPar += data.par;
        }

            return totalPar;
    }
}