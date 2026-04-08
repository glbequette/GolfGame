using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target References")]
    public Transform ball;
    public Transform hole;

    [Header("Camera Settings")]
    public float smoothSpeed = 5f;
    public float lookAheadDistance = 3f;

    [Tooltip("How close the ball needs to be for the camera to center on the hole")]
    public float holeSnapDistance = 7f; // <-- NEW: The trigger distance

    private float currentYOffset;

    void LateUpdate()
    {
        if (ball == null || hole == null) return;

        Vector3 targetPosition;

        // Measure the distance between the ball and the hole
        float distanceToHole = Vector2.Distance(ball.position, hole.position);

        // If the ball is close enough to the hole...
        if (distanceToHole <= holeSnapDistance)
        {
            // Set the target exactly to the hole's center (keep Z at -10 for the camera)
            targetPosition = new Vector3(hole.position.x, hole.position.y, -10f);
        }
        else
        {
            // Otherwise, use our normal "Look Ahead" logic
            float targetYOffset = (ball.position.y < hole.position.y) ? lookAheadDistance : -lookAheadDistance;
            currentYOffset = Mathf.Lerp(currentYOffset, targetYOffset, Time.deltaTime * smoothSpeed);

            targetPosition = new Vector3(ball.position.x, ball.position.y + currentYOffset, -10f);
        }

        // Smoothly glide the camera to whatever the chosen target is
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);
    }
}