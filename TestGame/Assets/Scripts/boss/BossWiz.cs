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
    private Collider2D bossCollider;

    private bool isImmuneToContactDamage = false;

    public override bool CanDealContactDamage => !isImmuneToContactDamage;

    protected override void Start()
    {
        maxHealth = 15f;
        base.Start();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        bossCollider = GetComponent<Collider2D>();
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
    }
    
    public void StartBehaviorCycle() // This method is triggered by the collider on BossTriggerZone
    {
        if (isDead) return; // Don't start if already dead
        StopAllCoroutines(); // Stop any existing behavior cycles

        // Immediately disable contact damage immunity when the cycle restarts to prevent it from getting stuck.
        isImmuneToContactDamage = false;

        StartCoroutine(BehaviorCycle());
    }

    private IEnumerator BehaviorCycle()
    {
        // Initial delay before the first action
        yield return new WaitForSeconds(3f);

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
        bossCollider.enabled = false;

        // --- Disappear sequence ---
        SpawnSmokePuff();
        yield return new WaitForSeconds(0.2f); // Short delay for smoke to appear before vanishing
        SetVisibility(false);

        // Check if we can teleport AFTER disappearing
        if (teleportPoints == null || teleportPoints.Length == 0)
        {
            Debug.LogWarning("BossWiz cannot teleport; no points. Reappearing in place.");
            yield return new WaitForSeconds(1f); // Wait a bit before reappearing
            SetVisibility(true);
            bossCollider.enabled = true;
            yield break;
        }

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

        // Wait a bit at new location before starting the reappear sequence
        yield return new WaitForSeconds(1f);

        // --- Reappear sequence ---
        SpawnSmokePuff();
        yield return new WaitForSeconds(0.5f);
        SetVisibility(true);
        bossCollider.enabled = true; // Enable collider AFTER becoming visible

        // --- Grace period ---
        // Boss is visible and can be damaged, but won't deal contact damage for a short time
        isImmuneToContactDamage = true;
        Debug.Log("No contact damage");
        yield return new WaitForSeconds(3f); // 4.5s + 2s requested grace period
        isImmuneToContactDamage = false;
        Debug.Log("contact damage reenabled");
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

    // OnCollisionStay2D and OnTriggerEnter2D are no longer needed here.
    // The base Enemy class handles the CanDealContactDamage check, which is controlled
    // by the isImmuneToContactDamage flag in this script.
}
