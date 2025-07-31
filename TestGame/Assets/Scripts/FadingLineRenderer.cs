using System.Collections;
using UnityEngine;

public class FadingLineRenderer : MonoBehaviour
{
    public float fadeDuration = 1f;
    private LineRenderer lineRenderer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void StartFade(Gradient sourceGradient)
    {
        StartCoroutine(FadeOut(sourceGradient));
    }

    IEnumerator FadeOut(Gradient gradient)
    {
        float time = 0f;
        GradientColorKey[] colorKeys = gradient.colorKeys;
        GradientAlphaKey[] alphaKeys = gradient.alphaKeys;

        while (time < fadeDuration)
        {
            float t = time / fadeDuration;
            float alpha = Mathf.Lerp(1f, 0f, t);

            GradientAlphaKey[] newAlphaKeys = new GradientAlphaKey[alphaKeys.Length];
            for (int i = 0; i < alphaKeys.Length; i++)
            {
                newAlphaKeys[i] = new GradientAlphaKey(alpha, alphaKeys[i].time);
            }

            Gradient faded = new Gradient();
            faded.SetKeys(colorKeys, newAlphaKeys);
            lineRenderer.colorGradient = faded;

            time += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
