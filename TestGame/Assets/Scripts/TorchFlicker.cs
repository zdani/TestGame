using UnityEngine;

public class TorchFlicker : MonoBehaviour
{
    public Sprite frame1;
    public Sprite frame2;
    public float flickerRate = 0.2f;

    private SpriteRenderer spriteRenderer;
    private float timer;
    private bool showingFrame1 = true;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = frame1;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= flickerRate)
        {
            spriteRenderer.sprite = showingFrame1 ? frame2 : frame1;
            showingFrame1 = !showingFrame1;
            timer = 0f;
        }
    }
}
