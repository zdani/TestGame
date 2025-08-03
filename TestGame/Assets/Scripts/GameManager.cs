using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Game Over Settings")]
    [SerializeField] private GameObject gameOverGameObject;
    
    [Header("Scene Management")]
    [SerializeField] private string level1SceneName = "level1"; // Change this to your actual level 1 scene name


    private void Start()
    {
        Debug.Log("GameManager Start() called - script is working!");
        
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

    public void OnGameOver()
    {
        Debug.Log("Game Over triggered! Enabling game over UI.");
        Player player = FindFirstObjectByType<Player>();
        player.gameObject.SetActive(false);
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
    
    // Scene Management Methods
    public void StartGame()
    {
        Debug.Log("=== BUTTON CLICKED! StartGame() method called ===");
        Debug.Log("Button clicked! Starting game... Loading scene: " + level1SceneName);
        
        // Test without scene loading first
        //Debug.Log("Testing button functionality - scene loading disabled for now");
        SceneManager.LoadScene(level1SceneName);
    }
    
    // Simple test method for debugging
    public void TestButtonClick()
    {
        Debug.Log("=== SIMPLE TEST BUTTON CLICKED! ===");
    }
    
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    // Function to hide the ControlsPanelForStart GameObject
    public void HideControlsPanelForStart()
    {
        var controlsPanelForStart = GameObject.Find("ControlsPanelForStart");
 
        if (controlsPanelForStart != null)
        {
            controlsPanelForStart.SetActive(false);
        }
        else
        {
            Debug.LogWarning("ControlsPanelForStart GameObject is not found in the scene!");
        }
    }
}
