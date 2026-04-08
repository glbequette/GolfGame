using UnityEngine;
using System.Collections; // <-- NEW: Required for Coroutines!

public class HoleLogic : MonoBehaviour
{
    [Header("Cup Physics")]
    public float maxCaptureSpeed = 3.5f;

    // NEW: Prevents the trigger from firing 100 times while the ball is sliding in
    private bool isHoleComplete = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isHoleComplete) return; // If we are already sinking the ball, ignore!

        if (other.CompareTag("Player"))
        {
            BallController ball = other.GetComponent<BallController>();

            if (ball != null)
            {
                if (ball.currentHeight <= 0.05f && ball.rb.linearVelocity.magnitude <= maxCaptureSpeed)
                {
                    // Start the smooth animation instead of instantly snapping!
                    StartCoroutine(SmoothSinkAnimation(ball));
                }
                else if (ball.currentHeight <= 0.05f && ball.rb.linearVelocity.magnitude > maxCaptureSpeed)
                {
                    Debug.Log("Lipped out! Speed was: " + ball.rb.linearVelocity.magnitude);
                }
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (isHoleComplete) return;

        if (other.CompareTag("Player"))
        {
            BallController ball = other.GetComponent<BallController>();

            if (ball != null && ball.currentHeight <= 0.05f && ball.rb.linearVelocity.magnitude <= maxCaptureSpeed)
            {
                StartCoroutine(SmoothSinkAnimation(ball));
            }
        }
    }

    // --- NEW: The Smooth Animation (Updated for modern Unity Physics) ---
    private IEnumerator SmoothSinkAnimation(BallController ball)
    {
        isHoleComplete = true;

        ball.isSinking = true;

        // 1. Turn off the ball's physics using the new bodyType system
        ball.rb.linearVelocity = Vector2.zero;
        ball.rb.bodyType = RigidbodyType2D.Kinematic; // <-- FIXED WARNING HERE

        if (ball.lineRenderer != null) ball.lineRenderer.enabled = false;
        if (ball.minimapLineRenderer != null) ball.minimapLineRenderer.enabled = false;

        // 2. Setup the animation points
        Vector3 startPos = ball.transform.position;
        Vector3 targetPos = transform.position;

        float duration = 0.25f;
        float elapsed = 0f;

        // 3. Smoothly slide the ball to the center frame-by-frame
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            ball.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            yield return null;
        }

        // 4. Ensure it ends exactly dead-center
        ball.transform.position = targetPos;

        // --- NEW: The Celebration Delay! ---
        // Wait for 1.5 seconds so the player can bask in their glory
        yield return new WaitForSeconds(1.5f);

        // 5. Turn physics back on to "Dynamic" so the ball works normally on the next hole!
        ball.rb.bodyType = RigidbodyType2D.Dynamic;

        // 6. Tell the manager we are done
        FindAnyObjectByType<LevelManager>().FinishHole();

        isHoleComplete = false;
    }
}