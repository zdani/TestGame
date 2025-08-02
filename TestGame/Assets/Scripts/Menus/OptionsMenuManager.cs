using UnityEngine;

public class OptionsMenuManager : MonoBehaviour
{
    public GameObject optionsMenu;   // Assign this in the Inspector
    public GameObject controlsMenu;  // Assign this in the Inspector

    private bool isGamePaused = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscapePress();
        }
    }

    private void HandleEscapePress()
    {
        if (controlsMenu.activeSelf)
        {
            // If in controls menu, return to options menu
            controlsMenu.SetActive(false);
            optionsMenu.SetActive(true);
        }
        else if (optionsMenu.activeSelf)
        {
            // If in options menu, close everything and unpause
            optionsMenu.SetActive(false);
            Time.timeScale = 1f;
            isGamePaused = false;
        }
        else
        {
            // If nothing is open, open options and pause
            optionsMenu.SetActive(true);
            Time.timeScale = 0f;
            isGamePaused = true;
        }
    }

    public void ShowControlsMenu()
    {
        optionsMenu.SetActive(false);
        controlsMenu.SetActive(true);
    }

    public void ResumeGame()
    {
        optionsMenu.SetActive(false);
        controlsMenu.SetActive(false);
        Time.timeScale = 1f;
        isGamePaused = false;
    }
}
