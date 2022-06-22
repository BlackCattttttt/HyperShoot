using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : Singleton<SpawnManager>
{
    public SpawnEnemy[] spawners;
  //  public int numberOfSpawns;

    private List<SpawnEnemy> nearSpawns;
    private GameObject player;

    public int maxEnemy = 4;
    public int currentEnemy;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        nearSpawns = new List<SpawnEnemy>();
        currentEnemy = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentEnemy <= maxEnemy)
        {
            findNearSpawns();
            for (int i = 0; i < nearSpawns.Count; i++)
            {
                nearSpawns[i].SpawnEnemies();
            }
        }
    }

    void findNearSpawns ()
    {
        nearSpawns.Clear();
        for (int i = 0; i < spawners.Length; i++)
        {
            float distance = Vector3.Distance(spawners[i].transform.position, player.transform.position);
            if (distance < spawners[i].range)
            {
                nearSpawns.Add(spawners[i]);
            }
        }
    }
}
