using UnityEngine;
using System.Collections.Generic;

public class Polygon
{
    private List<Vector2> vertices;
    private const float EPSILON = 0.0001f;

    public Polygon(List<Vector2> vertices)
    {
        if (vertices == null || vertices.Count < 3)
            throw new System.ArgumentException("Polygon must have at least 3 vertices");

        this.vertices = vertices;
    }

    public Vector2 GetRandomPointInside()
    {
        if (vertices.Count < 3)
            return GetCenter();

        // Разбиваем полигон на треугольники
        float totalArea = 0;
        List<(Vector2, Vector2, Vector2)> triangles = new List<(Vector2, Vector2, Vector2)>();
        Vector2 center = GetCenter();

        for (int i = 0; i < vertices.Count; i++)
        {
            Vector2 v1 = vertices[i];
            Vector2 v2 = vertices[(i + 1) % vertices.Count];
            triangles.Add((v1, v2, center));
            totalArea += CalculateTriangleArea(v1, v2, center);
        }

        // Выбираем случайный треугольник с учетом его площади
        float randomArea = Random.Range(0f, totalArea);
        float currentArea = 0;
        
        for (int i = 0; i < triangles.Count; i++)
        {
            currentArea += CalculateTriangleArea(triangles[i].Item1, triangles[i].Item2, triangles[i].Item3);
            if (currentArea >= randomArea)
            {
                // Генерируем случайную точку внутри выбранного треугольника
                float r1 = Mathf.Sqrt(Random.Range(0f, 1f));
                float r2 = Random.Range(0f, 1f);
                
                float a = 1 - r1;
                float b = r1 * (1 - r2);
                float c = r1 * r2;

                return a * triangles[i].Item1 + b * triangles[i].Item2 + c * triangles[i].Item3;
            }
        }

        return GetCenter();
    }

    private float CalculateTriangleArea(Vector2 a, Vector2 b, Vector2 c)
    {
        return Mathf.Abs((b.x - a.x) * (c.y - a.y) - (c.x - a.x) * (b.y - a.y)) / 2f;
    }

    public Vector2 GetCenter()
    {
        if (vertices == null || vertices.Count == 0)
            return Vector2.zero;

        Vector2 center = Vector2.zero;
        foreach (var vertex in vertices)
        {
            center += vertex;
        }
        return center / vertices.Count;
    }

    public List<Polygon> SplitByLine(Vector3 lineStart, Vector3 lineEnd)
    {
        if (vertices.Count < 3)
            return new List<Polygon> { this };

        List<Vector2> intersectionPoints = new List<Vector2>();
        List<int> intersectionIndices = new List<int>();

        // Находим точки пересечения
        for (int i = 0; i < vertices.Count; i++)
        {
            Vector2 start = vertices[i];
            Vector2 end = vertices[(i + 1) % vertices.Count];

            Vector2 intersection;
            if (LineIntersection((Vector2)lineStart, (Vector2)lineEnd, start, end, out intersection))
            {
                // Проверяем, не добавили ли мы уже эту точку
                bool isDuplicate = false;
                foreach (var point in intersectionPoints)
                {
                    if (Vector2.Distance(point, intersection) < EPSILON)
                    {
                        isDuplicate = true;
                        break;
                    }
                }

                if (!isDuplicate)
                {
                    intersectionPoints.Add(intersection);
                    intersectionIndices.Add(i);
                }
            }
        }

        // Если нет пересечений или только одно, возвращаем исходный полигон
        if (intersectionPoints.Count < 2)
        {
            return new List<Polygon> { this };
        }

        // Создаем два новых полигона
        List<Vector2> polygon1Points = new List<Vector2>();
        List<Vector2> polygon2Points = new List<Vector2>();

        // Сортируем точки пересечения по расстоянию от начала линии
        for (int i = 0; i < intersectionPoints.Count - 1; i++)
        {
            for (int j = i + 1; j < intersectionPoints.Count; j++)
            {
                float dist1 = Vector2.Distance((Vector2)lineStart, intersectionPoints[i]);
                float dist2 = Vector2.Distance((Vector2)lineStart, intersectionPoints[j]);
                if (dist2 < dist1)
                {
                    var tempPoint = intersectionPoints[i];
                    var tempIndex = intersectionIndices[i];
                    intersectionPoints[i] = intersectionPoints[j];
                    intersectionIndices[i] = intersectionIndices[j];
                    intersectionPoints[j] = tempPoint;
                    intersectionIndices[j] = tempIndex;
                }
            }
        }

        // Добавляем точки для первого полигона
        polygon1Points.Add(intersectionPoints[0]);
        int currentIndex = intersectionIndices[0];
        do
        {
            currentIndex = (currentIndex + 1) % vertices.Count;
            polygon1Points.Add(vertices[currentIndex]);
        }
        while (currentIndex != intersectionIndices[1]);
        polygon1Points.Add(intersectionPoints[1]);

        // Добавляем точки для второго полигона
        polygon2Points.Add(intersectionPoints[1]);
        currentIndex = intersectionIndices[1];
        do
        {
            currentIndex = (currentIndex + 1) % vertices.Count;
            polygon2Points.Add(vertices[currentIndex]);
        }
        while (currentIndex != intersectionIndices[0]);
        polygon2Points.Add(intersectionPoints[0]);

        return new List<Polygon> {
            new Polygon(polygon1Points),
            new Polygon(polygon2Points)
        };
    }

    private bool LineIntersection(Vector2 line1Start, Vector2 line1End, Vector2 line2Start, Vector2 line2End, out Vector2 intersection)
    {
        intersection = Vector2.zero;

        float denominator = (line2End.y - line2Start.y) * (line1End.x - line1Start.x) -
                          (line2End.x - line2Start.x) * (line1End.y - line1Start.y);

        if (Mathf.Abs(denominator) < EPSILON)
            return false;

        float ua = ((line2End.x - line2Start.x) * (line1Start.y - line2Start.y) -
                   (line2End.y - line2Start.y) * (line1Start.x - line2Start.x)) / denominator;
        float ub = ((line1End.x - line1Start.x) * (line1Start.y - line2Start.y) -
                   (line1End.y - line1Start.y) * (line1Start.x - line2Start.x)) / denominator;

        if (ua < 0 || ua > 1 || ub < 0 || ub > 1)
            return false;

        intersection = new Vector2(
            line1Start.x + ua * (line1End.x - line1Start.x),
            line1Start.y + ua * (line1End.y - line1Start.y)
        );

        return true;
    }

    public bool Contains(Vector2 point)
    {
        if (vertices.Count < 3)
            return false;

        bool inside = false;
        for (int i = 0, j = vertices.Count - 1; i < vertices.Count; j = i++)
        {
            if (((vertices[i].y > point.y) != (vertices[j].y > point.y)) &&
                (point.x < (vertices[j].x - vertices[i].x) * (point.y - vertices[i].y) /
                          (vertices[j].y - vertices[i].y) + vertices[i].x))
            {
                inside = !inside;
            }
        }
        return inside;
    }
}
