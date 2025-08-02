using UnityEngine;

public class OptionsMenu : MonoBehaviour
{
    public GameObject backgroundDimmerObj;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            backgroundDimmerObj.SetActive(false);
            gameObject.SetActive(false);
        }
    }
}
