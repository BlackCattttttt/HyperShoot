using HyperShoot.Enemy;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperShoot.Manager
{
    public class IsolatedSpace : MonoBehaviour
    {
        [SerializeField] private BaseEnemy[] enemies;
        [SerializeField] private List<Transform> spawnPoints;
        [SerializeField] private ParticleSystem spawnParticle;
        [SerializeField] private float spawnTime = 5f;

        private float delay = -1f;
        private List<BaseEnemy> listEnemy = new List<BaseEnemy>();

        private void Update()
        {
            delay -= Time.deltaTime;
            if (delay < 0)
            {
                for (int i = 0; i < spawnPoints.Count; i++)
                {
                    SpawnEnemies(spawnPoints[i]);
                }
                delay = spawnTime;
            }
        }
        public void SpawnEnemies(Transform spawnPoint)
        {
            if (spawnParticle != null)
            {
                Instantiate(spawnParticle, spawnPoint.position, Quaternion.identity);
            }

            Vector3 circlePos = Random.insideUnitSphere;
            var spawnPos = circlePos * Random.Range(5f, 15f);
            BaseEnemy enemy = SimplePool.Spawn(enemies[Random.Range(0, enemies.Length)], transform, spawnPoint.position + new Vector3(spawnPos.x, 0, spawnPos.z), Quaternion.identity);
            listEnemy.Add(enemy);
        }

        private void OnDisable()
        {
            for (int i = 0; i < listEnemy.Count; i++)
            {
                if (listEnemy[i] != null)
                    SimplePool.Despawn(listEnemy[i].gameObject);
            }
        }
    }
}

