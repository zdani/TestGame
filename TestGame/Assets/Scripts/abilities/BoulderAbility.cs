using UnityEngine;

public class BoulderAbility : MonoBehaviour
{
    public GameObject boulderPrefab;
    public Transform boulderSpawnPoint;

    public void Cast()
    {
        Vector2 spawnPosition = boulderSpawnPoint.position;
        GameObject boulder = Instantiate(boulderPrefab, spawnPosition, Quaternion.identity);
        //boulder.GetComponent<Boulder>().Initialize();
    }
}
