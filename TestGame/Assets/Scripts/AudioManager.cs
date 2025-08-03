using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider volumeSlider;
    public float defaultVolume = 0.7f;

    public AudioClip backgroundMusic;
    public AudioClip bossMusic;
    public float fadeOutDuration = 0.3f;
    public float delayBeforeBossMusic = 1f;

    private float volumeBeforeFade;

    private AudioSource backgroundAudioSource;

    void Start()
    {
        backgroundAudioSource = GetComponent<AudioSource>();

        float savedVolume = PlayerPrefs.HasKey("MasterVolume") ? PlayerPrefs.GetFloat("MasterVolume") : defaultVolume;
        SetVolume(savedVolume);

        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene != "Start Game Menu")
        {
            PlayLoopedMusic(backgroundAudioSource, backgroundMusic);
        }
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

    public void StartBossMusic()
    {
        StartCoroutine(FadeOutThenPlayBossMusic());
    }
    
    private IEnumerator FadeOutThenPlayBossMusic()
    {
        volumeBeforeFade = backgroundAudioSource.volume;
        float t = 0f;

        // Fade out
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            backgroundAudioSource.volume = Mathf.Lerp(volumeBeforeFade, 0f, t / fadeOutDuration);
            yield return null;
        }

        backgroundAudioSource.Stop();
        backgroundAudioSource.clip = bossMusic;

        // Wait before starting boss music
        yield return new WaitForSeconds(delayBeforeBossMusic);

        // Start boss music instantly at desired volume
        backgroundAudioSource.volume = volumeBeforeFade;
        backgroundAudioSource.loop = true;
        backgroundAudioSource.Play();
    }

    private void PlayLoopedMusic(AudioSource source, AudioClip musicClip)
    {
        source.clip = musicClip;
        source.loop = true;
        source.Play();
    }
}
