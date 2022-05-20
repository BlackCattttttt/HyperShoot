using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

namespace HyperShoot.Combat
{
	public class DamageHandler : MonoBehaviour, IDamageable
	{
		// health and death
		public float MaxHealth = 1.0f;                      // initial health of the object instance, to be reset on respawn
		public GameObject[] DeathSpawnObjects = null;       // gameobjects to spawn when object dies.
															// TIP: could be fx, could also be rigidbody rubble
		public float MinDeathDelay = 0.0f;                  // random timespan in seconds to delay death. good for cool serial explosions
		public float MaxDeathDelay = 0.0f;
		public ReactiveProperty<float> HealthProperty;
		public ReadOnlyReactiveProperty<float> HealthObservable =>
	                     HealthProperty.ToReadOnlyReactiveProperty();
		protected bool m_InstaKill = false;                 // temporarily disables death delay, for example: on death by impact

        public IObservable<DamageData> TakeDamageObservable => _takeDamageSubject.AsObservable();

		private readonly Subject<DamageData> _takeDamageSubject = new Subject<DamageData>();

		private void Awake()
        {
			HealthProperty = new ReactiveProperty<float>(MaxHealth);
			TakeDamageObservable.Subscribe(TakeDamage).AddTo(this);
		}
        public virtual void  TakeDamage(DamageData data)
        {
			HealthProperty.Value = Mathf.Min(HealthProperty.Value - data.Damage, MaxHealth);

			if (HealthProperty.Value < 0)
            {
				if (m_InstaKill)
					Die();
				else
					fp_Timer.In(UnityEngine.Random.Range(MinDeathDelay, MaxDeathDelay), delegate () { Die(); });
			}
		}
		public virtual void Die()
		{

			if (!enabled || !fp_Utility.IsActive(gameObject))
				return;

			//if (m_Audio != null)
			//{
			//	m_Audio.pitch = Time.timeScale;
			//	m_Audio.PlayOneShot(DeathSound);
			//}

			foreach (GameObject o in DeathSpawnObjects)
			{
				if (o != null)
				{
					GameObject g = (GameObject)fp_Utility.Instantiate(o, transform.position, transform.rotation);
				}
			}

			//if (Respawner == null)
			//{
			//	vp_Utility.Destroy(gameObject);
			//}
			//else
			//{
			//	RemoveBulletHoles();
			//	vp_Utility.Activate(gameObject, false);
			//}

			m_InstaKill = false;
		}
		protected virtual void Reset()
		{
			HealthProperty.Value = MaxHealth;
		}
	}
}
