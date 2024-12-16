using UnityEngine;

[CreateAssetMenu(fileName = "SpritesConfig", menuName = "Game/Sprites Configuration")]
public class SpritesConfig : ScriptableObject
{
    [Header("Sprites")]
    public Sprite starSprite;
    public Sprite junkSprite;
    public Sprite boardSprite;
    public Sprite lineSprite;

    [Header("Colors")]
    public Color starColor = new Color(1f, 0.92f, 0.016f, 1f);  // Bright yellow
    public Color junkColor = new Color(0.5f, 0.5f, 0.5f, 1f);   // Gray
    public Color boardColor = new Color(0.1f, 0.1f, 0.2f, 1f);  // Dark space blue
    public Color lineColor = Color.white;                        // White lines
}
