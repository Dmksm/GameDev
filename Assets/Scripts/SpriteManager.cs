using UnityEngine;

public class SpriteManager : MonoBehaviour
{
    private SpritesConfig spritesConfig;

    private void Awake()
    {
        LoadSpritesConfig();
    }

    private void LoadSpritesConfig()
    {
        spritesConfig = Resources.Load<SpritesConfig>("SpritesConfig");
        if (spritesConfig == null)
        {
            Debug.LogError("SpritesConfig not found in Resources folder!");
            return;
        }
        Debug.Log($"SpritesConfig loaded successfully. Star sprite null? {spritesConfig.starSprite == null}");
    }

    public void ApplyStarSprite(GameObject star)
    {
        Debug.Log("Starting ApplyStarSprite...");
        
        var collider = star.GetComponent<CircleCollider2D>();
        if (collider == null)
        {
            Debug.LogError("No CircleCollider2D found on star object!");
            return;
        }

        // Create default material
        Material material = new Material(Shader.Find("Sprites/Default"));
        material.color = spritesConfig != null ? spritesConfig.starColor : Color.white;

        // Try to use sprite if available
        if (spritesConfig != null && spritesConfig.starSprite != null)
        {
            var spriteRenderer = star.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = star.AddComponent<SpriteRenderer>();
            }

            spriteRenderer.sprite = spritesConfig.starSprite;
            spriteRenderer.color = spritesConfig.starColor;
            spriteRenderer.sortingOrder = 1;
            
            // Set a fixed scale to match the collider size
            float scale = 0.013f;
            star.transform.localScale = new Vector3(scale, scale, scale);
        }
        else
        {
            // Fallback to using MeshRenderer with colored material
            EnsureRenderer(star, material);
            star.transform.localScale = Vector3.one * (collider.radius * 2);
        }
    }

    public void ApplyJunkSprite(GameObject junk)
    {
        var collider = junk.GetComponent<CircleCollider2D>();
        if (collider == null)
        {
            Debug.LogError("No CircleCollider2D found on junk object!");
            return;
        }

        Material material = new Material(Shader.Find("Sprites/Default"));
        
        if (spritesConfig != null && spritesConfig.junkSprite != null)
        {
            var spriteRenderer = junk.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = spritesConfig.junkSprite;
            spriteRenderer.color = spritesConfig.junkColor;
            spriteRenderer.sortingOrder = 1;
        }
        else
        {
            material.color = spritesConfig != null ? spritesConfig.junkColor : Color.gray;
            EnsureRenderer(junk, material);
        }
        
        junk.transform.localScale = Vector3.one * (collider.radius * 2);
    }

    public void ApplyBoardSprite(GameObject board)
    {
        if (board.GetComponent<Renderer>() != null)
        {
            Material material = new Material(Shader.Find("Sprites/Default"));
            
            if (spritesConfig != null && spritesConfig.boardSprite != null)
            {
                material.mainTexture = spritesConfig.boardSprite.texture;
                material.color = spritesConfig.boardColor;
            }
            else
            {
                material.color = spritesConfig != null ? spritesConfig.boardColor : Color.white;
            }
            
            board.GetComponent<Renderer>().material = material;
        }
    }

    public void SetupLineRenderer(LineRenderer lineRenderer)
    {
        if (lineRenderer != null)
        {
            Material material = new Material(Shader.Find("Sprites/Default"));
            
            if (spritesConfig != null && spritesConfig.lineSprite != null)
            {
                material.mainTexture = spritesConfig.lineSprite.texture;
                material.color = spritesConfig.lineColor;
            }
            else
            {
                material.color = spritesConfig != null ? spritesConfig.lineColor : Color.black;
            }
            
            lineRenderer.material = material;
        }
    }

    private void EnsureRenderer(GameObject obj, Material material)
    {
        var renderer = obj.GetComponent<Renderer>();
        if (renderer == null)
        {
            renderer = obj.AddComponent<MeshRenderer>();
            obj.AddComponent<MeshFilter>().mesh = CreateCircleMesh(32);
        }
        renderer.material = material;
    }

    private Mesh CreateCircleMesh(int segments)
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[segments + 1];
        int[] triangles = new int[segments * 3];

        vertices[0] = Vector3.zero;
        for (int i = 0; i < segments; i++)
        {
            float angle = ((float)i / segments) * 2 * Mathf.PI;
            vertices[i + 1] = new Vector3(Mathf.Cos(angle) * 0.5f, Mathf.Sin(angle) * 0.5f, 0);
        }

        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = (i + 1) % segments + 1;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }
}
