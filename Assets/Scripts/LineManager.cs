using UnityEngine;
using System.Collections.Generic;

public class LineManager : MonoBehaviour
{
    private List<LineRenderer> drawnLines = new List<LineRenderer>();
    private LineRenderer previewLine;
    private Vector3 startPoint;
    private bool isDrawing = false;
    private Camera mainCamera;
    private BoardManager boardManager;
    private GameManager gameManager;
    private LevelGenerator levelGenerator;
    private SpriteManager spriteManager;

    private Material normalLineMaterial;
    private const float BOARD_SIZE = 100f;
    private const float LINE_WIDTH = 0.1f; // Match JUNK_COLLIDER_RADIUS from SpriteManager
    private int maxLines = 4;

    public void Initialize(GameManager gameManager, BoardManager boardManager)
    {
        this.gameManager = gameManager;
        this.boardManager = boardManager;
        this.levelGenerator = FindObjectOfType<LevelGenerator>();
        this.spriteManager = FindObjectOfType<SpriteManager>();
        mainCamera = Camera.main;
        CreateLineMaterials();
        CreatePreviewLine();
    }

    private void CreateLineMaterials()
    {
        normalLineMaterial = new Material(Shader.Find("Sprites/Default"));
        if (spriteManager != null)
        {
            normalLineMaterial.color = spriteManager.GetLineColor();
            Sprite lineSprite = spriteManager.GetLineSprite();
            if (lineSprite != null)
            {
                normalLineMaterial.mainTexture = lineSprite.texture;
            }
        }
        else
        {
            normalLineMaterial.color = Color.black;
        }
    }

    private void CreatePreviewLine()
    {
        GameObject lineObj = new GameObject("Preview Line");
        lineObj.transform.SetParent(transform);
        previewLine = lineObj.AddComponent<LineRenderer>();
        SetupLineRenderer(previewLine);
        previewLine.gameObject.SetActive(false);
    }

    private void SetupLineRenderer(LineRenderer line)
    {
        line.startWidth = LINE_WIDTH;
        line.endWidth = LINE_WIDTH;
        line.material = new Material(normalLineMaterial);
        line.positionCount = 2;
    }

    private Vector3[] ExtendLine(Vector3 start, Vector3 end)
    {
        Vector3 direction = (end - start).normalized;
        
        // Используем размеры из LevelGenerator
        float boardWidth = 8f;
        float boardHeight = 12f;
        
        // Находим точки пересечения с границами поля
        float minX = -boardWidth/2;
        float maxX = boardWidth/2;
        float minY = -boardHeight/2;
        float maxY = boardHeight/2;

        float t1 = float.MinValue;
        float t2 = float.MaxValue;

        // Проверяем пересечения с вертикальными границами
        if (Mathf.Abs(direction.x) > 0.0001f)
        {
            float tx1 = (minX - start.x) / direction.x;
            float tx2 = (maxX - start.x) / direction.x;
            t1 = Mathf.Max(t1, Mathf.Min(tx1, tx2));
            t2 = Mathf.Min(t2, Mathf.Max(tx1, tx2));
        }

        // Проверяем пересечения с горизонтальными границами
        if (Mathf.Abs(direction.y) > 0.0001f)
        {
            float ty1 = (minY - start.y) / direction.y;
            float ty2 = (maxY - start.y) / direction.y;
            t1 = Mathf.Max(t1, Mathf.Min(ty1, ty2));
            t2 = Mathf.Min(t2, Mathf.Max(ty1, ty2));
        }

        // Вычисляем точки пересечения с границами
        Vector3 extendedStart = start + direction * t1;
        Vector3 extendedEnd = start + direction * t2;

        return new Vector3[] { extendedStart, extendedEnd };
    }

    private bool CheckIntersectionWithObjects(Vector3 start, Vector3 end)
    {
        Vector3[] points = ExtendLine(start, end);
        
        // Проверяем пересечение со всеми объектами
        foreach (var star in levelGenerator.GetStars())
        {
            if (IsLineIntersectingObject(points[0], points[1], star.transform.position, 0.25f))
            {
                return true;
            }
        }

        foreach (var junk in levelGenerator.GetJunks())
        {
            // Use the actual collider radius from the junk object
            float junkRadius = junk.GetComponent<CircleCollider2D>().radius;
            if (IsLineIntersectingObject(points[0], points[1], junk.transform.position, junkRadius))
            {
                return true;
            }
        }
        return false;
    }

    private bool IsPointInsideGameField(Vector3 point)
    {
        float boardWidth = 8f;
        float boardHeight = 12f;
        
        float minX = -boardWidth/2;
        float maxX = boardWidth/2;
        float minY = -boardHeight/2;
        float maxY = boardHeight/2;

        return point.x >= minX && point.x <= maxX && 
               point.y >= minY && point.y <= maxY;
    }

    public void StartDrawing(Vector3 position)
    {
        // Проверяем, находится ли точка внутри игрового поля
        if (!isDrawing && gameManager.CanDrawLine() && IsPointInsideGameField(position))
        {
            isDrawing = true;
            startPoint = position;
            previewLine.gameObject.SetActive(true);
            Vector3[] points = ExtendLine(startPoint, startPoint);
            previewLine.SetPosition(0, points[0]);
            previewLine.SetPosition(1, points[1]);
        }
    }

    public void UpdatePreview(Vector3 position)
    {
        if (isDrawing)
        {
            Vector3[] points = ExtendLine(startPoint, position);
            previewLine.SetPosition(0, points[0]);
            previewLine.SetPosition(1, points[1]);
            
            bool intersectsWithObject = CheckIntersectionWithObjects(startPoint, position);
            Color lineColor = intersectsWithObject ? Color.red : 
                (spriteManager != null ? spriteManager.GetLineColor() : Color.black);
            previewLine.startColor = lineColor;
            previewLine.endColor = lineColor;
        }
    }

    public void FinishDrawing(Vector3 endPoint)
    {
        if (isDrawing)
        {
            if (Vector3.Distance(startPoint, endPoint) > 0.1f && !CheckIntersectionWithObjects(startPoint, endPoint))
            {
                Vector3[] points = ExtendLine(startPoint, endPoint);
                GameObject lineObj = new GameObject("Drawn Line");
                lineObj.transform.SetParent(transform);
                LineRenderer line = lineObj.AddComponent<LineRenderer>();
                SetupLineRenderer(line);

                line.SetPosition(0, points[0]);
                line.SetPosition(1, points[1]);
                Color lineColor = spriteManager != null ? spriteManager.GetLineColor() : Color.black;
                line.startColor = lineColor;
                line.endColor = lineColor;

                drawnLines.Add(line);
                gameManager.OnLineDrawn();
            }

            isDrawing = false;
            previewLine.gameObject.SetActive(false);
        }
    }

    private bool IsLineIntersectingObject(Vector3 lineStart, Vector3 lineEnd, Vector3 objectPosition, float radius)
    {
        Vector2 line2DStart = new Vector2(lineStart.x, lineStart.y);
        Vector2 line2DEnd = new Vector2(lineEnd.x, lineEnd.y);
        Vector2 objectPos2D = new Vector2(objectPosition.x, objectPosition.y);

        Vector2 lineDirection = (line2DEnd - line2DStart).normalized;
        float projection = Vector2.Dot(objectPos2D - line2DStart, lineDirection);
        Vector2 nearestPoint = line2DStart + lineDirection * projection;

        return Vector2.Distance(objectPos2D, nearestPoint) < radius;
    }

    public void ClearLines()
    {
        foreach (var line in drawnLines)
        {
            if (line != null)
            {
                Destroy(line.gameObject);
            }
        }
        drawnLines.Clear();
        
        if (previewLine != null)
        {
            previewLine.gameObject.SetActive(false);
        }
        
        isDrawing = false;
    }

    public List<LineRenderer> GetDrawnLines()
    {
        return drawnLines;
    }

    public Vector3 GetWorldPosition(Vector2 screenPosition)
    {
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -mainCamera.transform.position.z));
        worldPosition.z = 0;
        return worldPosition;
    }

    public int GetAvailableLines()
    {
        return maxLines - drawnLines.Count;
    }

    public void SetMaxLines(int count)
    {
        maxLines = count;
    }

    public bool UndoLastLine()
    {
        if (drawnLines.Count > 0)
        {
            var lastLine = drawnLines[drawnLines.Count - 1];
            if (lastLine != null)
            {
                Destroy(lastLine.gameObject);
            }
            drawnLines.RemoveAt(drawnLines.Count - 1);
            return true;
        }
        return false;
    }
}