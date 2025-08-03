using UnityEngine;
using System.Collections;

public class Boulder : MonoBehaviour
{
    public float shakeDuration = 0.5f;
    public float slamPause = 0.2f;
    public float lingerDuration = 0.8f;
    public float fadeOutDuration = 0.2f;
    public AudioClip slamSound;
    public LayerMask groundLayer;

    private Vector3 originalPos;
    private SpriteRenderer sr;
    private AudioSource audioSource;
    private Rigidbody2D rb;
    private Collider2D col;
    private bool hasLanded = false;
    private bool isFalling = false;
    private bool hasExitedGroundCollider = false;
    private const float overlapThreshold = 0.05f; // Allow tiny overlap due to physics imprecision

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        rb.bodyType = RigidbodyType2D.Static; // The boulder is unaffected by gravity while static
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void Start()
    {
        audioSource.PlayOneShot(slamSound);

        StartCoroutine(ShakeAndDrop());
    }

    private IEnumerator ShakeAndDrop()
    {
        Vector3 startPos = transform.position;

        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float xOffset = Mathf.Sin(Time.time * 50f) * 0.05f;
            transform.position = startPos + new Vector3(xOffset, 0f, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = startPos; // Reset position
        rb.bodyType = RigidbodyType2D.Dynamic; // Switch to dynamic to allow falling
        isFalling = true;
    }

    private void Update()
    {
        if (isFalling && !hasExitedGroundCollider)
        {
            Collider2D overlap = Physics2D.OverlapCapsule(
                col.bounds.center,
                col.bounds.size,
                CapsuleDirection2D.Vertical,
                0f,
                groundLayer
            );
            if (overlap == null)
            {
                hasExitedGroundCollider = true;
                Debug.Log("[Boulder] No overlap with ground, exited ground collider. Ready to land on floor.");
            }
            else
            {
                var dist = col.Distance(overlap);
                Debug.Log($"[Boulder] Still overlapping ground: {overlap.name}, overlap distance: {dist.distance}");
                // Allow a small overlap threshold (e.g., -0.05f)
                if (dist.isOverlapped && dist.distance > -0.05f)
                {
                    hasExitedGroundCollider = true;
                    Debug.Log("[Boulder] Overlap is small, treating as exited ground collider. Ready to land on floor.");
                }
            }
        }
        else if (isFalling && hasExitedGroundCollider && !hasLanded)
        {
            Collider2D overlap = Physics2D.OverlapCapsule(
                col.bounds.center,
                col.bounds.size,
                CapsuleDirection2D.Vertical,
                0f,
                groundLayer
            );
            if (overlap != null)
            {
                var dist = col.Distance(overlap);
                // Treat as landed only if the overlap normal is pointing upward (floor beneath the boulder)
                if (dist.isOverlapped && dist.normal.y > 0.2f)
                {
                    Land(overlap);
                }
            }
            else
            {
                Debug.Log("[Boulder] No ground detected for landing.");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasLanded) return;

        // Check if the boulder has hit the ground
        if ((groundLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            // Only allow landing if the boulder has exited the ceiling (ground collider) at least once
            if (isFalling && hasExitedGroundCollider)
            {
                var dist = col.Distance(other);
                if (dist.isOverlapped && dist.normal.y > 0.2f)
                {
                    Land(other);
                }
            }
        }
    }

    private void Land(Collider2D overlap)
    {
        Debug.Log($"[Boulder] Landed on ground: {overlap.name}");
        hasLanded = true;
        isFalling = false;
        rb.bodyType = RigidbodyType2D.Static;
        StartCoroutine(FadeAndDestroy());
    }

    private IEnumerator FadeAndDestroy()
    {
        yield return new WaitForSeconds(lingerDuration);

        float startAlpha = sr.color.a;
        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            float alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeOutDuration);
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
