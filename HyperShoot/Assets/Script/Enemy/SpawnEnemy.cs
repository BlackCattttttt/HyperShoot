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

    void Start()
    {
        canSpawn = true;
    }

    public void SpawnEnemies ()
    {
        if (canSpawn)
        {
            if (spawnParticle != null)
            {
                Instantiate(spawnParticle, transform.position, Quaternion.identity);
            }
            SpawnManager.Instance.currentEnemy++;
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
