using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private PlayerMovement playerController; // The player to follow
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10); // Camera offset from player
    
    [Header("Follow Settings")]
    [SerializeField] private float smoothSpeed = 5f; // How smoothly the camera follows (higher = faster)

    private float yPositionWhenJumped;
    private bool wasGrounded;

    private void Start()
    {
        if (playerController == null)
        {
            Debug.LogError("CameraFollow: PlayerController not set in the inspector.");
            return;
        }

        Vector3 desiredPosition = GetDesiredPosition();
        transform.position = desiredPosition;
        yPositionWhenJumped = transform.position.y;
        wasGrounded = playerController.isGrounded;
    }

    private void LateUpdate()
    {
        if (playerController == null) return;

        Vector3 desiredPosition = GetDesiredPosition();
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        
        transform.position = smoothedPosition;
    }

    private Vector3 GetDesiredPosition()
    {
        Vector3 desiredPosition = transform.position;
        desiredPosition.x = playerController.transform.position.x + offset.x;

        // Check if the player has just jumped
        if (wasGrounded && !playerController.isGrounded)
        {
            // Player has just jumped, so store the current y-position
            yPositionWhenJumped = transform.position.y;
        }

        if (playerController.isGrounded)
        {
            // Follow player's Y position when grounded
            desiredPosition.y = playerController.transform.position.y + offset.y;
        }
        else
        {
            // When in the air, maintain the Y position from when the jump started,
            // but still follow the player down if they are falling.
            desiredPosition.y = Mathf.Min(yPositionWhenJumped, playerController.transform.position.y + offset.y);
        }
        
        desiredPosition.z = offset.z; // Always maintain the camera's Z position
        
        // Update wasGrounded for the next frame
        wasGrounded = playerController.isGrounded;

        return desiredPosition;
    }

    // Public method to set the target at runtime
    public void SetTarget(PlayerMovement newPlayerController)
    {
        playerController = newPlayerController;
    }
}
