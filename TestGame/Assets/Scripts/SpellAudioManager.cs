using UnityEngine;
/*
public class SpellAudioManager : MonoBehaviour
{
    public LoopSpellDetector loopSpellDetector; // Assign in Inspector
    public AudioSource audioSource;      // Assign in Inspector
    public AudioClip clipIntro;          // First (non-looped)
    public AudioClip clipLoop;           // Second (looped)
    public AudioClip clipEndA;           // Third (if useClipB == false)
    public AudioClip clipEndB;           // Third (if useClipB == true)

    public bool useClipB = false;        // Set this to determine the ending
    private bool hasStarted = false;
    private bool waitingToLoop = false;
    private bool loopPlaying = false;

    void Update()
    {
        // Start the sequence on left click
        if (Input.GetMouseButtonDown(0) && loopSpellDetector.isDrawing && !hasStarted)
        {
            audioSource.loop = false;
            audioSource.clip = clipIntro;
            audioSource.Play();
            hasStarted = true;
            waitingToLoop = true;
        }

        // Transition to loop clip after intro finishes
        if (waitingToLoop && !audioSource.isPlaying)
        {
            audioSource.clip = clipLoop;
            audioSource.loop = true;
            audioSource.Play();
            waitingToLoop = false;
            loopPlaying = true;
        }

        if (Input.GetMouseButtonUp(0) && loopSpellDetector.loopReady && loopPlaying)
        {
            useClipB = true;
            // Stop the loop and play the ending clip
            PlayEndingClip();
        }
    }

    // Call this method when you're ready to play the final sound
    public void PlayEndingClip()
    {
        if (!loopPlaying) return;

        audioSource.loop = false;
        audioSource.Stop(); // Stop the loop immediately

        audioSource.clip = useClipB ? clipEndB : clipEndA;
        audioSource.Play();
        loopPlaying = false;
    }
}
*/
