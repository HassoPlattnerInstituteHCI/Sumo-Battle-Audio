using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPowerUpManager : MonoBehaviour
{
    private float spawnRange = 9f;
    public GameObject powerupPrefab;


    void Start()
    {
        SpawnPowerup();
    }

    void Update()
    {
        //SpawnPowerup();
    }

    private Vector3 GenerateSpawnPosition()
    {
        float randomPosX = Random.Range(-spawnRange, spawnRange);
        float randomPosZ = Random.Range(-spawnRange, spawnRange);
        Vector3 randomPos = new Vector3(randomPosX, 0, randomPosZ);
        return randomPos;
    }

    void SpawnPowerup()
    {
        Instantiate(powerupPrefab, GenerateSpawnPosition(), powerupPrefab.transform.rotation);
    }
}
