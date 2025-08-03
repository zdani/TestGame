using UnityEngine;

public class FootstepAudio : MonoBehaviour
{
    public AudioClip[] footstepClips; // Assign 3 clips in Inspector
    public float stepInterval = 0.5f; // Time between steps
    public AudioSource audioSource;
    public PlayerMovement playerMovementScript; // Your script with isWalking

    private float stepTimer;

    void Update()
    {
        if (playerMovementScript.isMoving && playerMovementScript.isGrounded)
        
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                PlayFootstep();
                stepTimer = stepInterval;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    void PlayFootstep()
    {
        if (footstepClips.Length > 0)
        {
            int index = Random.Range(0, footstepClips.Length);
            audioSource.PlayOneShot(footstepClips[index]);
        }
    }
}
