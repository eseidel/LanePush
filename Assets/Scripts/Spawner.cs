using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public int secondsBetweenSpawns;
    public GameObject meleePrefab;
    public GameObject rangedPrefab;
    public Transform spawnPoint;
    public Waypoints waypoints;

    float timeUntilNextSpawn = 0;

    public Team team;

    bool nextIsMelee = false;

    void Spawn(GameObject prefab)
    {
        var obj = Object.Instantiate(prefab, spawnPoint.transform.position, Quaternion.identity);
        var mob = obj.GetComponent<MOB>();
        mob.isMinion = true;
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
            if (nextIsMelee)
            {
                Spawn(meleePrefab);
            } else
            {
                Spawn(rangedPrefab);
            }
            nextIsMelee = !nextIsMelee;
        }
    }
}
