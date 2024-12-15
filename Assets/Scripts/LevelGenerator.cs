using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public enum ObjectType
{
    Star,
    Junk
}

public class LevelGenerator : MonoBehaviour
{
    private const float BOARD_WIDTH = 8f;
    private const float BOARD_HEIGHT = 12f;
    private const float CAMERA_HEIGHT = 12f; // Увеличиваем высоту камеры
    private const float CAMERA_WIDTH = 8f; // Ширина камеры в Unity units
    private const float MIN_OBJECT_SPACING = 1f;
    private const float OBJECT_RADIUS = 0.2f; // Единый размер для всех объектов
    private const float SAFE_MARGIN = 0.3f; // Отступ от краев экрана

    private List<GameObject> stars = new List<GameObject>();
    private List<GameObject> junks = new List<GameObject>();
    private GameObject board;
    private List<Polygon> segments = new List<Polygon>();
    private LineManager lineManager;
    private List<LineRenderer> hintLines = new List<LineRenderer>();
    private bool areHintsVisible = false;

    private void Start()
    {
        lineManager = FindObjectOfType<LineManager>();
    }

    public void Initialize()
    {
        CreateBoard();
    }

    private void CreateBoard()
    {
        board = GameObject.CreatePrimitive(PrimitiveType.Quad);
        board.transform.SetParent(transform);
        board.transform.localScale = new Vector3(CAMERA_WIDTH, CAMERA_HEIGHT, 1);
        board.transform.position = new Vector3(0, 0, 1);
        
        Material material = new Material(Shader.Find("Sprites/Default"));
        material.color = Color.white;
        board.GetComponent<Renderer>().material = material;
    }

    public void GenerateLevel(int level)
    {
        ClearLevel();

        // Создаем начальный полигон (все игровое поле)
        List<Vector2> boardVertices = new List<Vector2>
        {
            new Vector2(-CAMERA_WIDTH/2, -CAMERA_HEIGHT/2),
            new Vector2(CAMERA_WIDTH/2, -CAMERA_HEIGHT/2),
            new Vector2(CAMERA_WIDTH/2, CAMERA_HEIGHT/2),
            new Vector2(-CAMERA_WIDTH/2, CAMERA_HEIGHT/2)
        };

        GameObject initialPolygon = new GameObject("Initial Polygon");
        initialPolygon.transform.SetParent(transform);
        Polygon poly = initialPolygon.AddComponent<Polygon>();
        poly.SetPoints(boardVertices);
        segments.Add(poly);

        // Получаем количество доступных линий из LineManager
        int availableLines = lineManager.GetAvailableLines();
        
        // Генерируем линии для разделения поля и создаем линии-подсказки
        List<(Vector2, Vector2)> divisionLines = GenerateDivisionLines();
        
        // Разделяем поле на сегменты
        SplitPolygonsWithLines(divisionLines);

        // Распределяем типы объектов по сегментам
        AssignObjectTypesToSegments();

        // Заполняем сегменты объектами
        FillSegmentsWithObjects();

        // Скрываем подсказки при генерации нового уровня
        HideHints();
    }

    private List<(Vector2, Vector2)> GenerateDivisionLines()
    {
        // Очищаем предыдущие линии-подсказки
        foreach (var line in hintLines)
        {
            if (line != null)
            {
                Destroy(line.gameObject);
            }
        }
        hintLines.Clear();

        // Генерируем новые линии
        int numLines = 3; // Фиксированное количество линий
        List<Vector2> lineStartPoints = new List<Vector2>();
        List<Vector2> lineEndPoints = new List<Vector2>();

        for (int i = 0; i < numLines; i++)
        {
            Vector2 startPoint, endPoint;
            bool validLine;
            int attempts = 0;
            const int maxAttempts = 100;

            do
            {
                validLine = true;
                float lineType = Random.value;

                if (lineType < 0.33f) // Вертикальная линия
                {
                    float x = Random.Range(-BOARD_WIDTH / 2 + 1f, BOARD_WIDTH / 2 - 1f);
                    startPoint = new Vector2(x, BOARD_HEIGHT / 2);
                    endPoint = new Vector2(x, -BOARD_HEIGHT / 2);
                }
                else if (lineType < 0.66f) // Горизонтальная линия
                {
                    float y = Random.Range(-BOARD_HEIGHT / 2 + 1f, BOARD_HEIGHT / 2 - 1f);
                    startPoint = new Vector2(-BOARD_WIDTH / 2, y);
                    endPoint = new Vector2(BOARD_WIDTH / 2, y);
                }
                else // Диагональная линия
                {
                    // Выбираем случайную точку на одной из сторон поля
                    int side = Random.Range(0, 4);
                    switch (side)
                    {
                        case 0: // Верхняя сторона
                            startPoint = new Vector2(Random.Range(-BOARD_WIDTH / 2, BOARD_WIDTH / 2), BOARD_HEIGHT / 2);
                            break;
                        case 1: // Правая сторона
                            startPoint = new Vector2(BOARD_WIDTH / 2, Random.Range(-BOARD_HEIGHT / 2, BOARD_HEIGHT / 2));
                            break;
                        case 2: // Нижняя сторона
                            startPoint = new Vector2(Random.Range(-BOARD_WIDTH / 2, BOARD_WIDTH / 2), -BOARD_HEIGHT / 2);
                            break;
                        default: // Левая сторона
                            startPoint = new Vector2(-BOARD_WIDTH / 2, Random.Range(-BOARD_HEIGHT / 2, BOARD_HEIGHT / 2));
                            break;
                    }

                    // Выбираем случайную точку на противоположной стороне
                    int oppositeSide = (side + 2) % 4;
                    switch (oppositeSide)
                    {
                        case 0: // Верхняя сторона
                            endPoint = new Vector2(Random.Range(-BOARD_WIDTH / 2, BOARD_WIDTH / 2), BOARD_HEIGHT / 2);
                            break;
                        case 1: // Правая сторона
                            endPoint = new Vector2(BOARD_WIDTH / 2, Random.Range(-BOARD_HEIGHT / 2, BOARD_HEIGHT / 2));
                            break;
                        case 2: // Нижняя сторона
                            endPoint = new Vector2(Random.Range(-BOARD_WIDTH / 2, BOARD_WIDTH / 2), -BOARD_HEIGHT / 2);
                            break;
                        default: // Левая сторона
                            endPoint = new Vector2(-BOARD_WIDTH / 2, Random.Range(-BOARD_HEIGHT / 2, BOARD_HEIGHT / 2));
                            break;
                    }
                }

                // Проверяем, не слишком ли близко к существующим линиям
                for (int j = 0; j < lineStartPoints.Count; j++)
                {
                    if (Vector2.Distance(startPoint, lineStartPoints[j]) < 1f ||
                        Vector2.Distance(endPoint, lineEndPoints[j]) < 1f)
                    {
                        validLine = false;
                        break;
                    }
                }

                attempts++;
            } while (!validLine && attempts < maxAttempts);

            if (validLine)
            {
                lineStartPoints.Add(startPoint);
                lineEndPoints.Add(endPoint);

                // Создаем линию-подсказку
                GameObject lineObj = new GameObject($"HintLine_{i}");
                lineObj.transform.SetParent(transform);
                LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                lineRenderer.startColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                lineRenderer.endColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                lineRenderer.startWidth = 0.1f;
                lineRenderer.endWidth = 0.1f;
                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, new Vector3(startPoint.x, startPoint.y, 0));
                lineRenderer.SetPosition(1, new Vector3(endPoint.x, endPoint.y, 0));
                lineRenderer.gameObject.SetActive(false);
                hintLines.Add(lineRenderer);
            }
        }

        List<(Vector2, Vector2)> result = new List<(Vector2, Vector2)>();
        for (int i = 0; i < lineStartPoints.Count; i++)
        {
            result.Add((lineStartPoints[i], lineEndPoints[i]));
        }
        return result;
    }

    private void SplitPolygonsWithLines(List<(Vector2, Vector2)> lines)
    {
        foreach (var line in lines)
        {
            List<Polygon> newSegments = new List<Polygon>();
            
            foreach (var segment in segments.ToList())
            {
                var splitResult = segment.SplitByLine(line.Item1, line.Item2);
                if (splitResult.Count > 1)
                {
                    newSegments.AddRange(splitResult);
                    segments.Remove(segment);
                    Destroy(segment.gameObject);
                }
                else
                {
                    newSegments.Add(segment);
                }
            }

            segments = newSegments;
        }
    }

    private bool AreSegmentsAdjacent(Polygon a, Polygon b)
    {
        // Проверяем, есть ли общие точки у полигонов
        var pointsA = a.GetPoints();
        var pointsB = b.GetPoints();

        foreach (var pointA in pointsA)
        {
            foreach (var pointB in pointsB)
            {
                if (Vector2.Distance(pointA, pointB) < 0.1f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void AssignObjectTypesToSegments()
    {
        if (segments.Count == 0) return;

        // Очищаем предыдущие назначения
        foreach (var segment in segments)
        {
            segment.SetObjectType(ObjectType.Star); // По умолчанию все звезды
        }

        // Создаем список необработанных сегментов
        List<Polygon> unassignedSegments = new List<Polygon>(segments);
        List<Polygon> junkSegments = new List<Polygon>();
        List<Polygon> starSegments = new List<Polygon>();

        // Начинаем с первого сегмента
        var firstSegment = unassignedSegments[0];
        unassignedSegments.RemoveAt(0);
        starSegments.Add(firstSegment);

        while (unassignedSegments.Count > 0)
        {
            // Находим сегмент, который граничит с уже обработанными
            Polygon nextSegment = null;
            ObjectType nextType = ObjectType.Star;
            
            foreach (var segment in unassignedSegments)
            {
                bool adjacentToStar = false;
                bool adjacentToJunk = false;

                // Проверяем соседство со звездными сегментами
                foreach (var starSegment in starSegments)
                {
                    if (AreSegmentsAdjacent(segment, starSegment))
                    {
                        adjacentToStar = true;
                        break;
                    }
                }

                // Проверяем соседство с мусорными сегментами
                foreach (var junkSegment in junkSegments)
                {
                    if (AreSegmentsAdjacent(segment, junkSegment))
                    {
                        adjacentToJunk = true;
                        break;
                    }
                }

                // Если сегмент граничит с уже обработанным
                if (adjacentToStar || adjacentToJunk)
                {
                    nextSegment = segment;
                    
                    // Если граничит со звездами, делаем его мусором
                    if (adjacentToStar && !adjacentToJunk)
                    {
                        nextType = ObjectType.Junk;
                    }
                    // Если граничит с мусором, делаем его звездами
                    else if (!adjacentToStar && adjacentToJunk)
                    {
                        nextType = ObjectType.Star;
                    }
                    // Если граничит с обоими типами или ни с одним,
                    // выбираем тип, которого меньше
                    else
                    {
                        nextType = starSegments.Count <= junkSegments.Count ? ObjectType.Star : ObjectType.Junk;
                    }
                    break;
                }
            }

            // Если не нашли граничащий сегмент, берем первый попавшийся
            if (nextSegment == null && unassignedSegments.Count > 0)
            {
                nextSegment = unassignedSegments[0];
                nextType = starSegments.Count <= junkSegments.Count ? ObjectType.Star : ObjectType.Junk;
            }

            if (nextSegment != null)
            {
                unassignedSegments.Remove(nextSegment);
                nextSegment.SetObjectType(nextType);
                
                if (nextType == ObjectType.Star)
                    starSegments.Add(nextSegment);
                else
                    junkSegments.Add(nextSegment);
            }
        }

        // Проверяем, есть ли хотя бы один сегмент каждого типа
        if (starSegments.Count == 0 && junkSegments.Count > 0)
        {
            var segment = junkSegments[0];
            segment.SetObjectType(ObjectType.Star);
            starSegments.Add(segment);
            junkSegments.RemoveAt(0);
        }
        else if (junkSegments.Count == 0 && starSegments.Count > 0)
        {
            var segment = starSegments[0];
            segment.SetObjectType(ObjectType.Junk);
            junkSegments.Add(segment);
            starSegments.RemoveAt(0);
        }
    }

    private bool IsPositionInsideBounds(Vector2 position)
    {
        float margin = SAFE_MARGIN + OBJECT_RADIUS;
        float maxX = CAMERA_WIDTH/2 - margin;
        float maxY = CAMERA_HEIGHT/2 - margin;
        
        return position.x >= -maxX && 
               position.x <= maxX && 
               position.y >= -maxY && 
               position.y <= maxY;
    }

    private void FillSegmentsWithObjects(int maxAttempts = 100)
    {
        float objectSpacing = MIN_OBJECT_SPACING * 1.2f;
        float minDistanceFromLine = OBJECT_RADIUS + 0.2f; // Minimum distance from dividing lines

        foreach (var segment in segments)
        {
            float segmentArea = CalculatePolygonArea(segment.GetPoints());
            int maxObjects = Mathf.Max(1, Mathf.FloorToInt(segmentArea / (objectSpacing * objectSpacing * 4)));

            List<Vector2> placedPositions = new List<Vector2>();

            for (int i = 0; i < maxObjects; i++)
            {
                int attempts = 0;
                bool positionFound = false;
                Vector2 position = Vector2.zero;

                while (!positionFound && attempts < maxAttempts)
                {
                    position = segment.GetRandomPointInside();
                    
                    if (!IsPositionInsideBounds(position))
                    {
                        attempts++;
                        continue;
                    }

                    bool isTooClose = false;

                    // Check distance from segment edges (dividing lines)
                    var points = segment.GetPoints();
                    for (int j = 0; j < points.Count; j++)
                    {
                        int nextIndex = (j + 1) % points.Count;
                        Vector2 start = points[j];
                        Vector2 end = points[nextIndex];
                        
                        float distanceToLine = HandleUtility.DistancePointLine(position, start, end);
                        if (distanceToLine < minDistanceFromLine)
                        {
                            isTooClose = true;
                            break;
                        }
                    }

                    if (!isTooClose)
                    {
                        foreach (var placedPos in placedPositions)
                        {
                            if (Vector2.Distance(position, placedPos) < objectSpacing)
                            {
                                isTooClose = true;
                                break;
                            }
                        }
                    }

                    if (!isTooClose)
                    {
                        foreach (var star in stars)
                        {
                            if (Vector2.Distance(position, star.transform.position) < objectSpacing)
                            {
                                isTooClose = true;
                                break;
                            }
                        }
                    }

                    if (!isTooClose)
                    {
                        foreach (var junk in junks)
                        {
                            if (Vector2.Distance(position, junk.transform.position) < objectSpacing)
                            {
                                isTooClose = true;
                                break;
                            }
                        }
                    }

                    positionFound = !isTooClose;
                    attempts++;
                }

                if (positionFound)
                {
                    placedPositions.Add(position);
                    GameObject obj;
                    if (segment.GetObjectType() == ObjectType.Star)
                    {
                        obj = CreateStar(position);
                        obj.transform.localScale = Vector3.one * (OBJECT_RADIUS * 2);
                        stars.Add(obj);
                    }
                    else
                    {
                        obj = CreateJunk(position);
                        obj.transform.localScale = Vector3.one * (OBJECT_RADIUS * 2);
                        junks.Add(obj);
                    }
                }
            }
        }
    }

    private float CalculatePolygonArea(List<Vector2> points)
    {
        float area = 0;
        int j = points.Count - 1;
        
        for (int i = 0; i < points.Count; i++)
        {
            area += (points[j].x + points[i].x) * (points[j].y - points[i].y);
            j = i;
        }
        
        return Mathf.Abs(area / 2);
    }

    private GameObject CreateStar(Vector2 position)
    {
        GameObject star = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        star.transform.SetParent(transform);
        star.transform.position = new Vector3(position.x, position.y, 0);

        star.AddComponent<CircleCollider2D>();
        
        Material material = new Material(Shader.Find("Sprites/Default"));
        material.color = Color.yellow;
        star.GetComponent<Renderer>().material = material;

        return star;
    }

    private GameObject CreateJunk(Vector2 position)
    {
        GameObject junk = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        junk.transform.SetParent(transform);
        junk.transform.position = new Vector3(position.x, position.y, 0);

        junk.AddComponent<CircleCollider2D>();
        
        Material material = new Material(Shader.Find("Sprites/Default"));
        material.color = Color.gray;
        junk.GetComponent<Renderer>().material = material;

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
        
        foreach (var segment in segments)
        {
            if (segment != null)
            {
                Destroy(segment.gameObject);
            }
        }
        segments.Clear();
    }

    public List<GameObject> GetStars()
    {
        return stars;
    }

    public List<GameObject> GetJunks()
    {
        return junks;
    }

    public void ToggleHints()
    {
        areHintsVisible = !areHintsVisible;
        foreach (var line in hintLines)
        {
            if (line != null)
            {
                line.gameObject.SetActive(areHintsVisible);
            }
        }
    }

    public void HideHints()
    {
        areHintsVisible = false;
        foreach (var line in hintLines)
        {
            if (line != null)
            {
                line.gameObject.SetActive(false);
            }
        }
    }
}