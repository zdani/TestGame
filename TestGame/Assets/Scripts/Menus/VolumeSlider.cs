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
    }

    public void SetVolume(float volume)
    {
        // Convert from 0–1 slider value to decibels
        float dB = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat("MasterVolume", dB);
        //Debug.Log($"Volume set to {volume} (dB: {dB})");

        // Save the linear volume (0–1), not the dB
        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();
    }
}
