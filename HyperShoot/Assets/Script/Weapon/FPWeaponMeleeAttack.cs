using UnityEngine;
using System.Collections.Generic;
using HyperShoot.Player;
using HyperShoot.Weapon;
using HyperShoot.Bullet;
using HyperShoot.Combat;

namespace HyperShoot.Weapon
{
	public class FPWeaponMeleeAttack : fp_Component
	{

#if (UNITY_EDITOR)
		public bool DrawDebugObjects = false;
#endif
		public float damage;
		public string WeaponStatePull = "Pull";         // weapon state for pulling back the weapon pre-slash
		public string WeaponStateSwing = "Swing";       // weapon state for the slash. NOTE: this is not a slash in itself. it just

		// swing
		public float SwingDelay = 0.5f;         // delay until slash begins after weapon has been raised
		public float SwingDuration = 0.5f;      // delay until the weapon swing is stopped
		public float SwingRate = 1.0f;
		protected float m_NextAllowedSwingTime = 0.0f;
		public int SwingSoftForceFrames = 50;               // number of frames over which to apply the forces of each attack
		public Vector3 SwingPositionSoftForce = new Vector3(-0.5f, -0.1f, 0.3f);
		public Vector3 SwingRotationSoftForce = new Vector3(50, -25, 0);

		// impact
		public float ImpactTime = 0.11f;
		public Vector3 ImpactPositionSpringRecoil = new Vector3(0.01f, 0.03f, -0.05f);
		public Vector3 ImpactPositionSpring2Recoil = Vector3.zero;
		public Vector3 ImpactRotationSpringRecoil = Vector3.zero;
		public Vector3 ImpactRotationSpring2Recoil = new Vector3(0.0f, 0.0f, 10.0f);

		// attack
		public bool AttackPickRandomState = true;
		protected int m_AttackCurrent = 0;  // current randomly selected attack

		// sounds
		public List<UnityEngine.Object> SoundSwing = new List<UnityEngine.Object>();    // list of impact sounds to be randomly played
		public Vector2 SoundSwingPitch = new Vector2(0.5f, 1.5f);   // random pitch range for swing sounds

		// timers
		fp_Timer.Handle SwingDelayTimer = new fp_Timer.Handle();
		fp_Timer.Handle ImpactTimer = new fp_Timer.Handle();
		fp_Timer.Handle SwingDurationTimer = new fp_Timer.Handle();
		fp_Timer.Handle ResetTimer = new fp_Timer.Handle();

		FPCharacterEventHandler m_Player = null;
		FPCharacterEventHandler Player
		{
			get
			{
				if (m_Player == null)
				{
					if (EventHandler != null)
						m_Player = (FPCharacterEventHandler)EventHandler;
				}
				return m_Player;
			}
		}

		FPWeapon m_FPWeapon = null;
		FPWeapon FPWeapon
		{
			get
			{
				if (m_FPWeapon == null)
					m_FPWeapon = Transform.GetComponent<FPWeapon>();
				return m_FPWeapon;
			}
		}

		WeaponShooter m_WeaponShooter = null;
		WeaponShooter WeaponShooter
		{
			get
			{
				if (m_WeaponShooter == null)
				{
					m_WeaponShooter = Transform.GetComponent<WeaponShooter>();
					if (m_WeaponShooter == null)
					{
						m_WeaponShooter = gameObject.AddComponent<WeaponShooter>();
					}
					else if (m_WeaponShooter is BaseShooter)
					{
						m_WeaponShooter.enabled = false;
						m_WeaponShooter = gameObject.AddComponent<WeaponShooter>();
					}
				}
				return m_WeaponShooter;
			}
		}

		protected FPCamera m_FPCamera = null;
		public FPCamera FPCamera
		{
			get
			{
				if (m_FPCamera == null)
					m_FPCamera = Root.GetComponentInChildren<FPCamera>();
				return m_FPCamera;
			}
		}

		protected BaseController m_FPController = null;
		public BaseController FPController
		{
			get
			{
				if (m_FPController == null)
					m_FPController = Root.GetComponentInChildren<BaseController>();
				return m_FPController;
			}
		}

		protected BaseBullet m_Bullet = null;
		public BaseBullet Bullet
		{
			get
			{
				if (m_Bullet == null && (WeaponShooter != null) && (WeaponShooter.ProjectilePrefab != null))
				{
					m_Bullet = WeaponShooter.ProjectilePrefab.GetComponent<BaseBullet>();
				}
				return m_Bullet;
			}
		}

		protected override void Awake()
		{
			base.Awake();

			if (WeaponShooter != null)
			{
				WeaponShooter.ProjectileFiringRate = SwingRate;
				WeaponShooter.ProjectileTapFiringRate = SwingRate;
				WeaponShooter.ProjectileSpawnDelay = SwingDelay;
				WeaponShooter.ProjectileScale = 1;
				WeaponShooter.ProjectileCount = 1;
				WeaponShooter.ProjectileSpread = 0;

				if (WeaponShooter.Weapon != null)
					WeaponShooter.Weapon.AnimationType = (int)BaseWeapon.Type.Melee;
			}
		}

		protected override void OnEnable()
		{
			RefreshFirePoint();
			base.OnEnable();
		}

		protected override void Update()
		{
			base.Update();
			UpdateAttack();
		}

		protected void UpdateAttack()
		{
			if (!Player.Attack.Active)
				return;

			if (Player.SetWeapon.Active)
				return;

			if (FPWeapon == null)
				return;

			if (!FPWeapon.Wielded)
				return;

			if (Time.time < m_NextAllowedSwingTime)
				return;

			m_NextAllowedSwingTime = Time.time + SwingRate;

			if (AttackPickRandomState)
				PickAttack();

			FPWeapon.SetState(WeaponStatePull);
			FPWeapon.Refresh();

			fp_Timer.In(SwingDelay, delegate ()
			{
			// play a random swing sound
			if (SoundSwing.Count > 0)
				{
					Audio.pitch = Random.Range(SoundSwingPitch.x, SoundSwingPitch.y) * Time.timeScale;
					Audio.clip = (AudioClip)SoundSwing[(int)Random.Range(0, (SoundSwing.Count))];
					if (fp_Utility.IsActive(gameObject))
						Audio.Play();
				}

			// switch to the swing state
			FPWeapon.SetState(WeaponStatePull, false);
				FPWeapon.SetState(WeaponStateSwing);
				FPWeapon.Refresh();

			// apply soft forces of the current attack
			FPWeapon.AddSoftForce(SwingPositionSoftForce, SwingRotationSoftForce, SwingSoftForceFrames);

			// check for target impact after a predetermined duration
			fp_Timer.In(ImpactTime, delegate ()
				{
					RaycastHit hit;
					Ray ray = new Ray(new Vector3(FPController.Transform.position.x, FPCamera.Transform.position.y,
													FPController.Transform.position.z), FPCamera.Transform.forward);

					Physics.Raycast(ray, out hit, (Bullet != null ? Bullet.Range : 2), fp_Layer.Mask.BulletBlockers);

					if (hit.collider != null)
					{
						var damageable = hit.collider.GetComponent<IDamageable>();
						if (damageable != null)
						{
							damageable.TakeDamage(new DamageData
							{
								Type = DamageType.Bullet,
								Damage = damage,
								ImpactObject = gameObject
							});
						}
						ApplyRecoil();
					}
					else
					{
						fp_Timer.In(SwingDuration - ImpactTime, delegate ()
						{
							FPWeapon.StopSprings();
							Reset();
						}, SwingDurationTimer);
					}
				}, ImpactTimer);
			}, SwingDelayTimer);
		}

		void PickAttack()
		{
			int attack = States.Count - 1;

		reroll:

			attack = UnityEngine.Random.Range(0, States.Count - 1);
			if ((States.Count > 1) && (attack == m_AttackCurrent) && (Random.value < 0.5f))
				goto reroll;

			m_AttackCurrent = attack;

			SetState(States[m_AttackCurrent].Name);
		}

		void ApplyRecoil()
		{
			FPWeapon.StopSprings();
			FPWeapon.AddForce2(ImpactPositionSpring2Recoil, ImpactRotationSpring2Recoil);
			Reset();
		}

		void Reset()
		{
			fp_Timer.In(0.05f, delegate ()
			{
				if (FPWeapon != null)
				{
					FPWeapon.SetState(WeaponStatePull, false);
					FPWeapon.SetState(WeaponStateSwing, false);
					FPWeapon.Refresh();
					if (AttackPickRandomState)
						ResetState();
				}
			}, ResetTimer);
		}

		void RefreshFirePoint()
		{
			if (WeaponShooter == null)
				return;

			if (Player.IsFirstPerson == null)
				return;

			if (Player.IsFirstPerson.Get())
				WeaponShooter.m_ProjectileSpawnPoint = FPCamera.gameObject;
		}
	}
}
