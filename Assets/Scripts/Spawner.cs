using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public int secondsBetweenSpawns;
    public GameObject minionPrefab;
    public GameObject spawnPoint;

    float timeUntilNextSpawn = 0;

    public Team team;


    // Update is called once per frame
    void Update()
    {
        timeUntilNextSpawn -= Time.deltaTime;
        if (timeUntilNextSpawn < 0)
        {
            timeUntilNextSpawn = secondsBetweenSpawns;
            var obj = Object.Instantiate(minionPrefab, spawnPoint.transform.position, Quaternion.identity);
            var mob = obj.GetComponent<MOB>();
            mob.isMinion = true;
            mob.team = team;
        }
    }
}
