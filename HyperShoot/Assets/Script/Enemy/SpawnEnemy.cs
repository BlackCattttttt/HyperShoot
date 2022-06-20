using HyperShoot.Enemy;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEnemy : MonoBehaviour
{
    [SerializeField] private BaseEnemy[] enemies;
    [SerializeField] private ParticleSystem spawnParticle;
    [SerializeField] private float spawnTime = 5f;

    public float range = 30f;

    private bool canSpawn;

    public int maxEnemy = 4;
    public int currentEnemy;

    void Start()
    {
        canSpawn = true;
        currentEnemy = 0;
    }

    public void SpawnEnemies ()
    {
        if (canSpawn && currentEnemy <= maxEnemy)
        {
            if (spawnParticle != null)
            {
                Instantiate(spawnParticle, transform.position, Quaternion.identity);
            }
            currentEnemy++;
            Vector3 circlePos = Random.insideUnitSphere;
            var spawnPos = circlePos * Random.Range(5f, 15f);
            SimplePool.Spawn(enemies[Random.Range(0, enemies.Length)], transform.position + new Vector3(spawnPos.x,0,spawnPos.z), Quaternion.identity);
            StartCoroutine(SpawnTime());
        } 
    }

    IEnumerator SpawnTime()
    {
        canSpawn = false;
        yield return new WaitForSeconds(spawnTime);
        canSpawn = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
