using System.Collections;
using UnityEngine;

public class UISlider : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform panelRect; // RectTransform is the UI version of a Transform

    [Header("Animation Settings")]
    public float slideDuration = 0.25f; // How fast it slides in seconds
    public Vector2 offScreenPosition = new Vector2(0, -1200f); // Tweak this depending on your canvas size
    public Vector2 onScreenPosition = new Vector2(0, 0f);

    private Coroutine currentAnimation;

    public void SlideIn()
    {
        // Make sure the panel is turned on before we try to move it
        panelRect.gameObject.SetActive(true);

        if (currentAnimation != null) StopCoroutine(currentAnimation);
        currentAnimation = StartCoroutine(SlideRoutine(onScreenPosition, false));
    }

    public void SlideOut()
    {
        if (currentAnimation != null) StopCoroutine(currentAnimation);

        // We pass 'true' here so it turns itself off AFTER reaching the bottom
        currentAnimation = StartCoroutine(SlideRoutine(offScreenPosition, true));
    }

    private IEnumerator SlideRoutine(Vector2 targetPosition, bool disableOnComplete)
    {
        Vector2 startPosition = panelRect.anchoredPosition;
        float timeElapsed = 0f;

        while (timeElapsed < slideDuration)
        {
            // CRITICAL: We use unscaledDeltaTime so the pause menu still animates while the game is frozen
            timeElapsed += Time.unscaledDeltaTime;

            // Calculate our percentage of completion (0.0 to 1.0)
            float t = timeElapsed / slideDuration;

            // Optional: This line adds "Smooth Step" easing so it doesn't look like a robot moving it
            t = t * t * (3f - 2f * t);

            // Move the panel
            panelRect.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);

            // Wait for the next frame
            yield return null;
        }

        // Snap to the exact final position just to be safe
        panelRect.anchoredPosition = targetPosition;

        if (disableOnComplete)
        {
            panelRect.gameObject.SetActive(false);
        }
    }
}