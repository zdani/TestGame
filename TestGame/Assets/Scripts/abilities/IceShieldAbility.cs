using UnityEngine;

public class IceShieldAbility : MonoBehaviour
{
    public GameObject iceShieldPrefab;
    public Transform shieldSpawnPoint;
    public float shieldDuration = 1f;

    [HideInInspector]
    public bool isShieldActive = false;
    private IceShield _iceShield;


    public void Cast()
    {
        if (isShieldActive)
        {
            Debug.Log("Ice Shield is already active, cannot cast again.");
            return; // Prevent multiple shields
        }
        else Debug.Log("Casting Ice Shield");

        GameObject iceShield = Instantiate(iceShieldPrefab, shieldSpawnPoint.position, Quaternion.identity);
        iceShield.transform.SetParent(transform); // Attach to player
        _iceShield = iceShield.GetComponent<IceShield>();
        _iceShield.Initialize(shieldDuration);
        isShieldActive = true;
    }

    public void BreakShield()
    {
        _iceShield.BreakShield();
    }
}
