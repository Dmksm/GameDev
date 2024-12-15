using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    private const float BOARD_WIDTH = 8f;
    private const float BOARD_HEIGHT = 12f;
    private const float MIN_OBJECT_SPACING = 1f;

    private List<GameObject> stars = new List<GameObject>();
    private List<GameObject> junks = new List<GameObject>();
    private GameObject board;

    private Texture2D starSprite;
    private Texture2D junkSprite;

    public void Initialize()
    {
        CreateBoard();
        CreateSprites();
    }

    private void CreateBoard()
    {
        board = GameObject.CreatePrimitive(PrimitiveType.Quad);
        board.transform.SetParent(transform);
        board.transform.localScale = new Vector3(BOARD_WIDTH, BOARD_HEIGHT, 1);
        board.transform.position = new Vector3(0, 0, 1);
        
        Material material = new Material(Shader.Find("Sprites/Default"));
        material.color = Color.white;
        board.GetComponent<Renderer>().material = material;
    }

    private void CreateSprites()
    {
        // Create star sprite
        starSprite = new Texture2D(64, 64);
        starSprite.filterMode = FilterMode.Bilinear;
        Color[] starPixels = new Color[64 * 64];
        
        // Create star shape
        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                float dx = x - 32;
                float dy = y - 32;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
                
                // Create 5-pointed star
                float starPoint = Mathf.Abs(((angle + 360) % 72) - 36) / 36;
                float starDist = Mathf.Lerp(12, 28, starPoint);
                
                // Add glow effect
                float glow = Mathf.Max(0, 1 - dist / 32);
                float star = dist < starDist ? 1 : 0;
                
                // Combine star and glow
                float brightness = Mathf.Max(star, glow * 0.5f);
                Color starColor = Color.Lerp(new Color(1f, 0.9f, 0.2f, 0), new Color(1f, 0.95f, 0.2f, 1), brightness);
                starPixels[y * 64 + x] = starColor;
            }
        }
        starSprite.SetPixels(starPixels);
        starSprite.Apply();

        // Create junk sprite
        junkSprite = new Texture2D(64, 64);
        junkSprite.filterMode = FilterMode.Bilinear;
        Color[] junkPixels = new Color[64 * 64];
        
        // Create asteroid/space junk shape
        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                float dx = x - 32;
                float dy = y - 32;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float angle = Mathf.Atan2(dy, dx);
                
                // Create irregular shape
                float noise = Mathf.PerlinNoise(angle * 2, Time.time * 0.1f) * 8;
                float radius = 20 + noise;
                
                // Add detail and texture
                float detail = Mathf.PerlinNoise(x * 0.2f, y * 0.2f) * 0.5f;
                float crater = dist < radius - 5 ? Mathf.PerlinNoise(x * 0.3f, y * 0.3f) * 0.3f : 0;
                
                float alpha = dist < radius ? 1 : 0;
                Color baseColor = new Color(0.3f, 0.3f, 0.35f, alpha);
                Color detailColor = new Color(0.4f + detail, 0.4f + detail, 0.45f + detail, alpha);
                Color craterColor = new Color(0.2f, 0.2f, 0.25f, alpha);
                
                Color finalColor = Color.Lerp(baseColor, detailColor, 0.5f);
                finalColor = Color.Lerp(finalColor, craterColor, crater);
                
                junkPixels[y * 64 + x] = finalColor;
            }
        }
        junkSprite.SetPixels(junkPixels);
        junkSprite.Apply();
    }

    public void GenerateLevel(int level)
    {
        ClearLevel();

        // Calculate number of objects based on level
        int numStars = CalculateStars(level);
        int numJunks = CalculateJunks(level);

        // Create list for all positions
        List<Vector2> positions = new List<Vector2>();
        
        // Generate random positions for all objects
        for (int i = 0; i < numStars + numJunks; i++)
        {
            Vector2 newPos;
            bool validPosition;
            int attempts = 0;
            const int MAX_ATTEMPTS = 100;

            do
            {
                validPosition = true;
                newPos = new Vector2(
                    Random.Range(-BOARD_WIDTH/2 + 1, BOARD_WIDTH/2 - 1),
                    Random.Range(-BOARD_HEIGHT/2 + 1, BOARD_HEIGHT/2 - 1)
                );

                // Check distance to all existing positions
                foreach (var pos in positions)
                {
                    if (Vector2.Distance(pos, newPos) < MIN_OBJECT_SPACING)
                    {
                        validPosition = false;
                        break;
                    }
                }

                attempts++;
                if (attempts >= MAX_ATTEMPTS)
                {
                    Debug.LogWarning("Could not find suitable position after " + MAX_ATTEMPTS + " attempts");
                    break;
                }
            }
            while (!validPosition);

            positions.Add(newPos);
        }

        // Create stars
        for (int i = 0; i < numStars; i++)
        {
            if (i < positions.Count)
            {
                GameObject star = CreateStar(positions[i]);
                stars.Add(star);
            }
        }

        // Create junks
        for (int i = 0; i < numJunks; i++)
        {
            if (i + numStars < positions.Count)
            {
                GameObject junk = CreateJunk(positions[i + numStars]);
                junks.Add(junk);
            }
        }
    }

    private int CalculateStars(int level)
    {
        // Progressive star calculation for 100 levels
        // Start with 1 star, gradually increase
        // Level 1: 1 star
        // Level 20: ~4 stars
        // Level 50: ~8 stars
        // Level 100: ~15 stars
        return Mathf.Max(1, Mathf.FloorToInt(1 + (level * 0.14f)));
    }

    private int CalculateJunks(int level)
    {
        // Progressive junk calculation for 100 levels
        // More aggressive scaling than stars
        // Level 1: 2 junks
        // Level 20: ~8 junks
        // Level 50: ~15 junks
        // Level 100: ~25 junks
        return Mathf.Max(2, Mathf.FloorToInt(2 + (level * 0.23f)));
    }

    private GameObject CreateStar(Vector2 position)
    {
        GameObject star = new GameObject("Star");
        star.transform.SetParent(transform);
        star.transform.position = new Vector3(position.x, position.y, 0);
        
        SpriteRenderer spriteRenderer = star.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = Sprite.Create(starSprite, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64);
        spriteRenderer.sortingOrder = 1;
        
        CircleCollider2D collider = star.AddComponent<CircleCollider2D>();
        collider.radius = 0.4f;
        
        // Add visual effects
        star.AddComponent<StarRotation>();
        star.AddComponent<StarGlow>();
        
        return star;
    }

    private GameObject CreateJunk(Vector2 position)
    {
        GameObject junk = new GameObject("Space Junk");
        junk.transform.SetParent(transform);
        junk.transform.position = new Vector3(position.x, position.y, 0);
        
        SpriteRenderer spriteRenderer = junk.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = Sprite.Create(junkSprite, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64);
        spriteRenderer.sortingOrder = 1;
        
        CircleCollider2D collider = junk.AddComponent<CircleCollider2D>();
        collider.radius = 0.3f;
        
        // Random rotation
        junk.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
        
        return junk;
    }

    private void ClearLevel()
    {
        foreach (var star in stars)
        {
            if (star != null)
            {
                Destroy(star);
            }
        }
        stars.Clear();

        foreach (var junk in junks)
        {
            if (junk != null)
            {
                Destroy(junk);
            }
        }
        junks.Clear();
    }

    public List<GameObject> GetStars()
    {
        return stars;
    }

    public List<GameObject> GetJunks()
    {
        return junks;
    }
}

public class StarRotation : MonoBehaviour
{
    public float rotationSpeed = 10f;

    private void Update()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
}
