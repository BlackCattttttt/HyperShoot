using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperShoot.Bullet
{
    public class BaseEnemyBullet : MonoBehaviour
    {
        [SerializeField] protected float damage;
        [SerializeField] protected Collider bulletCollider;
        [SerializeField] protected Rigidbody rigidBody;
        [SerializeField] protected ParticleSystem destroyParticle;
        [SerializeField] protected float lifeTime = 1f;

        protected float CurrentLifeTime;
        protected virtual bool CanDamageOnEnter { get; }
        protected virtual bool CanDamageOnStay { get; }

        public Rigidbody Rigidbody => rigidBody;
        protected virtual void Awake()
        {
            transform.parent = BulletContainer.Instance.transform;
        }

        protected virtual void OnEnable()
        {
            ResetBullet();
        }

        protected virtual void FixedUpdate()
        {
            CurrentLifeTime -= Time.fixedDeltaTime;
            if (CurrentLifeTime < 0f)
            {
                DeSpawn();
            }
        }

        public virtual void Shoot(Vector3 direction, float thrust, ForceMode forceMode = ForceMode.Impulse)
        {
            rigidBody.AddRelativeForce(direction * thrust, forceMode);
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (CanDamageOnEnter)
                DamageTarget(other);
        }

        protected virtual void OnTriggerStay(Collider other)
        {
            if (CanDamageOnStay)
                DamageTarget(other);
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            ExitTarget(other);
        }

        protected virtual void OnCollisionEnter(Collision other)
        {
            if (CanDamageOnEnter)
                DamageTarget(other.collider);
        }

        protected virtual void OnCollisionStay(Collision other)
        {
            if (CanDamageOnStay)
                DamageTarget(other.collider);
        }
        protected virtual void DamageTarget(Collider other)
        {
        }
        protected virtual void OnCollisionExit(Collision other)
        {
            ExitTarget(other.collider);
        }

        protected virtual void ExitTarget(Collider other)
        {
        }
        public virtual void SetDamage(float damage)
        {
            this.damage = damage;
        }
        protected virtual void DeSpawn()
        {
            if (destroyParticle != null)
            {
                SimplePool.Spawn(destroyParticle, EffectContainer.Instance.transform, transform.position, transform.rotation);
            }
            SimplePool.Despawn(gameObject);
        }

        protected virtual void ResetBullet()
        {
            CurrentLifeTime = lifeTime;
            rigidBody.velocity = Vector3.zero;
        }

        public virtual void SetVelocity(Vector2 velocity)
        {
            rigidBody.velocity = velocity;
        }
    }
}
