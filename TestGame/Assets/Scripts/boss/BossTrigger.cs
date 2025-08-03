using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    public BossWiz bossWizScript;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            bossWizScript.StartBehaviorCycle();
            Destroy(gameObject); // Destroy the trigger so it only happens once
        }
    }
}
