using HyperShoot.Combat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace HyperShoot.Enemy
{
    public class BomberBugEnemy : BaseEnemy
    {
        [SerializeField] private float Radius = 15.0f;                    // any objects within radius will be affected by the explosion
        [SerializeField] private float Force = 1000.0f;                   // amount of positional force to apply to affected objects
        [SerializeField] private ParticleSystem explodeParticle;
        [SerializeField] private AudioClip explosionSound;

        // physics
        protected Ray m_Ray;
        protected RaycastHit m_RaycastHit;
        protected Collider m_TargetCollider = null;
        protected Transform m_TargetTransform = null;
        protected Rigidbody m_TargetRigidbody = null;
        protected float m_DistanceModifier = 0.0f;
        protected float DistanceModifier
        {
            get
            {
                if (m_DistanceModifier == 0.0f)
                    m_DistanceModifier = (1 - Vector3.Distance(transform.position, m_TargetTransform.position) / Radius);
                return m_DistanceModifier;
            }
        }
        protected override void Start()
        {
            enemyDamageHandler.HealthObservable
              .Where(hp => hp <= 0f)
              .Subscribe(_ => {
                  if (!canAttack)
                  {
                      canAttack = true;
                      anim.SetTrigger("attack");
                  }
              } )
              .AddTo(_healthDisposables);
        }
        protected override void FixedUpdate()
        {
            
        }
        public override void Patrolling()
        {
            if (canAttack)
                return;
            if (!walkPointSet) SearchWalkPoint();

            if (walkPointSet)
            {
                if (isNav)
                {
                    agent.SetDestination(walkPoint);
                }
                else
                {
                    aIPath.destination = walkPoint;
                }
                anim.SetBool("walk", true);
                anim.SetBool("fly", false);
            }

            Vector3 distanceToWalkPoint = transform.position - walkPoint;

            if (distanceToWalkPoint.magnitude < 1f)
            {
                walkPointSet = false;
            }
        }
        public override void ChasePlayer()
        {
            if (canAttack)
                return;
            if (isNav)
            {
                agent.SetDestination(player.transform.position);
            }
            else
            {
                aIPath.destination = player.transform.position;
            }
            transform.LookAt(player.transform.position);
            anim.SetBool("fly", true);
            anim.SetBool("walk", false);
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
            anim.SetBool("fly", false);
            anim.SetBool("walk", false);
            if (!canAttack)
            {
                canAttack = true;
                anim.SetTrigger("attack");
            }
        }
        public void Explode()
        {
            if (explodeParticle != null)
            {
                SimplePool.Spawn(explodeParticle.gameObject, EffectContainer.Instance.transform, transform.position, Quaternion.identity);
            }
            if (m_Audio != null)
            {
                m_Audio.pitch = Time.timeScale;
                m_Audio.PlayOneShot(explosionSound);
            }
            Collider[] colliders = Physics.OverlapSphere(transform.position, Radius, fp_Layer.Mask.IgnoreWalkThru);
            foreach (Collider hit in colliders)
            {
                m_DistanceModifier = 0.0f;

                if ((hit != null) && (hit.CompareTag("Player")))
                {
                    m_TargetCollider = hit;
                    m_TargetTransform = hit.transform;

                    // --- abort if we have no line of sight to target ---
                    if (TargetInCover())
                        continue;

                    AddForce();
                    
                    TryDamage();
                }
            }

            Die();
        }
        public override void Die()
        {
            if (!isDead)
            {
                isDead = true;
                Destroy(gameObject, 2f);
            }
        }
        protected bool TargetInCover()
        {
            m_Ray.origin = transform.position;  // center of explosion

            m_Ray.direction = (m_TargetCollider.bounds.center - transform.position).normalized;
            if (Physics.Raycast(m_Ray, out m_RaycastHit, Radius + 1.0f) && (m_RaycastHit.transform.root.GetComponent<Collider>() == m_TargetCollider))
                return false;   // target's center / waist exposed

            // --- top / head ---
            m_Ray.direction = ((fp_3DUtility.HorizontalVector(m_TargetCollider.bounds.center) + (Vector3.up * (m_TargetCollider.bounds.max.y))) - transform.position).normalized;
            if (Physics.Raycast(m_Ray, out m_RaycastHit, Radius + 1.0f) && (m_RaycastHit.transform.root.GetComponent<Collider>() == m_TargetCollider))
                return false;   // target's top / head exposed

            return true;
        }
        protected void AddForce()
        {
            fp_TargetEvent<Vector3>.Send(m_TargetTransform.root, "ForceImpact", (m_TargetTransform.position -
                                                                                    transform.position).normalized *
                                                                                    Force * 0.001f * DistanceModifier);
        }
        public void TryDamage()
        {
            if (m_TargetCollider.CompareTag("Player"))
            {
                var damageble = m_TargetCollider.GetComponent<IDamageable>();
                if (damageble != null)
                {
                    damageble.TakeDamage(new DamageData
                    {
                        Type = DamageType.Explosion,
                        Damage = damage,
                        ImpactObject = gameObject
                    });
                }
            }
        }
    }
}