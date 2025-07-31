using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LoopSpellDetector : MonoBehaviour
{
    public float minPointDistance = 0.15f;
    public float closeLoopThreshold = 0.4f;

    private readonly List<Vector2> drawnPoints = new();
    private LineRenderer lineRenderer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = false;
        lineRenderer.widthMultiplier = 0.05f;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            drawnPoints.Clear();
            lineRenderer.positionCount = 0;
            AddPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }
        else if (Input.GetMouseButton(0))
        {
            Vector2 currentPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (Vector2.Distance(currentPos, drawnPoints[^1]) > minPointDistance)
            {
                AddPoint(currentPos);
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (drawnPoints.Count < 3)
            {
                Debug.Log("Too few points.");
                return;
            }

            bool isClosed = Vector2.Distance(drawnPoints[0], drawnPoints[^1]) < closeLoopThreshold;

            if (isClosed)
            {
                int crossings = CountSelfIntersections(drawnPoints, closeLoopThreshold);
                Debug.Log($"Loop closed. Self-crossings: {crossings}");
                TriggerSpell(crossings);
            }
            else
            {
                Debug.Log("Not a closed loop.");
            }
        }
    }

    void AddPoint(Vector2 point)
    {
        drawnPoints.Add(point);
        lineRenderer.positionCount = drawnPoints.Count;
        lineRenderer.SetPosition(drawnPoints.Count - 1, point);
    }

   int CountSelfIntersections(List<Vector2> path, float closeLoopThreshold)
    {
        int count = 0;
        int segmentCount = path.Count;

        if (segmentCount < 4)
            return 0; // Not enough segments to intersect

        Vector2 startPoint = path[0];

        for (int i = 0; i < segmentCount - 1; i++)
        {
            Vector2 a1 = path[i];
            Vector2 a2 = path[i + 1];

            // Skip if either point in segment A is near the start
            if (Vector2.Distance(a1, startPoint) < closeLoopThreshold || Vector2.Distance(a2, startPoint) < closeLoopThreshold)
                continue;

            for (int j = i + 2; j < segmentCount - 1; j++)
            {
                // Skip adjacent segments
                if (j == i + 1)
                    continue;

                Vector2 b1 = path[j];
                Vector2 b2 = path[j + 1];

                // Skip if either point in segment B is near the start
                if (Vector2.Distance(b1, startPoint) < closeLoopThreshold || Vector2.Distance(b2, startPoint) < closeLoopThreshold)
                    continue;

                if (LineSegmentsIntersect(a1, a2, b1, b2))
                    count++;
            }
        }

        return count;
    }

    bool LineSegmentsIntersect(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
    {
        return DoLinesIntersect(p1, p2, q1, q2);
    }

    bool DoLinesIntersect(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
    {
        float d = (a2.x - a1.x) * (b2.y - b1.y) - (a2.y - a1.y) * (b2.x - b1.x);
        if (Mathf.Approximately(d, 0))
            return false;

        float u = ((b1.x - a1.x) * (b2.y - b1.y) - (b1.y - a1.y) * (b2.x - b1.x)) / d;
        float v = ((b1.x - a1.x) * (a2.y - a1.y) - (b1.y - a1.y) * (a2.x - a1.x)) / d;

        return (u >= 0 && u <= 1) && (v >= 0 && v <= 1);
    }

    void TriggerSpell(int crossings)
    {
        // Replace these with actual spell logic
        switch (crossings)
        {
            case 0:
                Debug.Log("Cast Fireball!");
                break;
            case 1:
                Debug.Log("Cast Ice Spear!");
                break;
            case 2:
                Debug.Log("Cast Lightning Arc!");
                break;
            default:
                Debug.Log("Cast Black Hole!");
                break;
        }
    }
}
