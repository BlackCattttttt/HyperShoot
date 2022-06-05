using HyperShoot.Combat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperShoot.Enemy
{
    public class BaseMeleeEnemy : BaseEnemy
    {
        [SerializeField] private Transform checkPoint;
        [SerializeField] private float radius;
        public void DealDamage()
        {
            if (!isDead && canAttack)
            {
                Collider[] target = Physics.OverlapSphere(checkPoint.position, radius);
                for (int i = 0; i < target.Length; i++)
                {
                    if (target[i].CompareTag("Player"))
                    {
                        var damageble = target[i].GetComponentInParent<IDamageable>();
                        if (damageble != null)
                        {
                            damageble.TakeDamage(new DamageData
                            {
                                Type = DamageType.Unknown,
                                Damage = damage,
                                ImpactObject = gameObject
                            });
                        }
                    }
                }
            }
        }
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(checkPoint.position, radius);
        }
#endif
    }
}
