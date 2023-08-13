using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    // Float
    [SerializeField] float SpawnDelay;
    [SerializeField] float SpawnDistance;

    // GameObject
    [SerializeField] GameObject Drone;
    [SerializeField] GameObject Charger;
    [SerializeField] GameObject Destroyer;
    [SerializeField] GameObject Behemoth;
    GameObject Player;

    // Other
    [SerializeField] int MaxEnemies;
    public int TotalEnemiesToSpawn;
    public Dictionary<EnemyType, int> EnemiesLeft;
    [SerializeField] public List<Wave> Waves;

    void Start()
    {
        Player = GameObject.Find("Player");

        EnemiesLeft = new Dictionary<EnemyType, int>();
        List<EnemyType> enemyTypes = Enum.GetValues(typeof(EnemyType)).Cast<EnemyType>().ToList();

        foreach (EnemyType type in enemyTypes)
            EnemiesLeft.Add(type, 0);
    }

    public void StartWave(int wave)
    {
        foreach (EnemyToSpawn enemyToSpawn in Waves[wave - 1].EnemiesToSpawn)
        {
            StartCoroutine(SpawnEnemies(enemyToSpawn.Type, enemyToSpawn.SpawnAmount, enemyToSpawn.BossEnemy));
        }
    }

    IEnumerator Spawn(GameObject enemyToSpawn, bool isBoss = false)
    {
        GameObject newEnemy = Instantiate(enemyToSpawn);

        //// Creates a vector with a length of spawndistance pointing in a random direction
        //float v = Random.Range(0, 2 * Mathf.PI);
        //float x = Mathf.Cos(v);
        //float y = Mathf.Sin(v);

        //Vector3 spawnPos = new(x, y);
        //spawnPos *= SpawnDistance;

        //// Sets the newEnemy position to that vector away from the player then makes it look in a random direction
        //newEnemy.transform.position = Player.transform.position - spawnPos;
        //newEnemy.transform.eulerAngles = new(0, 0, Random.Range(0f, 2 * Mathf.PI));

        Rect cameraBounds = GameObject.Find("Main Camera").GetComponent<CameraController>().GetCameraBounds();

        Rect spawnArea = new(cameraBounds.position.x - 20f, cameraBounds.position.y - 20f, cameraBounds.width + 40f, cameraBounds.height + 40f);

        Vector2 spawnPos = Vector2.zero;

        // Generates a random position outside the camera bounds
        switch (Random.Range(0, 4))
        {
            // North
            case 0:
                spawnPos = new(Random.Range(spawnArea.xMin, spawnArea.xMax), Random.Range(cameraBounds.yMax, spawnArea.yMax));
                break;
            // South
            case 1:
                spawnPos = new(Random.Range(spawnArea.xMin, spawnArea.xMax), Random.Range(spawnArea.yMin, cameraBounds.yMin));
                break;
            // East
            case 2:
                spawnPos = new(Random.Range(cameraBounds.xMax, spawnArea.xMax), Random.Range(spawnArea.yMin, spawnArea.yMax));
                break;
            // West
            case 3:
                spawnPos = new(Random.Range(spawnArea.xMin, cameraBounds.xMin), Random.Range(spawnArea.yMin, spawnArea.yMax));
                break;
        }

        newEnemy.transform.position = spawnPos;
        newEnemy.transform.eulerAngles = new(0, 0, Random.Range(0f, 2 * Mathf.PI));

        if (isBoss)
            StartCoroutine(GetComponent<UIManager>().ActivateBossHealthBar(newEnemy));

        yield return new WaitForSeconds(SpawnDelay);
    }

    IEnumerator SpawnEnemies(EnemyType enemy, int enemyAmount, bool isBoss = false)
    {
        //GameObject.Find("Player").gameObject.GetComponent<PlayerController>().EnableDisableHeat(true);

        GameObject enemyToSpawn = null;

        switch (enemy)
        {
            case EnemyType.Drone:
                enemyToSpawn = Drone;
                break;
            case EnemyType.Charger:
                enemyToSpawn = Charger;
                break;
            case EnemyType.Destroyer:
                enemyToSpawn = Destroyer;
                break;
            case EnemyType.Behemoth:
                enemyToSpawn = Behemoth;
                break;
        }

        TotalEnemiesToSpawn += enemyAmount;

        yield return new WaitForSeconds(3);

        while (enemyAmount > 0 && Player != null)
        {
            if (GameObject.FindGameObjectsWithTag("Enemy").Length < MaxEnemies)
            {
                enemyAmount--;
                TotalEnemiesToSpawn--;
                EnemiesLeft[enemy]++;

                yield return StartCoroutine(Spawn(enemyToSpawn, isBoss));
            }

            else
                yield return new WaitForSeconds(0.5f);
        }

        while (EnemiesLeft[enemy] > 0 && Player != null)
        {
            if (GameObject.FindGameObjectsWithTag("Enemy").Length < EnemiesLeft[enemy])
                yield return StartCoroutine(Spawn(enemyToSpawn));

            else
                yield return new WaitForSeconds(0.5f);
        }

        //GameObject.Find("Player").gameObject.GetComponent<PlayerController>().EnableDisableHeat(false);
    }

    public bool EnemiesStillLeft()
    {
        return EnemiesLeft.Any(p => p.Value > 0);
    }

    public bool LastWave(int currentWave)
    {
        return Waves.Count <= currentWave;
    }
}

[System.Serializable]
public class Wave
{
    public List<EnemyToSpawn> EnemiesToSpawn;
}

[System.Serializable]
public class EnemyToSpawn
{
    [SerializeField] public EnemyType Type;
    public int SpawnAmount;
    public bool BossEnemy;
}

public enum EnemyType
{
    Drone,
    Charger,
    Destroyer,
    Behemoth
}
