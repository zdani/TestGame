using UnityEngine;

public class IceShieldAbility : MonoBehaviour
{
    public GameObject iceShieldPrefab;
    public float shieldDuration = 5f;
    public Transform shieldSpawnPoint;

    public void Cast()
    {
        GameObject iceShield = Instantiate(iceShieldPrefab, shieldSpawnPoint.position, Quaternion.identity);
        iceShield.transform.SetParent(transform); // Attach to player
        iceShield.GetComponent<IceShield>().Initialize(shieldDuration);
    }
}
