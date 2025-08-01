using UnityEngine;

public class Fireball : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 3f;

    void Start()
    {
        Destroy(gameObject, lifetime); // Prevent lingering fireballs
    }

    void Update()
    {
        transform.position += speed * Time.deltaTime * transform.right;
    }
}
