using UnityEngine;

public class FireballProjectile : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 3f;

    public Sprite[] animationFrames; // Size = 2 in Inspector
    public float animationSpeed = 0.1f; // Time between frames

    private SpriteRenderer spriteRenderer;
    private int currentFrame;
    private float animationTimer;

    void Start()
    {
        Destroy(gameObject, lifetime); // Prevent lingering fireballs

        spriteRenderer = GetComponent<SpriteRenderer>();
        animationTimer = animationSpeed;
        currentFrame = 0;

        if (animationFrames != null && animationFrames.Length > 0)
        {
            spriteRenderer.sprite = animationFrames[0];
        }
    }

        public void Initialize(Vector2 shootDirection)
    {
        Vector2 direction = shootDirection.normalized;

        // Flip sprite if shooting left
        if (direction.x < 0)
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }

        // Apply velocity
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction * speed;
    }

    void Update()
    {
        if (animationFrames == null || animationFrames.Length < 2) return;

        animationTimer -= Time.deltaTime;

        if (animationTimer <= 0f)
        {
            currentFrame = (currentFrame + 1) % animationFrames.Length;
            spriteRenderer.sprite = animationFrames[currentFrame];
            animationTimer = animationSpeed;
        }
    }
}