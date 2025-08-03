using UnityEngine;

public class PlayOnAwake : MonoBehaviour
{
    public AudioSource audioSource;

    void Awake()
    {
        if (audioSource != null)
        {
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("AudioSource is not assigned on " + gameObject.name);
        }
    }
}
