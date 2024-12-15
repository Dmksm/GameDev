using UnityEngine;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    private const float BOARD_WIDTH = 10f;
    private const float BOARD_HEIGHT = 10f;
    private const float EPSILON = 0.0001f;

    public void Initialize()
    {
        // Add any initialization logic here if needed
    }

    public bool CheckIntersection(Vector3 startPoint, Vector3 endPoint, List<LineRenderer> existingLines)
    {
        Vector2 start2D = new Vector2(startPoint.x, startPoint.y);
        Vector2 end2D = new Vector2(endPoint.x, endPoint.y);

        foreach (var line in existingLines)
        {
            if (line != null)
            {
                Vector2 lineStart = line.GetPosition(0);
                Vector2 lineEnd = line.GetPosition(1);

                if (DoLineSegmentsIntersect(start2D, end2D, lineStart, lineEnd))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool DoLineSegmentsIntersect(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        float denominator = ((p4.y - p3.y) * (p2.x - p1.x)) - ((p4.x - p3.x) * (p2.y - p1.y));

        if (Mathf.Abs(denominator) < EPSILON)
        {
            return false;
        }

        float ua = (((p4.x - p3.x) * (p1.y - p3.y)) - ((p4.y - p3.y) * (p1.x - p3.x))) / denominator;
        float ub = (((p2.x - p1.x) * (p1.y - p3.y)) - ((p2.y - p1.y) * (p1.x - p3.x))) / denominator;

        if (ua < 0 || ua > 1 || ub < 0 || ub > 1)
        {
            return false;
        }

        return true;
    }

    public bool IsPointInsideBoard(Vector2 point)
    {
        return point.x >= -BOARD_WIDTH/2 && point.x <= BOARD_WIDTH/2 &&
               point.y >= -BOARD_HEIGHT/2 && point.y <= BOARD_HEIGHT/2;
    }

    public Vector2 ClampPointToBoard(Vector2 point)
    {
        return new Vector2(
            Mathf.Clamp(point.x, -BOARD_WIDTH/2, BOARD_WIDTH/2),
            Mathf.Clamp(point.y, -BOARD_HEIGHT/2, BOARD_HEIGHT/2)
        );
    }
}
