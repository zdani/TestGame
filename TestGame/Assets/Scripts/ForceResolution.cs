using UnityEngine;

public class ForceResolution : MonoBehaviour
{
    void Start()
    {
        // Force 1920x1080 resolution in windowed mode
        Screen.SetResolution(1920, 1080, false);
    }
}
