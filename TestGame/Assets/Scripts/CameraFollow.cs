using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target; // The player to follow
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10); // Camera offset from player
    
    [Header("Follow Settings")]
    [SerializeField] private float smoothSpeed = 5f; // How smoothly the camera follows (higher = faster)

    private void Start()
    {
        Vector3 desiredPosition = GetDesiredPosition();
        transform.position = desiredPosition;
    }

    private void LateUpdate()
    {
        Vector3 desiredPosition = GetDesiredPosition();
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        
        transform.position = smoothedPosition;
    }

    private Vector3 GetDesiredPosition()
    {
        Vector3 desiredPosition = transform.position;
        desiredPosition.x = target.position.x + offset.x;
        desiredPosition.y = target.position.y + offset.y;      
        desiredPosition.z = offset.z; // Always maintain the camera's Z position
        
        return desiredPosition;
    }

    // Public method to set the target at runtime
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
} 