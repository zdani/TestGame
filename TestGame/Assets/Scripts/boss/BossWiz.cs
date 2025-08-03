using UnityEngine;
using System.Collections;

public class BossWiz : Enemy
{
    public override float DamageAmount => 1f;

    [Header("Boss Settings")]
    private Transform[] teleportPoints;
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    public GameObject smokePuffPrefab;

    private Rigidbody2D rb;
    private int lastTeleportIndex = -1;
    private Transform headTarget;
    private bool isDead = false;
    private Animator animator;

    protected override void Start()
    {
        maxHealth = 15f;
        base.Start();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        
        headTarget = playerTransform.Find("Head");

        // Find teleport points
        GameObject teleportParent = GameObject.Find("TeleportationPoints");
        if (teleportParent != null)
        {
            int pointCount = teleportParent.transform.childCount;
            teleportPoints = new Transform[pointCount];
            for (int i = 0; i < pointCount; i++)
            {
                teleportPoints[i] = teleportParent.transform.GetChild(i);
            }
            Debug.Log($"BossWiz loaded with {pointCount} teleport points.");
        }
        else
        {
            Debug.LogWarning("BossWiz could not find a 'TeleportationPoints' parent object. Teleportation will be disabled.");
            teleportPoints = new Transform[0]; // Initialize to prevent null reference errors
        }

        StartCoroutine(BehaviorCycle());
    }

    private IEnumerator BehaviorCycle()
    {
        // Initial delay before the first action
        yield return new WaitForSeconds(1f);

        while (!isDead)
        {
            yield return StartCoroutine(TeleportSequence());
            
            // First attack with casting
            yield return new WaitForSeconds(1f);
            yield return StartCoroutine(PerformCastingAttack());

            // Second attack with casting
            yield return new WaitForSeconds(2.5f); // Increased delay
            yield return StartCoroutine(PerformCastingAttack());

            yield return new WaitForSeconds(3f);
        }
    }

    private IEnumerator PerformCastingAttack()
    {
        if (animator != null) animator.SetBool("IsCasting", true);
        yield return new WaitForSeconds(0.7f);
        PerformAttack();
        yield return new WaitForSeconds(0.3f);
        if (animator != null) animator.SetBool("IsCasting", false);
    }

    private IEnumerator TeleportSequence()
    {
        if (teleportPoints == null || teleportPoints.Length == 0)
        {
            yield break; // Can't teleport if there are no points
        }
        
        // --- Disappear sequence ---
        SpawnSmokePuff();
        SetVisibility(false);
        yield return new WaitForSeconds(0.2f);

        // --- Move to new location ---
        int nextTeleportIndex = lastTeleportIndex;
        if (teleportPoints.Length > 1)
        {
            while (nextTeleportIndex == lastTeleportIndex)
            {
                nextTeleportIndex = Random.Range(0, teleportPoints.Length);
            }
        }
        else
        {
            nextTeleportIndex = 0;
        }
            
        lastTeleportIndex = nextTeleportIndex;
        rb.position = teleportPoints[nextTeleportIndex].position;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // Wait for 1 second before reappearing
        yield return new WaitForSeconds(1f);

        // --- Reappear sequence ---
        SpawnSmokePuff();
        yield return new WaitForSeconds(0.5f);
        SetVisibility(true);
    }

    private void SpawnSmokePuff()
    {
        if (smokePuffPrefab == null) return;
        
        GameObject smokePuff = Instantiate(smokePuffPrefab, transform.position, Quaternion.identity);
        var bossRenderer = GetComponent<Renderer>();
        if (bossRenderer != null)
        {
            foreach (var smokeRenderer in smokePuff.GetComponentsInChildren<Renderer>())
            {
                smokeRenderer.sortingLayerName = bossRenderer.sortingLayerName;
                smokeRenderer.sortingOrder = bossRenderer.sortingOrder + 1;
            }
        }
    }

    private void SetVisibility(bool isVisible)
    {
        foreach (var r in GetComponentsInChildren<Renderer>())
        {
            r.enabled = isVisible;
        }
    }

    private void PerformAttack()
    {
        if (headTarget != null)
        {
            Vector2 direction = (headTarget.position - projectileSpawnPoint.position).normalized;
            GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
            projectile.GetComponent<BossProjectile>().Initialize(direction);
        }
    }

    protected override void OnDeath()
    {
        base.OnDeath();
        isDead = true;
        StopAllCoroutines();
        // Add any death effects or logic here
    }

    protected override void OnPlayerCollision(Player player)
    {
        // Boss might not damage on touch, relies on projectiles
    }
}
