using UnityEngine;
using UnityEngine.UI; // Required for UI elements!

// This line forces Unity to add a RawImage component if there isn't one
[RequireComponent(typeof(RawImage))]
public class SwingMeterGradient : MonoBehaviour
{
    [Header("Colors")]
    public Color edgeColor = Color.red;
    public Color centerColor = Color.green;

    [Header("Gradient Settings")]
    [Tooltip("How wide the pure green 'perfect' zone is (0.0 to 1.0)")]
    [Range(0f, 1f)]
    public float perfectZoneSize = 0.15f;

    private int textureResolution = 256; // 256 pixels is plenty for a smooth UI gradient

    void Start()
    {
        GenerateGradient();
    }

    void GenerateGradient()
    {
        // Grab the RawImage component attached to this object
        RawImage img = GetComponent<RawImage>();

        // Create a new blank 1D texture (256 pixels wide, 1 pixel tall)
        Texture2D tex = new Texture2D(textureResolution, 1);
        tex.wrapMode = TextureWrapMode.Clamp; // Keeps the edges looking crisp

        for (int i = 0; i < textureResolution; i++)
        {
            // Find our current position along the bar from 0.0 (left) to 1.0 (right)
            float positionPercentage = (float)i / (textureResolution - 1);

            // Calculate how far this pixel is from the exact center (0.5)
            // This turns the math into a mirror, so both the left and right sides do the same thing!
            float distanceFromCenter = Mathf.Abs(positionPercentage - 0.5f) * 2f;

            // Apply our "Perfect Zone" buffer
            // If the pixel is inside the perfect zone, 'blend' is 0. If it's outside, it fades to 1.
            float blend = Mathf.InverseLerp(perfectZoneSize, 1f, distanceFromCenter);

            // Mix the two colors based on our math
            Color pixelColor = Color.Lerp(centerColor, edgeColor, blend);

            // Paint the pixel!
            tex.SetPixel(i, 0, pixelColor);
        }

        // Apply the painted pixels to the texture, and assign it to the UI
        tex.Apply();
        img.texture = tex;
    }
}