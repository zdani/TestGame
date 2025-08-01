using System.Collections;
using UnityEngine;

public class IceShield : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private readonly float fadeOutDuration = 1f;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("IceShield: No SpriteRenderer found!");
        }
    }

    public void Initialize(float duration)
    {
        if (duration > 0f)
        {
            StartCoroutine(ShieldLifeRoutine(duration));
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

        // Ensure it's fully transparent
        spriteRenderer.color = new Color(color.r, color.g, color.b, 0f);

        Destroy(gameObject);
    }
}
