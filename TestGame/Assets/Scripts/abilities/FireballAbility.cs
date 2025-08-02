using UnityEngine;

public class FireballAbility : MonoBehaviour
{
    public GameObject fireballPrefab;
    public Transform fireballSpawnPoint;
    public AudioClip fireballAudio;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Cast()
    {
        Vector2 shootDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        GameObject fireball = Instantiate(fireballPrefab, fireballSpawnPoint.position, Quaternion.identity);
        fireball.GetComponent<FireballProjectile>().Initialize(shootDirection);

        audioSource.clip = fireballAudio;
        audioSource.pitch = Random.Range(0.95f, 1.15f);
        //Debug.Log("Fireball sound pitch set to: " + audioSource.pitch);
        audioSource.Play();
    }
}
