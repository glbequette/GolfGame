using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CourseDatabase", menuName = "Golf/Course Database")]
public class CourseDatabase : ScriptableObject
{
    [Tooltip("Drag every CourseData file you make into this list!")]
    public List<GolfCourse> allCourses = new List<GolfCourse>();

    // A handy helper method to find a course by its name
    public GolfCourse GetCourseByName(string searchName)
    {
        foreach (GolfCourse course in allCourses)
        {
            if (course.courseName == searchName)
            {
                return course;
            }
        }

        Debug.LogError($"Could not find a course named {searchName} in the database!");
        return null;
    }
}