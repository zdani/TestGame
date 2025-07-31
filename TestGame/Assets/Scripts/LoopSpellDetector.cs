using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(LineRenderer))]
public class LoopSpellDetector : MonoBehaviour
{
    public Gradient sparkGradientNormal;
    public Gradient sparkGradientBright;

    public float minPointDistance = 0.15f;
    public float minLoopLength = 0.75f;
    public float closeLoopThreshold = 0.4f;

    private LineRenderer lineRenderer;
    private ParticleSystem myParticleSystem;

    private readonly List<Vector2> drawnPoints = new();
    private Vector2 startPoint;    
    private bool isDrawing = false;
    private bool loopReady = false;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = false;
        lineRenderer.widthMultiplier = 0.05f;
        lineRenderer.colorGradient = sparkGradientNormal;

        myParticleSystem = GetComponent<ParticleSystem>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
        {
            isDrawing = true;
            drawnPoints.Clear();
            lineRenderer.positionCount = 0;
            startPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            AddPoint(startPoint);

            //Particle effect management
            transform.position = startPoint;
            myParticleSystem.Play();
        }
        else if (Input.GetMouseButton(0) && isDrawing)
        {
            Vector2 currentPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = currentPoint;
            if (Vector2.Distance(currentPoint, drawnPoints[^1]) > minPointDistance)
            {
                AddPoint(currentPoint);
            }

            float pathLength = GetPathLength(drawnPoints);
            bool isCloseToStart = Vector2.Distance(currentPoint, startPoint) < closeLoopThreshold;
            loopReady = isCloseToStart && pathLength >= minLoopLength;

            // Change appearance if loop is ready
            if (loopReady)
            {
                lineRenderer.colorGradient = sparkGradientBright;
                lineRenderer.widthMultiplier = 0.15f;
            }
            else
            {
                lineRenderer.colorGradient = sparkGradientNormal;
                lineRenderer.widthMultiplier = 0.05f;
            }
        }
        else if (Input.GetMouseButtonUp(0) && isDrawing)
        {
            isDrawing = false;
            myParticleSystem.Stop();

            if (loopReady)
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
    bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    void AddPoint(Vector2 point)
    {
        drawnPoints.Add(point);
        lineRenderer.positionCount = drawnPoints.Count;
        lineRenderer.SetPosition(drawnPoints.Count - 1, point);
    }

    float GetPathLength(List<Vector2> points, int startIndex = 0)
    {
        float length = 0f;
        for (int i = startIndex; i < points.Count - 1; i++)
        {
            length += Vector2.Distance(points[i], points[i + 1]);
        }
        return length;
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

            for (int j = i + 2; j < segmentCount - 1; j++)
            {
                // Skip adjacent segments
                if (j == i + 1)
                    continue;

                Vector2 b1 = path[j];
                Vector2 b2 = path[j + 1];

                if (DoLinesIntersect(a1, a2, b1, b2))
                {
                    // Don't count intersection if the segment is positionally close to the start point and path-length-wise close to the end point
                    bool segmentPositionallyCloseToStart = Vector2.Distance(a1, startPoint) < closeLoopThreshold || Vector2.Distance(a2, startPoint) < closeLoopThreshold;
                    if (!segmentPositionallyCloseToStart)
                    {
                        count++;
                    }
                    else
                    {
                        bool segmentLengthCloseToEnd = GetPathLength(path, j) < minLoopLength;
                        Debug.Log($"Length to end of path is {GetPathLength(path, j)}");
                        if (!segmentLengthCloseToEnd)
                        {
                            count++;
                        }
                        else
                        {
                            Debug.Log("Skipping segment due to close proximity to start and end points");
                            continue;
                        }

                    }
                }
            }
        }

        return count;
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
                Debug.Log("Casting Fireball");
                break;
            case 1:
                Debug.Log("Casting ability #2");
                break;
            case 2:
                Debug.Log("Casting ability #3");
                break;
            case 3:
                Debug.Log("Casting ability #4");
                break;
            default:
                Debug.Log("No ability is mapped to this number of crossings");
                break;
        }
    }
}
