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

    public void Initialize()
    {
        spritesConfig = Resources.Load<SpritesConfig>("SpritesConfig");
        if (spritesConfig == null)
        {
            Debug.LogError("Failed to load SpritesConfig!");
        }
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

        // Remove any existing renderers to avoid conflicts
        var existingSprite = star.GetComponent<SpriteRenderer>();
        var existingMesh = star.GetComponent<MeshRenderer>();
        
        if (existingSprite != null)
        {
            Debug.Log("Removing existing SpriteRenderer");
            Destroy(existingSprite);
        }
        if (existingMesh != null)
        {
            Debug.Log("Removing existing MeshRenderer");
            Destroy(existingMesh);
            var meshFilter = star.GetComponent<MeshFilter>();
            if (meshFilter != null) Destroy(meshFilter);
        }

        Debug.Log($"SpritesConfig null? {spritesConfig == null}");
        if (spritesConfig != null)
        {
            Debug.Log($"Star sprite null? {spritesConfig.starSprite == null}");
            Debug.Log($"Star color: {spritesConfig.starColor}");
        }

        // Try to use sprite if available
        if (spritesConfig != null && spritesConfig.starSprite != null)
        {
            Debug.Log("Applying sprite renderer setup");
            var spriteRenderer = star.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = spritesConfig.starSprite;
            spriteRenderer.color = spritesConfig.starColor;
            spriteRenderer.sortingOrder = 1;
            
            float scale = 0.013f;
            star.transform.localScale = new Vector3(scale, scale, scale);
            Debug.Log($"Applied sprite with scale: {scale}");
        }
        else
        {
            Debug.Log("Falling back to mesh renderer setup");
            Material material = new Material(Shader.Find("Sprites/Default"));
            material.color = spritesConfig != null ? spritesConfig.starColor : Color.white;
            EnsureRenderer(star, material);
            star.transform.localScale = Vector3.one * (collider.radius * 2);
            Debug.Log($"Applied mesh renderer with scale: {collider.radius * 2}");
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
        Debug.Log("Starting ApplyBoardSprite...");
        if (board.GetComponent<Renderer>() != null)
        {
            Material material = new Material(Shader.Find("Sprites/Default"));
            
            Debug.Log($"SpritesConfig null? {spritesConfig == null}");
            if (spritesConfig != null)
            {
                Debug.Log($"Board sprite null? {spritesConfig.boardSprite == null}");
                Debug.Log($"Board color: {spritesConfig.boardColor}");
                
                if (spritesConfig.boardSprite != null)
                {
                    material.mainTexture = spritesConfig.boardSprite.texture;
                }
                material.color = spritesConfig.boardColor;
            }
            else
            {
                Debug.LogWarning("SpritesConfig is null, using default white color");
                material.color = Color.white;
            }
            
            var renderer = board.GetComponent<Renderer>();
            renderer.material = material;
            Debug.Log($"Applied material to board. Color: {material.color}");
        }
        else
        {
            Debug.LogError("Board object has no Renderer component!");
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
        Debug.Log($"EnsureRenderer for {obj.name}");
        var meshFilter = obj.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.Log("Adding MeshFilter");
            meshFilter = obj.AddComponent<MeshFilter>();
            meshFilter.mesh = Resources.GetBuiltinResource<Mesh>("Quad.fbx");
        }

        var meshRenderer = obj.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            Debug.Log("Adding MeshRenderer");
            meshRenderer = obj.AddComponent<MeshRenderer>();
        }
        meshRenderer.material = material;
        Debug.Log($"Set material with color: {material.color}");
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
