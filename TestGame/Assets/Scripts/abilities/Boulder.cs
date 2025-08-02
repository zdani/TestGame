using UnityEngine;
using System.Collections;

public class Boulder : MonoBehaviour
{
    public float shakeDuration = 0.5f;
    public float slamPause = 0.2f;
    public float lingerDuration = 0.8f;
    public float fadeOutDuration = 0.2f;
    public AudioClip slamSound;

    private Vector3 originalPos;
    private SpriteRenderer sr;
    private AudioSource audioSource;
    private Rigidbody2D rb;
    private Collider2D col;
    private bool hasLanded = false;
    private bool isFalling = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        rb.bodyType = RigidbodyType2D.Static;
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
        rb.bodyType = RigidbodyType2D.Dynamic;
        isFalling = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasLanded) return;

        if (isFalling)
        {
            /*
            if (((1 << collision.gameObject.layer) & damageableLayers) != 0)
            {
                Health targetHealth = collision.gameObject.GetComponent<Health>();
                if (targetHealth != null)
                {
                    targetHealth.TakeDamage(damageAmount);
                }
            }
            */

            // Stop damaging and start fade
            hasLanded = true;
            isFalling = false;
            rb.bodyType = RigidbodyType2D.Static;

            StartCoroutine(FadeAndDestroy());
        }
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
