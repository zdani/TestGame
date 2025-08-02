using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 5f;
    public float damage = 1f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void Initialize(Vector2 direction)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction.normalized * speed;

        // Flip the sprite based on direction
        if (direction.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

}
