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

    private Material normalLineMaterial;
    private const float BOARD_SIZE = 100f;

    public void Initialize(GameManager gameManager, BoardManager boardManager)
    {
        this.gameManager = gameManager;
        this.boardManager = boardManager;
        this.levelGenerator = Object.FindFirstObjectByType<LevelGenerator>();
        mainCamera = Camera.main;
        CreateLineMaterials();
        CreatePreviewLine();
    }

    private void CreateLineMaterials()
    {
        normalLineMaterial = new Material(Shader.Find("Sprites/Default"));
        normalLineMaterial.color = Color.black;
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
        line.startWidth = 0.1f;
        line.endWidth = 0.1f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.positionCount = 2;
    }

    private Vector3[] ExtendLine(Vector3 start, Vector3 end)
    {
        Vector3 direction = (end - start).normalized;
        Vector3 extendedStart = start - direction * BOARD_SIZE;
        Vector3 extendedEnd = start + direction * BOARD_SIZE;
        return new Vector3[] { extendedStart, extendedEnd };
    }

    private bool CheckIntersectionWithObjects(Vector3 start, Vector3 end)
    {
        Vector3[] points = ExtendLine(start, end);
        
        // Проверяем пересечение со всеми объектами
        foreach (var star in levelGenerator.GetStars())
        {
            if (IsLineIntersectingObject(points[0], points[1], star.transform.position, 0.5f))
            {
                return true;
            }
        }

        foreach (var junk in levelGenerator.GetJunks())
        {
            if (IsLineIntersectingObject(points[0], points[1], junk.transform.position, 0.5f))
            {
                return true;
            }
        }

        return false;
    }

    public void StartDrawing(Vector3 position)
    {
        if (!isDrawing && gameManager.CanDrawLine())
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
            previewLine.startColor = intersectsWithObject ? Color.red : Color.black;
            previewLine.endColor = intersectsWithObject ? Color.red : Color.black;
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
                line.startColor = Color.black;
                line.endColor = Color.black;

                drawnLines.Add(line);
                gameManager.OnLineDrawn();
            }

            isDrawing = false;
            previewLine.gameObject.SetActive(false);
        }
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
}
