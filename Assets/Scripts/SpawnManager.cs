using UnityEngine;
using System.Collections;

public class SpawnManager : MonoBehaviour
{
    public GameObject enemyPrefab;
    private float spawnRange = 9f;
    public GameObject powerupPrefab;
    public int waveNumber = 0;  
    private int enemyCount;

    //public AudioClip[] enemyClips;
    public string[] enemyNames;

    public bool gameStarted = false;

    void Start()
    {
        StartCoroutine(StartGame());
    }

    IEnumerator StartGame() {
        Level room = GameObject.Find("Panto").GetComponent<Level>();
        yield return room.playIntroduction();
        GameObject.FindObjectOfType<PlayerController>().activatePlayer();
        gameStarted = true;
    }

    void Update()
    {
        if (!gameStarted) return;
        enemyCount = FindObjectsOfType<Enemy>().Length;
        if (enemyCount == 0)
        {
            waveNumber++;
            SpawnEnemyWave(waveNumber);
            SpawnPowerup();
        }
    }

    /// challenge: spawn specified numberOfEnemies using Instantiate(...)
    void SpawnEnemyWave(int numberOfEnemies)
    {
        GameObject enemy = Instantiate(
            enemyPrefab,
            GenerateSpawnPosition(),
            enemyPrefab.transform.rotation
        );
        enemy.GetComponent<Enemy>().enemyName = enemyNames[0];
        StartCoroutine(GameObject.Find("Panto").GetComponent<LowerHandle>().SwitchTo(enemy, 0.2f));

        for (int i = 1; i < numberOfEnemies; i++)
        {
            enemy = Instantiate(
                enemyPrefab,
                GenerateSpawnPosition(),
                enemyPrefab.transform.rotation
            );
            enemy.GetComponent<Enemy>().enemyName = enemyNames[i % enemyNames.Length];
        }
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
