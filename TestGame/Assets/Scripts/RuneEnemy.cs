using System.Collections;
using UnityEngine;

public class RuneEnemy : Enemy
{
    [Header("Rune Settings")]
    [SerializeField] private float chargeSpeed = 15f;
    [SerializeField] private float chargeDelay = 3f;
    [SerializeField] private float proximityThreshold = 0.5f; // How close to get before it's a guaranteed hit.

    [Header("Effects")]
    [SerializeField] private ParticleSystem chargeEffect;

    // Static variable to track the last time any rune enemy attacked.
    // This is shared across all instances of RuneEnemy.
    private static float lastAttackTime = -10f;
    private const float ATTACK_COOLDOWN = 5f;

    private bool isCharging = false;
    private Rigidbody2D rb;
    private Renderer rend;
    private Camera mainCamera;

    public override float DamageAmount => 2f;

    protected override void Start()
    {
        maxHealth = 1f;
        base.Start();

        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        rend = GetComponent<Renderer>();
        mainCamera = Camera.main;
    }

    protected override void Update()
    {
        // Don't use base.Update() or playerDetected.
        // Instead, we check if the rune is visible to the main camera.

        if (isCharging || rend == null || mainCamera == null)
        {
            return;
        }

        // Check if this rune is inside the camera's view frustum
        var planes = GeometryUtility.CalculateFrustumPlanes(mainCamera);
        if (GeometryUtility.TestPlanesAABB(planes, rend.bounds))
        {
            // Check if enough time has passed since the last rune attack
            if (Time.time >= lastAttackTime + ATTACK_COOLDOWN)
            {
                if (playerTransform != null) // Make sure we have a player to charge at
                {
                    StartCoroutine(ChargeAttack());
                }
            }
        }
    }

    private IEnumerator ChargeAttack()
    {
        isCharging = true;
        
        // Set the static attack time for all other runes
        lastAttackTime = Time.time;

        // Play the charge effect if it exists
        if (chargeEffect != null)
        {
            chargeEffect.Play();
        }
        
        // Pause for the duration of the charge.
        yield return new WaitForSeconds(chargeDelay);

        // Stop the charge effect after the delay.
        if (chargeEffect != null)
        {
            chargeEffect.Stop();
        }

        while (true) // This loop runs every physics frame until the object is destroyed
        {
            if (playerTransform == null || !playerTransform.gameObject.activeInHierarchy)
            {
                // Player is gone (e.g., died), so self-destruct
                Die();
                yield break; // Exit the coroutine
            }

            // --- Proximity Check ---
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer < proximityThreshold)
            {
                // We are close enough to guarantee a hit.
                if (playerTransform.TryGetComponent<Player>(out var player))
                {
                    player.TakeDamage(DamageAmount, enemyName);
                }
                Die(); // Self-destruct
                yield break; // Exit the coroutine
            }

            // --- Homing Logic ---
            // Continuously update the direction to home in on the player
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            rb.linearVelocity = direction * chargeSpeed;

            yield return new WaitForFixedUpdate(); // Wait for the next physics frame
        }
    }

    protected override void OnPlayerCollision(Player player)
    {
        // The base class handles dealing damage, we just need to destroy this object
        Die();
    }
    
    protected override void OnDeath()
    {
        base.OnDeath();
        Die();
    }
    
    private void Die()
    {
        // Add any death effects like explosions or sounds here
        Destroy(gameObject);
    }
}
