using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebrisSpawner : MonoBehaviour
{
    // Float
    [SerializeField][Range(0f, 1f)] float RotationChance;
    [SerializeField][Range(0f, 1f)] float DriftChance;

    // GameObject
    [SerializeField] GameObject SpawnParent;
    [SerializeField] List<GameObject> DebrisPrefabs;
    [SerializeField] GameObject Player;
    [SerializeField] int MinimumDebrisAmount;
    [SerializeField] string DebrisTag = "Debris";

    private void Update()
    {
        GameObject[] debris = GameObject.FindGameObjectsWithTag(DebrisTag);

        if (debris.Length <= MinimumDebrisAmount)
            SpawnInArea(debris.Length - MinimumDebrisAmount, GetComponent<GameManager>().PlayArea);
    }

    /// <summary>
    /// Spawns debris inside specified bounds
    /// </summary>
    /// <param name="debrisToSpawn">Amount of debris to spawn</param>
    /// <param name="spawnArea">Rectangle coresponding to area in world to spawn in</param>
    public void SpawnInArea(int debrisToSpawn, Rect spawnArea)
    {
        for (int x = 0; x < debrisToSpawn; x++)
        {
            int chosenPrefab = Random.Range(0, DebrisPrefabs.Count);
            GameObject newDebris = Instantiate(DebrisPrefabs[chosenPrefab]);

            Vector2 spawnPos = new Vector2(Random.Range(spawnArea.xMin, spawnArea.xMax), Random.Range(spawnArea.yMin, spawnArea.yMax));

            while (Vector2.Distance(Player.transform.position,spawnPos) < 10)
                spawnPos = new Vector2(Random.Range(spawnArea.xMin, spawnArea.xMax), Random.Range(spawnArea.yMin, spawnArea.yMax));

            newDebris.transform.position = spawnPos;

            if (DebrisPrefabs[chosenPrefab].name == "Asteroid")
            {
                float Scale = Random.Range(0.5f, 3f);
                newDebris.transform.localScale = new Vector3(Scale, Scale, Scale);
            }

            if (Random.Range(0f, 1f) <= DriftChance)
                newDebris.GetComponent<Rigidbody2D>().AddForce(Random.insideUnitCircle.normalized * Random.Range(0.5f, 3f), ForceMode2D.Impulse);

            if (Random.Range(0f, 1f) <= RotationChance)
                newDebris.GetComponent<Rigidbody2D>().AddTorque(Random.Range(-0.75f, 0.75f), ForceMode2D.Impulse);

            newDebris.transform.SetParent(SpawnParent.transform);
        }
    }

    public void ClearDebris(GameObject debris)
    {
        Destroy(debris);
    }

    public void ClearAllDebris()
    {
        foreach (GameObject debris in GameObject.FindGameObjectsWithTag("Debris"))
            Destroy(debris);
    }
}
