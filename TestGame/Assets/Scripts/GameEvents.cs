using System;
using UnityEngine;

// Singleton class to manage all game events
// ExecuteInEditMode ensures this runs in edit mode too
// Execution order -1000 ensures this runs before most other scripts
[DefaultExecutionOrder(-1000)]
public class GameEvents : MonoBehaviour
{
    // Singleton instance
    public static GameEvents Instance { get; private set; }
    
    // Flag to track if we're shutting down
    private static bool isQuitting = false;

    // Player events
    public event Action<Ability> OnAbilityCast;
    public event Action OnPlayerHit;
    public event Action OnInvincibilityStarted;
    public event Action OnInvincibilityEnded;
    public event Action OnPlayerDied;

    // Health events
    public event Action<float> OnHealthChanged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        // Create GameEvents before any scene loads
        if (Instance == null && !isQuitting)
        {
            GameObject gameEventsObject = new GameObject("GameEvents");
            Instance = gameEventsObject.AddComponent<GameEvents>();
            DontDestroyOnLoad(gameEventsObject);
        }
    }

    private void Awake()
    {
        // Simple singleton pattern - fail fast if duplicates exist

        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("GameEvents singleton initialized successfully");
    }

    private void OnApplicationQuit()
    {
        // Mark that we're quitting to prevent new instances
        isQuitting = true;
    }

    private void OnDestroy()
    {
        // Only clear the instance if this is the actual singleton instance
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // Player event triggers
    public void TriggerAbilityCast(Ability ability)
    {
        OnAbilityCast?.Invoke(ability);
    }

    public void TriggerPlayerHit()
    {
        OnPlayerHit?.Invoke();
    }

    public void TriggerInvincibilityStarted()
    {
        OnInvincibilityStarted?.Invoke();
    }

    public void TriggerInvincibilityEnded()
    {
        OnInvincibilityEnded?.Invoke();
    }

    public void TriggerPlayerDied()
    {
        OnPlayerDied?.Invoke();
    }

    // Health event triggers
    public void TriggerHealthChanged(float currentHealth)
    {
        OnHealthChanged?.Invoke(currentHealth);
    }
} 