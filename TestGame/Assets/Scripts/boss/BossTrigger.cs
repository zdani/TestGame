using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    public BossWiz bossWizScript;
    public AudioManager audioManager;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            bossWizScript.StartBehaviorCycle();
            audioManager.StartBossMusic();
            Debug.Log("Boss triggered!");

            Destroy(gameObject); // Destroy the trigger so it only happens once
        }
    }
}
