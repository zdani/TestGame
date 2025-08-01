using UnityEngine;

public class IceShieldAbility : MonoBehaviour
{
    public GameObject iceShieldPrefab;
    public Transform shieldSpawnPoint;
    public float shieldDuration = 5f;
    

    public void Cast()
    {
        GameObject iceShield = Instantiate(iceShieldPrefab, shieldSpawnPoint.position, Quaternion.identity);
        iceShield.transform.SetParent(transform); // Attach to player
        iceShield.GetComponent<IceShield>().Initialize(shieldDuration);
    }
}
