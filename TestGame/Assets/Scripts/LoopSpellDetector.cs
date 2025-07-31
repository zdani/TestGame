using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(LineRenderer))]
public class LoopSpellDetector : MonoBehaviour
{
    public GameObject intersectionPrefab;
    private List<Vector2> activeIntersections = new();
    private List<Vector2> pendingIntersections = new();
    private List<GameObject> intersectionObjs = new();

    public GameObject fadingLinePrefab;
    public Gradient lineGradientNormal;
    public Gradient lineGradientBright;

    public float minPointDistance = 0.15f;
    public float minLoopLength = 0.75f;
    public float closeLoopThreshold = 0.4f;
    public float fadeDuration = 1f;

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
        lineRenderer.colorGradient = lineGradientNormal;

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
                lineRenderer.colorGradient = lineGradientBright;
                lineRenderer.widthMultiplier = 0.15f;
            }
            else
            {
                lineRenderer.colorGradient = lineGradientNormal;
                lineRenderer.widthMultiplier = 0.05f;
            }

            for (int i = pendingIntersections.Count - 1; i >= 0; i--)
            {
                Vector2 intersectPoint = pendingIntersections[i];
                if (Vector2.Distance(intersectPoint, drawnPoints[^1]) > closeLoopThreshold)
                {
                    intersectionObjs.Add(Instantiate(intersectionPrefab, intersectPoint, intersectionPrefab.transform.rotation));
                    activeIntersections.Add(intersectPoint);
                    pendingIntersections.RemoveAt(i);
                }
            }
        }
        else if (Input.GetMouseButtonUp(0) && isDrawing)
        {
            isDrawing = false;
            myParticleSystem.Stop();
            CloseLoopIfNeeded();
            SpawnAndFadeCloneLine();
            lineRenderer.positionCount = 0;
            lineRenderer.colorGradient = lineGradientNormal;

            foreach (GameObject obj in intersectionObjs)
            {
                StartCoroutine(FadeAndDestroySprite(obj, fadeDuration));
            }

            if (loopReady)
            {
                int crossings = activeIntersections.Count;
                Debug.Log($"Loop closed. Self-crossings: {crossings}");
                TriggerSpell(crossings);
            }
            else
            {
                Debug.Log("Not a closed loop.");
            }

            pendingIntersections.Clear();
            activeIntersections.Clear();
        }
    }
    bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    void AddPoint(Vector2 newPoint)
    {
        if (drawnPoints.Count > 0)
        {
            Vector2 prevPoint = drawnPoints[^1];

            for (int i = 0; i < drawnPoints.Count - 2; i++)
            {
                Vector2 a1 = drawnPoints[i];
                Vector2 a2 = drawnPoints[i + 1];

                if (DoLinesIntersect(a1, a2, prevPoint, newPoint, out _))
                {
                    Vector2 intersectionApprox = (a1 + a2 + prevPoint + newPoint) / 4f;

                    bool alreadyPending = pendingIntersections.Exists(p => Vector2.Distance(p, intersectionApprox) < 0.1f);
                    bool alreadyActive = activeIntersections.Exists(p => Vector2.Distance(p, intersectionApprox) < 0.1f);

                    if (!alreadyPending && !alreadyActive)
                    {
                        pendingIntersections.Add(intersectionApprox);
                    }
                }
            }
        }

        drawnPoints.Add(newPoint);
        lineRenderer.positionCount = drawnPoints.Count;
        lineRenderer.SetPosition(drawnPoints.Count - 1, newPoint);
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

    bool DoLinesIntersect(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, out Vector2 intersection)
    {
        intersection = Vector2.zero;

        // Early-out if segments share an endpoint
        if (a1 == b1 || a1 == b2 || a2 == b1 || a2 == b2)
            return false;

        float d = (a2.x - a1.x) * (b2.y - b1.y) - (a2.y - a1.y) * (b2.x - b1.x);
        if (Mathf.Approximately(d, 0))
            return false; // Lines are parallel or coincident

        float u = ((b1.x - a1.x) * (b2.y - b1.y) - (b1.y - a1.y) * (b2.x - b1.x)) / d;
        float v = ((b1.x - a1.x) * (a2.y - a1.y) - (b1.y - a1.y) * (a2.x - a1.x)) / d;

        if ((u > 0 && u < 1) && (v > 0 && v < 1))
        {
            intersection = a1 + u * (a2 - a1);
            return true;
        }

        return false;
    }

    void CloseLoopIfNeeded()
    {
        if (loopReady)
        {
            Vector2 finalPoint = drawnPoints[^1]; // Last point drawn

            // Add segment to close loop
            drawnPoints.Add(startPoint);
            lineRenderer.positionCount = drawnPoints.Count;
            lineRenderer.SetPosition(drawnPoints.Count - 1, startPoint);

            // Prepare to add the closing segment
            Vector2 closingStart = finalPoint;
            Vector2 closingEnd = startPoint;

            // Check for intersection BEFORE adding the point to drawnPoints
            for (int i = 0; i < drawnPoints.Count - 2; i++) // -2 so we skip the last segment
            {
                Vector2 a1 = drawnPoints[i];
                Vector2 a2 = drawnPoints[i + 1];

                // Skip if a2 is the same as closingStart (adjacent segment)
                if ((a1 == closingStart) || (a2 == closingStart))
                    continue;

                if (DoLinesIntersect(a1, a2, closingStart, closingEnd, out Vector2 intersectPoint))
                {
                    // Avoid spawns near endpoints of the closing segment
                    if (Vector2.Distance(intersectPoint, startPoint) > 0.01f &&
                        Vector2.Distance(intersectPoint, finalPoint) > 0.01f)
                    {
                        intersectionObjs.Add(Instantiate(intersectionPrefab, intersectPoint, intersectionPrefab.transform.rotation));
                        activeIntersections.Add(intersectPoint);
                    }
                }
            }

            for (int i = pendingIntersections.Count - 1; i >= 0; i--)
            {
                Vector2 intersectPoint = pendingIntersections[i];

                intersectionObjs.Add(Instantiate(intersectionPrefab, intersectPoint, intersectionPrefab.transform.rotation));
                activeIntersections.Add(intersectPoint);
                pendingIntersections.RemoveAt(i);
            }
        }
    }

    void SpawnAndFadeCloneLine()
    {
        if (lineRenderer.positionCount == 0) return;

        GameObject clone = Instantiate(fadingLinePrefab);
        LineRenderer cloneLR = clone.GetComponent<LineRenderer>();

        // Copy appearance
        cloneLR.widthMultiplier = lineRenderer.widthMultiplier;
        cloneLR.colorGradient = lineRenderer.colorGradient;
        cloneLR.positionCount = lineRenderer.positionCount;

        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            cloneLR.SetPosition(i, lineRenderer.GetPosition(i));
        }

        // Begin fade out
        FadingLineRenderer fading = clone.GetComponent<FadingLineRenderer>();
        fading.fadeDuration = fadeDuration;
        fading.StartFade(lineRenderer.colorGradient);
    }

    IEnumerator FadeAndDestroySprite(GameObject obj, float duration)
    {
        if (obj == null)
            yield break;

        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        Color originalColor = sr.color;
        float time = 0f;

        while (time < duration)
        {
            if (obj == null || obj.gameObject == null)
                yield break;

            float alpha = Mathf.Lerp(1f, 0f, time / duration);
            sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            time += Time.deltaTime;
            yield return null;
        }

        if (obj != null && obj.gameObject != null)
        {
            Destroy(obj);
        }
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
