using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HyperShoot.Player;

namespace HyperShoot.Combat
{
	public class PlayerDamageHandler : DamageHandler
	{
		// falling damage
		public bool AllowFallDamage = true;
		public float FallDamageThreshold = 0.15f;
		public bool DeathOnFallImpactThreshold = false;
		protected float m_FallImpactMultiplier = 2;

		private CharacterEventHandler m_Player = null;  // should never be referenced directly
		protected CharacterEventHandler Player  // lazy initialization of the event handler field
		{
			get
			{
				if (m_Player == null)
					m_Player = transform.GetComponent<CharacterEventHandler>();
				return m_Player;
			}
		}

		protected List<Collider> m_Colliders = null;
		protected List<Collider> Colliders
		{
			get
			{
				if (m_Colliders == null)
				{
					m_Colliders = new List<Collider>();
					foreach (Collider c in GetComponentsInChildren<Collider>())
					{
						if (c.gameObject.layer == fp_Layer.RemotePlayer)
						{
							m_Colliders.Add(c);
						}
					}
				}
				return m_Colliders;
			}
		}
		protected virtual void OnEnable()
		{
			if (Player != null)
				Player.Register(this);

		}
		protected virtual void OnDisable()
		{
			if (Player != null)
				Player.Unregister(this);
		}
		public override void Die()
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
					fp_Utility.Instantiate(o, transform.position, transform.rotation);
			}

			foreach (Collider c in Colliders)
			{
				c.enabled = false;
			}

			Player.SetWeapon.Argument = 0;
			Player.SetWeapon.Start();
			Player.Dead.Start();
			Player.Run.Stop();
			Player.Jump.Stop();
			Player.Crouch.Stop();
			Player.Zoom.Stop();
			Player.Attack.Stop();
			Player.Reload.Stop();
			//Player.Climb.Stop();
			//Player.Interact.Stop();
		}
		protected override void Reset()
		{
			base.Reset();

			if (!Application.isPlaying)
				return;

			Player.Dead.Stop();
			Player.Stop.Send();

			foreach (Collider c in Colliders)
			{
				c.enabled = true;
			}

			//if ((Inventory != null) && !Inventory.enabled)
			//	Inventory.enabled = m_InventoryWasEnabledAtStart;

			//if (m_Audio != null)
			//{
			//	m_Audio.pitch = Time.timeScale;
			//	m_Audio.PlayOneShot(RespawnSound);
			//}
		}
		protected virtual void OnMessage_FallImpact(float impact)
		{
			if (!AllowFallDamage)
				return;

			if (Player.Dead.Active)
				return;

			if (impact <= FallDamageThreshold)
				return;

			float damage = (float)Mathf.Abs((float)(DeathOnFallImpactThreshold ? MaxHealth : MaxHealth * impact));

			TakeDamage(new DamageData
			{
				Type = DamageType.Fall,
				Damage = damage,
				ImpactObject = transform.gameObject
			});
		}
	}
}