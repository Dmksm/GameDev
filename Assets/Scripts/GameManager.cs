using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    [Header("Game Objects")]
    public List<GameObject> stars = new List<GameObject>();
    public List<GameObject> cosmicJunks = new List<GameObject>();
    public LineRenderer linePrefab;
    public GameObject boardObject;
    public GameObject starPrefab; // Assuming you have a star prefab
    public GameObject junkPrefab; // Assuming you have a junk prefab

    [Header("UI References")]
    private Canvas gameCanvas;
    private TextMeshProUGUI linesCounterText;
    private GameObject gameOverPanel;
    private TextMeshProUGUI gameOverText;
    private Button restartButton;
    private Button undoButton;

    [Header("Game Settings")]
    public int requiredLines = 3;
    public float lineWidth = 0.1f;
    public float previewLineWidth = 0.05f;

    private List<LineRenderer> linesDrawn = new List<LineRenderer>();
    private LineRenderer previewLine;
    private LineRenderer currentLine;
    private int linesCount = 0;
    private Camera mainCamera;
    private Vector2 startPoint;
    private bool isDragging = false;
    private BoxCollider2D boardBounds;
    private int remainingLines;

    void Start()
    {
        mainCamera = Camera.main;
        Debug.Log("GameManager started");
        boardBounds = boardObject.GetComponent<BoxCollider2D>();
        if (boardBounds == null)
        {
            Debug.LogError("Please add a BoxCollider2D to your board object!");
        }
        remainingLines = requiredLines;
        InitializeUI();
        GenerateLevel(); // Generate initial level
        UpdateUI();
    }

    private void InitializeUI()
    {
        // Find or create canvas
        gameCanvas = FindObjectOfType<Canvas>();
        if (gameCanvas == null)
        {
            GameObject canvasObj = new GameObject("Game Canvas");
            gameCanvas = canvasObj.AddComponent<Canvas>();
            gameCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            // Add Canvas Scaler
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
            // Add Graphic Raycaster
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Create lines counter
        GameObject counterObj = new GameObject("Lines Counter");
        counterObj.transform.SetParent(gameCanvas.transform, false);
        linesCounterText = counterObj.AddComponent<TextMeshProUGUI>();
        
        // Set counter properties
        RectTransform counterRect = counterObj.GetComponent<RectTransform>();
        counterRect.anchorMin = new Vector2(0.5f, 1f);
        counterRect.anchorMax = new Vector2(0.5f, 1f);
        counterRect.pivot = new Vector2(0.5f, 1f);
        counterRect.sizeDelta = new Vector2(300, 80);
        counterRect.anchoredPosition = new Vector2(0, -20);
        
        linesCounterText.fontSize = 48;
        linesCounterText.fontStyle = FontStyles.Bold;
        linesCounterText.alignment = TextAlignmentOptions.Center;
        linesCounterText.outlineWidth = 0.2f;
        linesCounterText.outlineColor = new Color(0, 0, 0, 1);

        // Create undo button
        GameObject undoObj = new GameObject("Undo Button");
        undoObj.transform.SetParent(gameCanvas.transform, false);
        
        // Add button image
        Image undoImage = undoObj.AddComponent<Image>();
        undoImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        // Add button component
        undoButton = undoObj.AddComponent<Button>();
        undoButton.targetGraphic = undoImage;
        
        // Position the button to the right of the counter
        RectTransform undoRect = undoObj.GetComponent<RectTransform>();
        undoRect.anchorMin = new Vector2(1f, 1f);
        undoRect.anchorMax = new Vector2(1f, 1f);
        undoRect.pivot = new Vector2(1f, 1f);
        undoRect.sizeDelta = new Vector2(80, 80);
        undoRect.anchoredPosition = new Vector2(-20, -20);
        
        // Add button text
        GameObject undoTextObj = new GameObject("Undo Text");
        undoTextObj.transform.SetParent(undoObj.transform, false);
        TextMeshProUGUI undoText = undoTextObj.AddComponent<TextMeshProUGUI>();
        
        RectTransform undoTextRect = undoTextObj.GetComponent<RectTransform>();
        undoTextRect.anchorMin = Vector2.zero;
        undoTextRect.anchorMax = Vector2.one;
        undoTextRect.offsetMin = Vector2.zero;
        undoTextRect.offsetMax = Vector2.zero;
        
        undoText.text = "↺";
        undoText.fontSize = 48;
        undoText.alignment = TextAlignmentOptions.Center;
        undoText.color = Color.white;
        undoText.fontStyle = FontStyles.Bold;
        
        // Add button click listener
        undoButton.onClick.AddListener(UndoLastLine);

        // Create game over panel
        gameOverPanel = new GameObject("Game Over Panel");
        gameOverPanel.transform.SetParent(gameCanvas.transform, false);
        
        Image panelImage = gameOverPanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.85f);
        
        RectTransform panelRect = gameOverPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(300, 200);
        panelRect.anchoredPosition = Vector2.zero;
        
        // Create game over text
        GameObject textObj = new GameObject("Game Over Text");
        textObj.transform.SetParent(gameOverPanel.transform, false);
        gameOverText = textObj.AddComponent<TextMeshProUGUI>();
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0.5f);
        textRect.anchorMax = new Vector2(1, 1f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        gameOverText.fontSize = 42;
        gameOverText.alignment = TextAlignmentOptions.Center;
        gameOverText.fontStyle = FontStyles.Bold;

        // Create restart button
        GameObject buttonObj = new GameObject("Restart Button");
        buttonObj.transform.SetParent(gameOverPanel.transform, false);
        
        // Add button image
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        // Add button component
        restartButton = buttonObj.AddComponent<Button>();
        restartButton.targetGraphic = buttonImage;
        
        // Position the button
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.2f, 0f);
        buttonRect.anchorMax = new Vector2(0.8f, 0.4f);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;
        
        // Add button text
        GameObject buttonTextObj = new GameObject("Button Text");
        buttonTextObj.transform.SetParent(buttonObj.transform, false);
        TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
        
        RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.offsetMin = Vector2.zero;
        buttonTextRect.offsetMax = Vector2.zero;
        
        buttonText.text = "RESTART";
        buttonText.fontSize = 32;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.color = Color.white;
        
        // Add button click listener
        restartButton.onClick.AddListener(RestartGame);
        
        // Initially hide game over panel
        gameOverPanel.SetActive(false);
    }

    private void UpdateUI()
    {
        if (linesCounterText != null)
        {
            linesCounterText.text = $"LINES LEFT: {remainingLines}";
            
            // Change color based on remaining lines with more vibrant colors
            if (remainingLines <= 1)
            {
                linesCounterText.color = new Color(1f, 0.2f, 0.2f); // Bright red
            }
            else if (remainingLines <= 2)
            {
                linesCounterText.color = new Color(1f, 0.9f, 0f); // Bright yellow
            }
            else
            {
                linesCounterText.color = new Color(0.4f, 1f, 0.4f); // Bright green
            }

            // Add outline to make text more visible
            linesCounterText.outlineWidth = 0.2f;
            linesCounterText.outlineColor = new Color(0, 0, 0, 1);
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && remainingLines > 0)
        {
            StartDrawingPreview();
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            UpdatePreview();
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            FinishLine();
            if (remainingLines <= 0)
            {
                CheckWinCondition();
            }
        }
    }

    private void UndoLastLine()
    {
        if (linesDrawn.Count > 0 && remainingLines < requiredLines)
        {
            LineRenderer lastLine = linesDrawn[linesDrawn.Count - 1];
            linesDrawn.RemoveAt(linesDrawn.Count - 1);
            Destroy(lastLine.gameObject);
            remainingLines++;
            UpdateUI();
        }
    }

    private void StartDrawingPreview()
    {
        isDragging = true;
        startPoint = GetInputPosition();
        
        previewLine = Instantiate(linePrefab);
        previewLine.startWidth = previewLineWidth;
        previewLine.endWidth = previewLineWidth;
        previewLine.positionCount = 2;
        previewLine.SetPosition(0, startPoint);
        previewLine.SetPosition(1, startPoint);
    }

    private void UpdatePreview()
    {
        if (previewLine != null)
        {
            Vector2 currentPoint = GetInputPosition();
            Vector2 direction = (currentPoint - startPoint).normalized;
            Vector2 intersectStart, intersectEnd;
            FindBoardIntersections(startPoint, direction, out intersectStart, out intersectEnd);

            // Check if the extended line would be valid
            bool isValid = !WouldLineIntersectObjects(intersectStart, intersectEnd);
            
            if (isValid)
            {
                previewLine.enabled = true;
                previewLine.SetPosition(0, intersectStart);
                previewLine.SetPosition(1, intersectEnd);
            }
            else
            {
                previewLine.enabled = false;
            }
        }
    }

    private void FinishLine()
    {
        if (previewLine != null && previewLine.enabled)
        {
            Vector2 endPoint = GetInputPosition();
            Vector2 direction = (endPoint - startPoint).normalized;
            Vector2 intersectStart, intersectEnd;
            FindBoardIntersections(startPoint, direction, out intersectStart, out intersectEnd);

            if (!WouldLineIntersectObjects(intersectStart, intersectEnd))
            {
                currentLine = Instantiate(linePrefab);
                currentLine.startWidth = lineWidth;
                currentLine.endWidth = lineWidth;
                currentLine.positionCount = 2;
                currentLine.SetPosition(0, intersectStart);
                currentLine.SetPosition(1, intersectEnd);

                linesDrawn.Add(currentLine);
                remainingLines--;
                UpdateUI();
            }

            Destroy(previewLine.gameObject);
            previewLine = null;
            isDragging = false;
        }
    }

    private bool IsPointOnBoardEdge(Vector2 point)
    {
        BoxCollider2D boardCollider = boardObject.GetComponent<BoxCollider2D>();
        Vector2 boardMin = (Vector2)boardObject.transform.position - boardCollider.size / 2;
        Vector2 boardMax = (Vector2)boardObject.transform.position + boardCollider.size / 2;
        float epsilon = 0.01f; // Small threshold for edge detection

        // Check if point is on any edge of the board
        return (Mathf.Abs(point.x - boardMin.x) < epsilon ||
                Mathf.Abs(point.x - boardMax.x) < epsilon ||
                Mathf.Abs(point.y - boardMin.y) < epsilon ||
                Mathf.Abs(point.y - boardMax.y) < epsilon);
    }

    private Vector2 FindMaxSafePoint(Vector2 start, Vector2 end)
    {
        Vector2 direction = (end - start).normalized;
        float minDistance = 0;
        float maxDistance = Vector2.Distance(start, end);
        float currentDistance = maxDistance;
        Vector2 bestPoint = start;
        
        // Binary search for maximum safe distance
        for (int i = 0; i < 10; i++) // 10 iterations should be enough for precision
        {
            currentDistance = (minDistance + maxDistance) / 2;
            Vector2 testPoint = start + direction * currentDistance;
            
            if (!WouldLineIntersectObjects(start, testPoint))
            {
                bestPoint = testPoint;
                minDistance = currentDistance;
            }
            else
            {
                maxDistance = currentDistance;
            }
        }
        
        return bestPoint;
    }

    private bool WouldLineIntersectObjects(Vector2 lineStart, Vector2 lineEnd)
    {
        // Reduce collision radius to 75% of actual radius for better gameplay
        const float radiusMultiplier = 0.75f;

        // Check intersection with stars
        foreach (GameObject star in stars)
        {
            CircleCollider2D collider = star.GetComponent<CircleCollider2D>();
            if (collider != null && LineIntersectsCircle(lineStart, lineEnd, star.transform.position, collider.radius * radiusMultiplier))
            {
                return true;
            }
        }

        // Check intersection with cosmic junk
        foreach (GameObject junk in cosmicJunks)
        {
            CircleCollider2D collider = junk.GetComponent<CircleCollider2D>();
            if (collider != null && LineIntersectsCircle(lineStart, lineEnd, junk.transform.position, collider.radius * radiusMultiplier))
            {
                return true;
            }
        }

        return false;
    }

    private bool LineIntersectsCircle(Vector2 lineStart, Vector2 lineEnd, Vector2 circleCenter, float radius)
    {
        // Convert line segment to vector form
        Vector2 lineDirection = lineEnd - lineStart;
        Vector2 startToCenter = circleCenter - lineStart;

        // Project circle center onto line
        float projectionLength = Vector2.Dot(startToCenter, lineDirection.normalized);
        
        // Find closest point on line segment to circle center
        Vector2 closestPoint;
        if (projectionLength < 0)
        {
            closestPoint = lineStart;
        }
        else if (projectionLength > lineDirection.magnitude)
        {
            closestPoint = lineEnd;
        }
        else
        {
            closestPoint = lineStart + lineDirection.normalized * projectionLength;
        }

        // Check if closest point is within circle radius
        return Vector2.Distance(closestPoint, circleCenter) <= radius;
    }

    private void FindBoardIntersections(Vector2 point, Vector2 direction, out Vector2 start, out Vector2 end)
    {
        Bounds bounds = boardBounds.bounds;
        float left = bounds.min.x;
        float right = bounds.max.x;
        float bottom = bounds.min.y;
        float top = bounds.max.y;

        List<Vector2> intersections = new List<Vector2>();

        // Handle vertical line case
        if (Mathf.Abs(direction.x) < 0.0001f)
        {
            intersections.Add(new Vector2(point.x, bottom));
            intersections.Add(new Vector2(point.x, top));
        }
        // Handle horizontal line case
        else if (Mathf.Abs(direction.y) < 0.0001f)
        {
            intersections.Add(new Vector2(left, point.y));
            intersections.Add(new Vector2(right, point.y));
        }
        else
        {
            // Check all four edges
            float t = (left - point.x) / direction.x;
            float y = point.y + t * direction.y;
            if (y >= bottom && y <= top) intersections.Add(new Vector2(left, y));

            t = (right - point.x) / direction.x;
            y = point.y + t * direction.y;
            if (y >= bottom && y <= top) intersections.Add(new Vector2(right, y));

            t = (bottom - point.y) / direction.y;
            float x = point.x + t * direction.x;
            if (x >= left && x <= right) intersections.Add(new Vector2(x, bottom));

            t = (top - point.y) / direction.y;
            x = point.x + t * direction.x;
            if (x >= left && x <= right) intersections.Add(new Vector2(x, top));
        }

        // Sort intersections by distance from point
        intersections.Sort((a, b) => Vector2.Distance(point, a).CompareTo(Vector2.Distance(point, b)));

        if (intersections.Count >= 2)
        {
            start = intersections[0];
            end = intersections[1];
        }
        else
        {
            // Fallback
            start = point;
            end = point + direction;
            Debug.LogWarning("Could not find board intersections!");
        }
    }

    private Vector2 GetInputPosition()
    {
        Vector3 inputPos;
        if (Input.touchCount > 0)
        {
            inputPos = Input.GetTouch(0).position;
        }
        else
        {
            inputPos = Input.mousePosition;
        }
        
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(inputPos);
        return new Vector2(worldPos.x, worldPos.y);
    }

    private void CheckWinCondition()
    {
        bool hasWon = true;
        foreach (GameObject star in stars)
        {
            if (star != null)
            {
                // Get all cosmic junks that are too close to the star
                foreach (GameObject junk in cosmicJunks)
                {
                    if (junk != null && junk.activeSelf)
                    {
                        // Check if there's a line between the star and this junk
                        if (!IsPathBlockedByLines(star.transform.position, junk.transform.position))
                        {
                            hasWon = false;
                            break;
                        }
                    }
                }
                
                if (!hasWon) break;
            }
        }

        ShowGameResult(hasWon);
    }

    private void ShowGameResult(bool hasWon)
    {
        if (gameOverPanel != null && gameOverText != null)
        {
            gameOverPanel.SetActive(true);
            if (hasWon)
            {
                gameOverText.text = "YOU WIN!";
                gameOverText.color = new Color(0.2f, 1f, 0.2f); // Light green for winning
            }
            else
            {
                gameOverText.text = "GAME OVER";
                gameOverText.color = Color.white;
            }
        }
    }

    private List<GameObject> FindArea(GameObject startObject)
    {
        List<GameObject> area = new List<GameObject>();
        Queue<GameObject> objectsToCheck = new Queue<GameObject>();
        
        objectsToCheck.Enqueue(startObject);

        while (objectsToCheck.Count > 0)
        {
            GameObject currentObject = objectsToCheck.Dequeue();
            area.Add(currentObject);

            foreach (GameObject obj in cosmicJunks)
            {
                if (!area.Contains(obj) && !IsPathBlockedByLines(currentObject.transform.position, obj.transform.position))
                {
                    objectsToCheck.Enqueue(obj);
                }
            }
        }

        return area;
    }

    private bool IsPathBlockedByLines(Vector2 from, Vector2 to)
    {
        foreach (LineRenderer line in linesDrawn)
        {
            Vector2 lineStart = line.GetPosition(0);
            Vector2 lineEnd = line.GetPosition(1);

            if (DoLineSegmentsIntersect(from, to, lineStart, lineEnd))
            {
                return true;
            }
        }
        return false;
    }

    private bool DoLineSegmentsIntersect(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        float denominator = (p4.y - p3.y) * (p2.x - p1.x) - (p4.x - p3.x) * (p2.y - p1.y);

        if (denominator == 0)
        {
            return false;
        }

        float ua = ((p4.x - p3.x) * (p1.y - p3.y) - (p4.y - p3.y) * (p1.x - p3.x)) / denominator;
        float ub = ((p2.x - p1.x) * (p1.y - p3.y) - (p2.y - p1.y) * (p1.x - p3.x)) / denominator;

        return ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1;
    }

    private void RestartGame()
    {
        // Clear all drawn lines
        foreach (LineRenderer line in linesDrawn)
        {
            if (line != null)
            {
                Destroy(line.gameObject);
            }
        }
        linesDrawn.Clear();

        // Reset game state
        remainingLines = requiredLines;
        isDragging = false;
        if (previewLine != null)
        {
            Destroy(previewLine.gameObject);
            previewLine = null;
        }

        // Hide game over panel
        gameOverPanel.SetActive(false);

        // Generate a new level
        GenerateLevel();

        // Update UI
        UpdateUI();
    }

 [Header("Level Generation Settings")]
    public int minStars = 1;
    public int maxStars = 3;
    public int minJunks = 1;
    public int maxJunks = 5;
    public float minObjectSize = 0.5f;
    public float maxObjectSize = 1.5f;

    private void GenerateLevel()
    {
        ClearLevel();
        
        List<Vector2[]> solutionLines = GenerateValidSolution();
        List<Polygon> areas = CalculateAreas(solutionLines);
        PlaceObjectsInAreas(areas);
        
        // Remove temporary solution lines
        foreach (var line in solutionLines)
        {
            Debug.DrawLine(line[0], line[1], Color.yellow, 5f); // Visualize solution in Scene view
        }

        remainingLines = requiredLines;
        UpdateUI();
    }

    private void ClearLevel()
    {
        foreach (var star in stars)
            Destroy(star);
        foreach (var junk in cosmicJunks)
            Destroy(junk);
        foreach (var line in linesDrawn)
            Destroy(line.gameObject);
            
        stars.Clear();
        cosmicJunks.Clear();
        linesDrawn.Clear();
    }

    private List<Vector2[]> GenerateValidSolution()
    {
        List<Vector2[]> solution = new List<Vector2[]>();
        
        for (int i = 0; i < requiredLines; i++)
        {
            Vector2 start, end;
            do
            {
                start = GetRandomPointOnBoardEdge();
                end = GetRandomPointOnBoardEdge();
            } while (!IsValidLine(start, end, solution));

            solution.Add(new Vector2[] { start, end });
        }
        
        return solution;
    }

    private Vector2 GetRandomPointOnBoardEdge()
    {
        Vector2 boardSize = boardBounds.size;
        float t = Random.value;

        if (Random.value < 0.5f) // Vertical edges
        {
            float x = Random.value < 0.5f ? -boardSize.x / 2 : boardSize.x / 2;
            return new Vector2(x, Mathf.Lerp(-boardSize.y / 2, boardSize.y / 2, t));
        }
        else // Horizontal edges
        {
            float y = Random.value < 0.5f ? -boardSize.y / 2 : boardSize.y / 2;
            return new Vector2(Mathf.Lerp(-boardSize.x / 2, boardSize.x / 2, t), y);
        }
    }

    private bool IsValidLine(Vector2 start, Vector2 end, List<Vector2[]> existingLines)
    {
        // Check if the line is too short
        if (Vector2.Distance(start, end) < minObjectSize * 2)
            return false;

        // Check if the line intersects with existing lines
        foreach (var line in existingLines)
        {
            if (LineSegmentsIntersect(start, end, line[0], line[1]))
                return false;
        }

        return true;
    }

    private bool LineSegmentsIntersect(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        float d = (p2.x - p1.x) * (p4.y - p3.y) - (p2.y - p1.y) * (p4.x - p3.x);

        if (d == 0)
            return false;

        float t = ((p3.x - p1.x) * (p4.y - p3.y) - (p3.y - p1.y) * (p4.x - p3.x)) / d;
        float u = ((p3.x - p1.x) * (p2.y - p1.y) - (p3.y - p1.y) * (p2.x - p1.x)) / d;

        return t >= 0 && t <= 1 && u >= 0 && u <= 1;
    }

    private List<Polygon> CalculateAreas(List<Vector2[]> lines)
    {
        // This is a simplified area calculation. For a more accurate implementation,
        // you might need a more sophisticated algorithm to handle complex polygons.
        List<Polygon> areas = new List<Polygon>();
        List<Vector2> intersections = new List<Vector2>();

        // Find all intersections
        for (int i = 0; i < lines.Count; i++)
        {
            for (int j = i + 1; j < lines.Count; j++)
            {
                Vector2 intersection;
                if (LineIntersection(lines[i][0], lines[i][1], lines[j][0], lines[j][1], out intersection))
                {
                    intersections.Add(intersection);
                }
            }
        }

        // Add board corners
        Vector2 boardSize = boardBounds.size;
        intersections.Add(new Vector2(-boardSize.x / 2, -boardSize.y / 2));
        intersections.Add(new Vector2(boardSize.x / 2, -boardSize.y / 2));
        intersections.Add(new Vector2(boardSize.x / 2, boardSize.y / 2));
        intersections.Add(new Vector2(-boardSize.x / 2, boardSize.y / 2));

        // Create polygons from intersections
        // This is a simplified approach and might not work for all cases
        for (int i = 0; i < intersections.Count; i++)
        {
            for (int j = i + 1; j < intersections.Count; j++)
            {
                for (int k = j + 1; k < intersections.Count; k++)
                {
                    areas.Add(new Polygon(new List<Vector2> { intersections[i], intersections[j], intersections[k] }));
                }
            }
        }

        return areas;
    }

    private bool LineIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2 intersection)
    {
        float d = (p2.x - p1.x) * (p4.y - p3.y) - (p2.y - p1.y) * (p4.x - p3.x);

        if (d == 0)
        {
            intersection = Vector2.zero;
            return false;
        }

        float t = ((p3.x - p1.x) * (p4.y - p3.y) - (p3.y - p1.y) * (p4.x - p3.x)) / d;
        float u = ((p3.x - p1.x) * (p2.y - p1.y) - (p3.y - p1.y) * (p2.x - p1.x)) / d;

        if (t < 0 || t > 1 || u < 0 || u > 1)
        {
            intersection = Vector2.zero;
            return false;
        }

        intersection = p1 + t * (p2 - p1);
        return true;
    }

    private void PlaceObjectsInAreas(List<Polygon> areas)
    {
        int totalStars = Random.Range(minStars, maxStars + 1);
        int totalJunks = Random.Range(minJunks, maxJunks + 1);

        List<Polygon> availableAreas = new List<Polygon>(areas);

        // Place stars
        for (int i = 0; i < totalStars; i++)
        {
            if (availableAreas.Count == 0) break;

            int areaIndex = Random.Range(0, availableAreas.Count);
            Vector2 position = availableAreas[areaIndex].GetRandomPointInside();
            GameObject star = Instantiate(starPrefab, position, Quaternion.identity);
            stars.Add(star);

            availableAreas.RemoveAt(areaIndex);
        }

        // Place cosmic junk
        for (int i = 0; i < totalJunks; i++)
        {
            if (availableAreas.Count == 0) break;

            int areaIndex = Random.Range(0, availableAreas.Count);
            Vector2 position = availableAreas[areaIndex].GetRandomPointInside();
            GameObject junk = Instantiate(junkPrefab, position, Quaternion.identity);
            cosmicJunks.Add(junk);

            availableAreas.RemoveAt(areaIndex);
        }
    }

    public void OnRestartButtonClick()
    {
        GenerateLevel();
    }

    [Header("Difficulty Settings")]
    public float difficultyMultiplier = 1.0f;
    public int maxLines = 5;

    private bool ValidateLevel()
    {
        // Check if all areas are properly formed
        // Verify minimum requirements are met
        return true;
    }
}

public class Polygon
{
    private List<Vector2> vertices;

    public Polygon(List<Vector2> points)
    {
        vertices = new List<Vector2>(points);
    }

    public Vector2 GetRandomPointInside()
    {
        Vector2 min = vertices.Aggregate((v1, v2) => Vector2.Min(v1, v2));
        Vector2 max = vertices.Aggregate((v1, v2) => Vector2.Max(v1, v2));

        while (true)
        {
            Vector2 point = new Vector2(
                Random.Range(min.x, max.x),
                Random.Range(min.y, max.y)
            );
            if (IsPointInside(point))
                return point;
        }
    }

    private bool IsPointInside(Vector2 point)
    {
        bool inside = false;
        for (int i = 0, j = vertices.Count - 1; i < vertices.Count; j = i++)
        {
            if (((vertices[i].y > point.y) != (vertices[j].y > point.y)) &&
                (point.x < (vertices[j].x - vertices[i].x) * (point.y - vertices[i].y) / (vertices[j].y - vertices[i].y) + vertices[i].x))
            {
                inside = !inside;
            }
        }
        return inside;
    }
}