using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    int secondsBetweenSpawns = 3;
    int secondsBetweenWaves = 12;

    public GameObject meleePrefab;
    public GameObject rangedPrefab;
    public Transform spawnPoint;
    public Waypoints waypoints;

    float timeUntilNextSpawn = 0;

    public Team team;

    int spawnCounter = 0;

    void Spawn(GameObject prefab)
    {
        var obj = Object.Instantiate(prefab, spawnPoint.transform.position, Quaternion.identity);
        var mob = obj.GetComponent<MOB>();
        mob.team = team;
        mob.waypoints = waypoints;
    }

    // Update is called once per frame
    void Update()
    {
        timeUntilNextSpawn -= Time.deltaTime;
        if (timeUntilNextSpawn < 0)
        {
            timeUntilNextSpawn = secondsBetweenSpawns;
            if ((spawnCounter % 6) < 3)
            {
                Spawn(meleePrefab);
            }
            //else
            //{
            //    Spawn(rangedPrefab);
            //}
            spawnCounter++;
            if (spawnCounter % 6 == 0)
            {
                timeUntilNextSpawn = secondsBetweenWaves;
            }
        }
    }
}
