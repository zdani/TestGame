using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    [Header("Mixer / UI")]
    public AudioMixer audioMixer;
    public Slider volumeSlider;
    public float defaultVolume = 0.7f; // 75% ~ 0.7 default on first launch

    [Header("Music Clips")]
    public AudioClip startMenuMusic;
    public AudioClip levelMusic;
    public AudioClip bossMusic;
    public AudioClip finalCutsceneMusic;

    [Header("Timing")]
    public float preBossFadeOutDuration = 0.3f;
    public float delayBeforeBossMusic = 1f;
    public float bossMusicVolumeIncrease = 0.1f;

    [Header("Runtime")]
    public AudioSource musicAudioSource;

    private float savedVolumeLinear = 0.7f; // linear 0..1 (target for the fade)
    private const float mixerSilentDB = -80f; // dB value considered silence

    void Start()
    {
        // Determine saved or default target volume (linear 0..1)
        bool hasSaved = PlayerPrefs.HasKey("MasterVolume");
        savedVolumeLinear = hasSaved ? PlayerPrefs.GetFloat("MasterVolume") : defaultVolume;

        // Set the slider visually without invoking callbacks
        if (volumeSlider != null)
            volumeSlider.SetValueWithoutNotify(savedVolumeLinear);

        // Start with mixer silent, we'll fade it up if needed
        audioMixer.SetFloat("MasterVolume", mixerSilentDB);

        // Play music for current scene and optionally fade
        string currentScene = SceneManager.GetActiveScene().name;

        switch (currentScene)
        {
            case "StartGame":
                // Start playing the music (it will be silent due to mixer)
                PlayLoopedMusic(musicAudioSource, startMenuMusic, true);
                // Fade the master mixer up to the saved/default linear volume over 2s
                StartCoroutine(FadeMasterVolumeTo(savedVolumeLinear, 2f));
                break;

            case "level1":
                // Immediately apply saved volume (no slow fade), then play
                SetMixerToLinearVolume(savedVolumeLinear);
                PlayLoopedMusic(musicAudioSource, levelMusic, true);
                break;

            case "FinalCutscene":
                SetMixerToLinearVolume(savedVolumeLinear);
                PlayLoopedMusic(musicAudioSource, finalCutsceneMusic, false);
                break;

            default:
                Debug.LogWarning($"AudioManager: No music assigned to scene '{currentScene}'.");
                // Ensure mixer reflects saved volume for other scenes
                SetMixerToLinearVolume(savedVolumeLinear);
                break;
        }
    }

    // Called by the UI slider OnValueChanged(float).  Sets master volume (in dB) and saves the linear value.
    public void SetVolume(float volume)
    {
        // convert 0..1 to decibels and set the mixer param
        float dB = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat("MasterVolume", dB);

        // Save linear value for next launch
        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();
    }

    // Set mixer parameter immediately using a linear 0..1 value (does not call PlayerPrefs).
    private void SetMixerToLinearVolume(float linear)
    {
        float dB = Mathf.Log10(Mathf.Clamp(linear, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat("MasterVolume", dB);
    }

    public void StartBossMusic()
    {
        StartCoroutine(FadeOutThenPlayBossMusic());
    }

    private IEnumerator FadeOutThenPlayBossMusic()
    {
        float volumeBeforeFade = musicAudioSource.volume;
        float t = 0f;

        // Fade out AudioSource volume (not master mixer)
        while (t < preBossFadeOutDuration)
        {
            t += Time.deltaTime;
            musicAudioSource.volume = Mathf.Lerp(volumeBeforeFade, 0f, t / preBossFadeOutDuration);
            yield return null;
        }

        musicAudioSource.Stop();
        musicAudioSource.volume = volumeBeforeFade + bossMusicVolumeIncrease;

        // Wait before starting boss music
        yield return new WaitForSeconds(delayBeforeBossMusic);

        // Start boss music (master mixer remains at whatever master volume is set)
        PlayLoopedMusic(musicAudioSource, bossMusic, true);
    }

    // Fades the MasterVolume mixer parameter from silence up to the target linear volume.
    public IEnumerator FadeMasterVolumeTo(float targetVolume, float duration)
    {
        float startVolume;
        audioMixer.GetFloat("MasterVolume", out startVolume);

        // AudioMixer parameters are usually in decibels, so linear fades don't sound right
        // Convert target volume to decibels if you were storing it as a linear 0–1 value
        float startVolLin = Mathf.Pow(10f, startVolume / 20f);
        float targetVolLin = Mathf.Clamp01(targetVolume);

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float newVolLin = Mathf.Lerp(startVolLin, targetVolLin, timer / duration);
            float newVolDb = Mathf.Log10(Mathf.Max(newVolLin, 0.0001f)) * 20f;
            audioMixer.SetFloat("MasterVolume", newVolDb);
            yield return null;
        }

        // Final snap to ensure exact target
        float finalVolDb = Mathf.Log10(Mathf.Max(targetVolLin, 0.0001f)) * 20f;
        audioMixer.SetFloat("MasterVolume", finalVolDb);
    }

    private void PlayLoopedMusic(AudioSource source, AudioClip musicClip, bool isLoop)
    {
        if (source == null)
        {
            Debug.LogWarning("AudioManager: musicAudioSource is not assigned.");
            return;
        }

        source.clip = musicClip;
        source.loop = isLoop;
        source.Play();
    }
}
