using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Game Over Settings")]
    [SerializeField] private GameObject gameOverGameObject;

    private void Start()
    {
        // Ensure game over UI is hidden at start
        if (gameOverGameObject != null)
        {
            gameOverGameObject.SetActive(false);
        }

        // Subscribe to player death event
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnPlayerDied += OnGameOver;
        }
        else
        {
            Debug.LogError("GameEvents instance not found! GameManager cannot listen for events.");
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnPlayerDied -= OnGameOver;
        }
    }

    private void OnGameOver()
    {
        Debug.Log("Game Over triggered! Enabling game over UI.");
        
        if (gameOverGameObject != null)
        {
            gameOverGameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Game Over GameObject is not assigned in GameManager!");
        }
    }

    // Public method to manually trigger game over (useful for testing)
    public void TriggerGameOver()
    {
        OnGameOver();
    }

    // Public method to reset game over state
    public void ResetGameOver()
    {
        if (gameOverGameObject != null)
        {
            gameOverGameObject.SetActive(false);
        }
    }
}
