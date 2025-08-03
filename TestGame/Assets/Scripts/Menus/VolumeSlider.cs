using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    public AudioMixer audioMixer;
    public float defaultVolume = 0.7f;
    private Slider volumeSlider;

    void Start()
    {
        volumeSlider = GetComponent<Slider>();

        // Load saved volume or default to 0.7
        float savedVolume = PlayerPrefs.HasKey("MasterVolume") ? PlayerPrefs.GetFloat("MasterVolume") : defaultVolume;
        volumeSlider.value = savedVolume;
        SetVolume(savedVolume); // Apply volume to mixer
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
