using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class PowerUpManager : MonoBehaviour
{
    [SerializeField] List<PwrUp> PowerUps;
    [SerializeField] int MaxPowerUps;
    [SerializeField] float SpawnDelayMin;
    [SerializeField] float SpawnDelayMax;
    [SerializeField] float BorderMargin;
    [SerializeField] float SpawnDistMin = 30f;
    [SerializeField] float SpawnDistMax = 60f;

    List<GameObject> SpawnedPowerUps = new();
    GameManager GM;
    PlayerController Player;
    [SerializeField] Transform SpawnParent;

    // Start is called before the first frame update
    void Start()
    {
        GM = GameObject.Find(GameObjectNames.Managers).GetComponent<GameManager>();
        Player = GameObject.Find(GameObjectNames.Player).GetComponent<PlayerController>();
    }

    private void Update()
    {
        //SpawnedPowerUps.RemoveAll(p => p == null);
    }

    public void SpawnPowerUp()
    {
        Vector2 player = Player.transform.position;
        Rect playArea = GM.PlayArea;
        playArea.height -= BorderMargin;
        playArea.width -= BorderMargin;
        playArea.x -= BorderMargin;
        playArea.y -= BorderMargin;

        // Generates spawn position that is some distance from the player
        float randAngle = Random.Range(0f,360f);

        Vector2 randDir = new(Mathf.Cos(randAngle), Mathf.Sin(randAngle));

        float randDist = Random.Range(SpawnDistMin, SpawnDistMax);

        Vector2 spawnPos = randDir * randDist + player;

        // If it is not inside the play area - bounds, invert spawn position
        if (!playArea.Contains(spawnPos))
        {
            if (playArea.Contains(player + new Vector2(randDir.x, randDir.y * (-1)) * randDist))
                spawnPos = new Vector2(randDir.x, randDir.y * (-1)) * randDist + player;

            else if (playArea.Contains(player + new Vector2(randDir.x * (-1), randDir.y) * randDist))
                spawnPos = new Vector2(randDir.x * (-1), randDir.y) * randDist + player;

            else
                spawnPos = new Vector2(randDir.x * (-1), randDir.y * (-1)) * randDist + player;
        }

        // Picks a random power up and instantiates it
        GameObject toSpawn = null;

        float weightSum = 0;

        foreach (PwrUp pwrUp in PowerUps)
            weightSum += pwrUp.Weight;

        foreach (PwrUp pwrUp in PowerUps)
        {
            if (Random.Range(0f, weightSum) <= pwrUp.Weight)
            {
                toSpawn = pwrUp.PowerUp;
                break;
            }

            weightSum -= pwrUp.Weight;
        }

        SpawnedPowerUps.Add(Instantiate(toSpawn, spawnPos, new(), SpawnParent));

        Debug.Log($"Spawning {toSpawn.name} at {spawnPos}");
    }

    public void SpawnPowerUp(Vector2 spawnPos)
    {
        GameObject toSpawn = null;

        float weightSum = 0;

        foreach (PwrUp pwrUp in PowerUps)
            weightSum += pwrUp.Weight;

        foreach (PwrUp pwrUp in PowerUps)
        {
            if (Random.Range(0f, weightSum) <= pwrUp.Weight)
            {
                toSpawn = pwrUp.PowerUp;
                break;
            }

            weightSum -= pwrUp.Weight;
        }

        SpawnedPowerUps.Add(Instantiate(toSpawn, spawnPos, new()));
    }

    public void SpawnPowerUp(Vector2 spawnPos, GameObject toSpawn)
    {
        SpawnedPowerUps.Add(Instantiate(toSpawn, spawnPos, new()));
    }

    IEnumerator SpawnDelay()
    {
        SpawnedPowerUps.RemoveAll(p => p == null);
        while (SpawnedPowerUps.Count >= MaxPowerUps) yield return null;
        
        if (Player ? Player.IsDead : true)
            yield break;

        yield return new WaitForSeconds(Random.Range(SpawnDelayMin,SpawnDelayMax));

        if (Player ? Player.IsDead : true)
            yield break;

        SpawnPowerUp();
        StartCoroutine(SpawnDelay());
    }

    public void StartSpawning()
    {
        StartCoroutine(SpawnDelay());
    }

    public void StopSpawning()
    {
        StopAllCoroutines();
    }
}

[Serializable]
public class PwrUp
{
    public GameObject PowerUp;
    public float Weight;
}
