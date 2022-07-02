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

		public ReactiveProperty<float> HealthProperty;
		public ReadOnlyReactiveProperty<float> HealthObservable =>
	                     HealthProperty.ToReadOnlyReactiveProperty();
		protected bool m_InstaKill = false;                 // temporarily disables death delay, for example: on death by impact

        public IObservable<DamageData> TakeDamageObservable => _takeDamageSubject.AsObservable();

		private readonly Subject<DamageData> _takeDamageSubject = new Subject<DamageData>();

		private bool _isDie;

		private void Awake()
        {
			_isDie = false;
			HealthProperty = new ReactiveProperty<float>(MaxHealth);
			TakeDamageObservable.Subscribe(TakeDamage).AddTo(this);
		}
        public virtual void  TakeDamage(DamageData data)
        {
			HealthProperty.Value = Mathf.Min(HealthProperty.Value - data.Damage, MaxHealth);

			if (HealthProperty.Value < 0)
            {
				if (!_isDie)
					Die();
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
			_isDie = true;
			foreach (GameObject o in DeathSpawnObjects)
			{
				if (o != null)
				{
					GameObject g = (GameObject)fp_Utility.Instantiate(o, transform.position, transform.rotation);
				}
			}

			//if (Respawner == null)
			//{
			//	fp_Utility.Destroy(gameObject);
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
			_isDie = false;
			HealthProperty.Value = MaxHealth;
		}
	}
}
