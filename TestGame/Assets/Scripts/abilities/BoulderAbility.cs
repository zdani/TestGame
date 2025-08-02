using System;
using UnityEngine;

public class BoulderAbility : MonoBehaviour
{
    public GameObject boulderPrefab;
    private Camera mainCamera;

    void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found. Please ensure there is a camera tagged as 'MainCamera'.");
        }
    }

    public void Cast()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        Instantiate(boulderPrefab, mouseWorldPos, Quaternion.identity);
    }
}
