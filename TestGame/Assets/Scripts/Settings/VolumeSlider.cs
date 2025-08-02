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
        if (volumeSlider != null)
        {
            volumeSlider = GetComponent<Slider>();
            SetVolume(defaultVolume);
        }
    }

    public void SetVolume(float sliderValue)
    {
        float dB = Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20f; // Converts the slider's scale of 0-1 to a logarithmic dB scale
        audioMixer.SetFloat("MasterVolume", dB);
    }
}
