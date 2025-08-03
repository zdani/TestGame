using System.Collections;
using UnityEngine;

public class StartMenu : MonoBehaviour
{
    public GameObject blackPanel;   // Assign in inspector
    public GameObject title;        // Assign in inspector
    public GameManager gameManager; // Assign in inspector

    private CanvasGroup blackCanvas;
    private CanvasGroup titleCanvas;

    public float fadeInDuration = 1f;
    public float fadeOutDuration = 3f;

    void Start()
    {
        // Ensure both objects are active
        blackPanel.SetActive(true);
        title.SetActive(true);

        // Add CanvasGroup components if missing
        blackCanvas = blackPanel.GetComponent<CanvasGroup>();
        if (blackCanvas == null)
            blackCanvas = blackPanel.AddComponent<CanvasGroup>();
        blackCanvas.alpha = 1f;

        titleCanvas = title.GetComponent<CanvasGroup>();
        if (titleCanvas == null)
            titleCanvas = title.AddComponent<CanvasGroup>();
        titleCanvas.alpha = 0f;

        // Start the sequence
        StartCoroutine(FadeInSequence());
    }

    IEnumerator FadeInSequence()
    {
        // Fade out black panel
        yield return StartCoroutine(FadeCanvasGroup(blackCanvas, 1f, 0f, fadeInDuration));

        // Fade in title
        yield return StartCoroutine(FadeCanvasGroup(titleCanvas, 0f, 1f, fadeInDuration));
    }

    public void FadeOut()
    {
        // Start the fade out sequence
        StartCoroutine(FadeOutSequence());
    }

    IEnumerator FadeOutSequence()
    {
        // Fade in black panel
        yield return StartCoroutine(FadeCanvasGroup(blackCanvas, 0f, 1f, fadeOutDuration));

        gameManager.StartGame();
    }

    IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        float timer = 0f;
        cg.alpha = from;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, timer / duration);
            yield return null;
        }

        cg.alpha = to;
    }
}
