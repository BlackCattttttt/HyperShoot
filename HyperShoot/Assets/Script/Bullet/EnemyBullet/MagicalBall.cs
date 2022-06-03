using HyperShoot.Combat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperShoot.Bullet
{
    public class MagicalBall : BaseEnemyBullet
    {
        [SerializeField] protected GameObject hitParticle;
        protected override bool CanDamageOnEnter { get; } = true;
        protected override bool CanDamageOnStay { get; } = false;


        protected override void DamageTarget(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                var damageable = other.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(new DamageData
                    {
                        Type = DamageType.Bullet,
                        Damage = damage,
                        ImpactObject = gameObject
                    });
                    if (hitParticle != null)
                        SimplePool.Spawn(hitParticle.gameObject, EffectContainer.Instance.transform, transform.position, Quaternion.identity);
                }
                DeSpawn();
            }
        }
    }
}
