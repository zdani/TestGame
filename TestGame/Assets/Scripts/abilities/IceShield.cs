using UnityEngine;

public class IceShield : MonoBehaviour
{
    public GameObject iceShieldPrefab;

    public void Initialize(float duration = 5f)
    {
        Destroy(gameObject, duration); // Destroy after duration, replace later with a method that includes animation
    }
}
