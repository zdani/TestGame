using UnityEngine;

public class OptionsMenuManager : MonoBehaviour
{
    public GameObject optionsPanel;   // Assign this in the Inspector
    public GameObject controlsPanel;  // Assign this in the Inspector

    private bool isGamePaused = false; // May use later

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleOptionsMenu();
        }
    }

    public void ToggleOptionsMenu()
    {
        if (controlsPanel.activeSelf)
        {
            // If in controls menu, return to options menu
            controlsPanel.SetActive(false);
            optionsPanel.SetActive(true);
        }
        else if (optionsPanel.activeSelf)
        {
            // If in options menu, close everything and unpause
            optionsPanel.SetActive(false);
            Time.timeScale = 1f;
            isGamePaused = false;
        }
        else
        {
            // If nothing is open, open options and pause
            optionsPanel.SetActive(true);
            Time.timeScale = 0f;
            isGamePaused = true;
        }
    }

    public void ResumeGame() // Haven't implemented Resume button in options menu yet
    {
        optionsPanel.SetActive(false);
        controlsPanel.SetActive(false);
        Time.timeScale = 1f;
        isGamePaused = false;
    }
}
