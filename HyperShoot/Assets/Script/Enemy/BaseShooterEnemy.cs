using HyperShoot.Bullet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperShoot.Enemy
{
    public class BaseShooterEnemy : BaseEnemy
    {
        [SerializeField] protected BaseEnemyBullet prefabBullet;
        [SerializeField] protected Transform shootPoint;
        [SerializeField] protected int preloadCount;
        [SerializeField] protected float shootForce = 400f;

        protected override void Awake()
        {
            base.Awake();
            SimplePool.Preload(prefabBullet.gameObject, preloadCount);
        }
        public override void AttackPlayer()
        {
            if (isNav)
            {
                agent.SetDestination(transform.position);
            }
            else
            {
                aIPath.destination = transform.position;
            }
            transform.LookAt(player.transform.position);
            if (!canAttack)
            {
                anim.SetBool("run", false);
                anim.SetBool("walk", false);
                canAttack = true;
                _delayAttack = delayAttack;
                Attack();
            }
        }
        public void Shoot()
        {
            AudioManager.Instance.Play("MagicalAttack");
            var bullet = SpawnBullets(prefabBullet, shootPoint.position);
            var bulletTransform = bullet.transform;
            bulletTransform.SetParent(BulletContainer.Instance.transform, true);
            Vector3 temp = player.transform.position + new Vector3(0.0f, 1.5f, 0.0f);
            Vector3 dir = temp - shootPoint.transform.position;
            bullet.Shoot(dir.normalized, shootForce, ForceMode.VelocityChange);
        }
        protected BaseEnemyBullet SpawnBullets(BaseEnemyBullet prefab, Vector3 position)
        {
            var bullet = SimplePool.Spawn(prefab, gameObject.tag, position, Quaternion.identity);
            bullet.SetDamage(damage);
            return bullet;
        }
    }
}
