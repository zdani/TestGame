using UnityEngine;

public class FireballProjectile : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 3f;
    private Vector2 direction;

    void Start()
    {
        Destroy(gameObject, lifetime); // Prevent lingering fireballs
    }
    
    public void Initialize(Vector2 shootDirection)
    {
        direction = shootDirection.normalized;

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
}