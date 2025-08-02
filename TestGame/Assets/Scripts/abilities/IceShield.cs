using System.Collections;
using UnityEngine;

public class IceShield : MonoBehaviour
{
    public Sprite[] animationFrames; // Size = 2 in Inspector
    public float animationSpeed = 0.5f; // Time between frames
    public AudioClip breakSound;

    private SpriteRenderer spriteRenderer;
    private IceShieldAbility iceShieldAbility;
    private readonly float fadeOutDuration = 1f;
    private Coroutine fadeCoroutine;

    public void Initialize(float duration)
    {
        if (duration > 0f)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            iceShieldAbility = transform.parent.GetComponent<IceShieldAbility>();

            fadeCoroutine = StartCoroutine(ShieldLifeRoutine(duration));
        }
    }

    private IEnumerator ShieldLifeRoutine(float delayBeforeFade)
    {
        yield return new WaitForSeconds(delayBeforeFade);

        Color color = spriteRenderer.color;
        float startAlpha = color.a;
        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeOutDuration);
            float newAlpha = Mathf.Lerp(startAlpha, 0f, t);
            spriteRenderer.color = new Color(color.r, color.g, color.b, newAlpha);
            yield return null;
        }

        spriteRenderer.color = new Color(color.r, color.g, color.b, 0f);
        iceShieldAbility.isShieldActive = false;
        Destroy(gameObject);
    }

    public IEnumerator BreakShield()
    {
        if (animationFrames == null || animationFrames.Length < 2)
        {
            Debug.LogWarning("Animation frames not set on IceShield component in inspector.");
            yield break;
        }

        iceShieldAbility.isShieldActive = false;
        
        if (breakSound != null)
        {
            AudioSource.PlayClipAtPoint(breakSound, transform.position);
        }

        transform.parent = null; // Detach from player so it stops rotating with them
        StopCoroutine(fadeCoroutine); // Stop the fade coroutine if it started
        fadeCoroutine = null;

        spriteRenderer.sprite = animationFrames[0];
        yield return new WaitForSeconds(animationSpeed);
        spriteRenderer.sprite = animationFrames[1];
        yield return new WaitForSeconds(animationSpeed);
        Destroy(gameObject);
    }

    // Just for testing, remove later
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            StartCoroutine(BreakShield());
        }
    }
}
