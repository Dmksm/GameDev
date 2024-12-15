using UnityEngine;

public class StarGlow : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private float pulseSpeed = 2f;
    private float minScale = 0.9f;
    private float maxScale = 1.1f;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    void Update()
    {
        // Smooth pulsing effect
        float scale = Mathf.Lerp(minScale, maxScale, (Mathf.Sin(Time.time * pulseSpeed) + 1) * 0.5f);
        transform.localScale = new Vector3(scale, scale, 1);
        
        // Subtle color variation
        float colorPulse = (Mathf.Sin(Time.time * pulseSpeed * 1.5f) + 1) * 0.5f;
        spriteRenderer.color = Color.Lerp(
            new Color(1f, 0.9f, 0.2f, 1f),
            new Color(1f, 1f, 0.5f, 1f),
            colorPulse
        );
    }
}
