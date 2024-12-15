using UnityEngine;
using System.Collections.Generic;

public class Polygon : MonoBehaviour
{
    private List<Vector2> points = new List<Vector2>();
    private ObjectType objectType;

    public void SetPoints(List<Vector2> newPoints)
    {
        points = new List<Vector2>(newPoints);
    }

    public List<Vector2> GetPoints()
    {
        return points;
    }

    public void SetObjectType(ObjectType type)
    {
        objectType = type;
    }

    public ObjectType GetObjectType()
    {
        return objectType;
    }

    public Vector2 GetCenter()
    {
        if (points == null || points.Count == 0)
            return Vector2.zero;

        Vector2 sum = Vector2.zero;
        foreach (var point in points)
        {
            sum += point;
        }
        return sum / points.Count;
    }

    public Vector2 GetRandomPointInside()
    {
        if (points == null || points.Count < 3)
            return Vector2.zero;

        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;

        foreach (var point in points)
        {
            minX = Mathf.Min(minX, point.x);
            minY = Mathf.Min(minY, point.y);
            maxX = Mathf.Max(maxX, point.x);
            maxY = Mathf.Max(maxY, point.y);
        }

        int maxAttempts = 100;
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            Vector2 randomPoint = new Vector2(
                Random.Range(minX, maxX),
                Random.Range(minY, maxY)
            );

            if (IsPointInside(randomPoint))
            {
                return randomPoint;
            }

            attempts++;
        }

        return GetCenter();
    }

    private bool IsPointInside(Vector2 point)
    {
        bool inside = false;
        int j = points.Count - 1;

        for (int i = 0; i < points.Count; i++)
        {
            if (((points[i].y > point.y) != (points[j].y > point.y)) &&
                (point.x < (points[j].x - points[i].x) * (point.y - points[i].y) / (points[j].y - points[i].y) + points[i].x))
            {
                inside = !inside;
            }
            j = i;
        }

        return inside;
    }

    public List<Polygon> SplitByLine(Vector2 lineStart, Vector2 lineEnd)
    {
        List<Vector2> intersectionPoints = new List<Vector2>();
        List<int> intersectionIndices = new List<int>();

        // Находим точки пересечения
        for (int i = 0; i < points.Count; i++)
        {
            Vector2 p1 = points[i];
            Vector2 p2 = points[(i + 1) % points.Count];

            if (LineSegmentsIntersect(lineStart, lineEnd, p1, p2, out Vector2 intersection))
            {
                intersectionPoints.Add(intersection);
                intersectionIndices.Add(i);
            }
        }

        // Если нет двух точек пересечения, возвращаем исходный полигон
        if (intersectionPoints.Count != 2)
        {
            return new List<Polygon> { this };
        }

        // Создаем два новых списка точек для полигонов
        List<Vector2> polygon1Points = new List<Vector2>();
        List<Vector2> polygon2Points = new List<Vector2>();

        // Добавляем первую точку пересечения в оба полигона
        polygon1Points.Add(intersectionPoints[0]);
        polygon2Points.Add(intersectionPoints[0]);

        // Проходим по точкам от первой точки пересечения до второй
        int startIdx = (intersectionIndices[0] + 1) % points.Count;
        int endIdx = intersectionIndices[1];

        if (startIdx <= endIdx)
        {
            for (int i = startIdx; i <= endIdx; i++)
            {
                polygon1Points.Add(points[i]);
            }
        }
        else
        {
            for (int i = startIdx; i < points.Count; i++)
            {
                polygon1Points.Add(points[i]);
            }
            for (int i = 0; i <= endIdx; i++)
            {
                polygon1Points.Add(points[i]);
            }
        }

        // Добавляем вторую точку пересечения
        polygon1Points.Add(intersectionPoints[1]);
        polygon2Points.Add(intersectionPoints[1]);

        // Проходим по оставшимся точкам для второго полигона
        startIdx = (intersectionIndices[1] + 1) % points.Count;
        endIdx = intersectionIndices[0];

        if (startIdx <= endIdx)
        {
            for (int i = startIdx; i <= endIdx; i++)
            {
                polygon2Points.Add(points[i]);
            }
        }
        else
        {
            for (int i = startIdx; i < points.Count; i++)
            {
                polygon2Points.Add(points[i]);
            }
            for (int i = 0; i <= endIdx; i++)
            {
                polygon2Points.Add(points[i]);
            }
        }

        // Создаем новые полигоны
        List<Polygon> result = new List<Polygon>();

        if (polygon1Points.Count >= 3)
        {
            GameObject newPolygon1 = new GameObject("Polygon1");
            newPolygon1.transform.SetParent(transform.parent);
            Polygon poly1 = newPolygon1.AddComponent<Polygon>();
            poly1.SetPoints(polygon1Points);
            result.Add(poly1);
        }

        if (polygon2Points.Count >= 3)
        {
            GameObject newPolygon2 = new GameObject("Polygon2");
            newPolygon2.transform.SetParent(transform.parent);
            Polygon poly2 = newPolygon2.AddComponent<Polygon>();
            poly2.SetPoints(polygon2Points);
            result.Add(poly2);
        }

        return result;
    }

    private bool LineSegmentsIntersect(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2 intersection)
    {
        intersection = Vector2.zero;

        float denominator = (p4.y - p3.y) * (p2.x - p1.x) - (p4.x - p3.x) * (p2.y - p1.y);

        if (denominator == 0)
            return false;

        float ua = ((p4.x - p3.x) * (p1.y - p3.y) - (p4.y - p3.y) * (p1.x - p3.x)) / denominator;
        float ub = ((p2.x - p1.x) * (p1.y - p3.y) - (p2.y - p1.y) * (p1.x - p3.x)) / denominator;

        if (ua < 0 || ua > 1 || ub < 0 || ub > 1)
            return false;

        intersection = new Vector2(
            p1.x + ua * (p2.x - p1.x),
            p1.y + ua * (p2.y - p1.y)
        );

        return true;
    }
}