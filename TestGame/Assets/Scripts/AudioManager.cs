using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public AudioMixer audioMixer;
    public float defaultVolume = 0.7f;

    public AudioClip backgroundMusic;

    void Start()
    {
        float savedVolume = PlayerPrefs.HasKey("MasterVolume") ? PlayerPrefs.GetFloat("MasterVolume") : defaultVolume;
        SetVolume(savedVolume);
    }

    public void SetVolume(float volume)
    {
        // Convert from 0–1 slider value to decibels
        float dB = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat("MasterVolume", dB);

        // Save the linear volume (0–1), not the dB
        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();
    }
}
