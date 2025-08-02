using UnityEngine;

public class ControlsMenu : MonoBehaviour
{
    public GameObject optionsPanel;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            optionsPanel.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}