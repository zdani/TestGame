using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SlideshowCrossfade : MonoBehaviour
{
    [Header("Assign these in the Inspector")]
    public Image imageA;
    public Image imageB;
    public Sprite[] slides;
    public CanvasGroup thanksForPlayingPanel;

    [Header("Settings")]
    public float slideDuration = 3f;    // How long each slide stays fully visible
    public float fadeDuration = 1f;     // Duration of crossfade

    private int currentIndex = 0;
    private bool usingImageA = true;

    void Start()
    {
        if (slides.Length == 0 || imageA == null || imageB == null)
        {
            Debug.LogError("SlideshowCrossfade: Missing images or slides.");
            return;
        }

        // Initialize
        imageA.sprite = slides[0];
        imageA.color = Color.white;
        imageB.color = new Color(1, 1, 1, 0);

        StartCoroutine(PlaySlideshow());
    }

    IEnumerator PlaySlideshow()
    {
        for (int i = 1; i < slides.Length; i++)
        {
            yield return new WaitForSeconds(slideDuration);

            currentIndex = (currentIndex + 1) % slides.Length;

            Image fadeOutImage = usingImageA ? imageA : imageB;
            Image fadeInImage = usingImageA ? imageB : imageA;

            fadeInImage.sprite = slides[currentIndex];

            float t = 0;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                float alpha = t / fadeDuration;

                fadeOutImage.color = new Color(1, 1, 1, 1 - alpha);
                fadeInImage.color = new Color(1, 1, 1, alpha);
                yield return null;
            }

            // Make sure values are final
            fadeOutImage.color = new Color(1, 1, 1, 0);
            fadeInImage.color = new Color(1, 1, 1, 1);

            usingImageA = !usingImageA;
        }

        StartCoroutine(FadeInPanel(thanksForPlayingPanel, 1f));
    }

    IEnumerator FadeInPanel(CanvasGroup panel, float duration)
    {
        yield return new WaitForSeconds(slideDuration);

        float t = 0;
        panel.gameObject.SetActive(true); // In case it was disabled
        panel.alpha = 0;
        panel.interactable = false;
        panel.blocksRaycasts = false;

        while (t < duration)
        {
            t += Time.deltaTime;
            panel.alpha = t / duration;
            yield return null;
        }

        panel.alpha = 1;
        panel.interactable = true;
        panel.blocksRaycasts = true;
    }

    public void BackToStartMenu()
    {
        SceneManager.LoadScene("StartGame");
    }
}
