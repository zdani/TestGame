using System.Collections;
using UnityEngine;

public class IceShield : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private IceShieldAbility iceShieldAbility;
    private readonly float fadeOutDuration = 1f;

    public void Initialize(float duration)
    {
        if (duration > 0f)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            iceShieldAbility = transform.parent.GetComponent<IceShieldAbility>();
            Debug.Log("parent ice shield ability found: " + iceShieldAbility);

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

        spriteRenderer.color = new Color(color.r, color.g, color.b, 0f);
        iceShieldAbility.isShieldActive = false;
        Destroy(gameObject);
    }
}
