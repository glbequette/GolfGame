using UnityEngine;

public class HoleData : MonoBehaviour
{
    [Header("Hole Info")]
    [Tooltip("The Par for this hole (e.g., 3, 4, 5)")]
    public int par = 3;

    [Tooltip("Calculated automatically based on Tee and Hole distance!")]
    public int yards;

    [Header("Distance Settings")]
    [Tooltip("How many yards is 1 Unity Unit? Adjust this until the distances feel realistic.")]
    public float yardsPerUnit = 5f;

    [Header("Player & Goal")]
    [Tooltip("Where the ball starts on this hole")]
    public Transform teeSpawnPoint;
    [Tooltip("Where the goal/flag is located")]
    public Transform holeLocation;

    [Header("Minimap Settings")]
    [Tooltip("The exact center of this specific hole")]
    public Transform minimapCenter;
    [Tooltip("How far the minimap needs to zoom out to fit this hole (bigger number = more zoomed out)")]
    public float minimapCameraSize = 15f;

    // --- NEW: The Automatic Calculator ---
    // OnValidate runs instantly in the Unity Editor whenever you change a variable or move an object!
    private void OnValidate()
    {
        // Only calculate if both points actually exist so we don't get errors
        if (teeSpawnPoint != null && holeLocation != null)
        {
            // 1. Get the raw distance between the Tee and the Hole in Unity space
            float rawDistance = Vector2.Distance(teeSpawnPoint.position, holeLocation.position);

            // 2. Multiply it by your scale, and round it to a clean, whole number
            yards = Mathf.RoundToInt(rawDistance * yardsPerUnit);
        }
    }

}