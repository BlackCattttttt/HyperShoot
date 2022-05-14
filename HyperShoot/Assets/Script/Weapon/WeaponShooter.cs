using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HyperShoot.Player;

namespace HyperShoot.Weapon
{
	public class WeaponShooter : BaseShooter
	{
		protected BaseWeapon m_Weapon = null;            // the weapon affected by the shooter

		// projectile
		public float ProjectileTapFiringRate = 0.1f;        // minimum delay between shots fired when fire button is tapped quickly and repeatedly
		protected float m_LastFireTime = 0.0f;
		protected float m_OriginalProjectileSpawnDelay = 0.0f;

		// motion
		public Vector3 MotionPositionRecoil = new Vector3(0, 0, -0.035f);   // positional force applied to weapon upon firing
		public Vector3 MotionRotationRecoil = new Vector3(-10.0f, 0, 0);    // angular force applied to weapon upon firing
		public float MotionRotationRecoilDeadZone = 0.5f;   // 'blind spot' center region for angular z recoil
		public float MotionDryFireRecoil = -0.1f;           // multiplies recoil when the weapon is out of ammo
		public float MotionRecoilDelay = 0.0f;              // delay between fire button pressed and recoil

		// muzzle flash
		public float MuzzleFlashFirstShotMaxDeviation = 180.0f; // max muzzleflash-to-fire-angle deviation for when the projectile is being fired from idle stance (disabled by default)
		protected bool m_WeaponWasInAttackStateLastFrame = false;           // work variables
		protected float m_MuzzleFlashWeaponAngle = 0.0f;
		protected float m_MuzzleFlashFireAngle = 0.0f;

		// sound
		public AudioClip SoundDryFire = null;               // out of ammo sound

		protected Quaternion m_MuzzlePointRotation = Quaternion.identity;

		// event handler property cast as a playereventhandler
		protected CharacterEventHandler m_Player = null;
		CharacterEventHandler Player
		{
			get
			{
				if (m_Player == null)
				{
					if (EventHandler != null)
						m_Player = (CharacterEventHandler)EventHandler;
				}
				return m_Player;
			}
		}


		public BaseWeapon Weapon
		{
			get
			{
				if (m_Weapon == null)
					m_Weapon = transform.GetComponent<BaseWeapon>();
				return m_Weapon;
			}
		}


		/// <summary>
		/// 
		/// </summary>
		protected override void Awake()
		{
			// if firing delegates haven't been set by a derived or external class yet, set them now
			if (GetFireSeed == null) GetFireSeed = delegate () { return Random.Range(0, 100); };
			if (GetFirePosition == null) GetFirePosition = delegate () { return FirePosition; };

			base.Awake();

			// reset the next allowed fire time
			m_NextAllowedFireTime = Time.time;

			ProjectileSpawnDelay = Mathf.Min(ProjectileSpawnDelay, (ProjectileFiringRate - 0.1f));
			m_OriginalProjectileSpawnDelay = ProjectileSpawnDelay;  // backup value since it may change at runtime
		}

		/// <summary>
		/// 
		/// </summary>
		protected override void LateUpdate()
		{
			if (Player == null)
				return;

			if (Player.IsFirstPerson == null)
				return;

			m_WeaponWasInAttackStateLastFrame = Weapon.StateManager.IsEnabled("Attack");

			base.LateUpdate();
		}

		/// <summary>
		/// in addition to spawning the projectile in the base class,
		/// plays a fire animation on the weapon and applies recoil
		/// to the weapon spring. also regulates tap fire
		/// </summary>
		protected override void Fire()
		{
			m_LastFireTime = Time.time;

			// apply recoil
			if (MotionRecoilDelay == 0.0f)
				ApplyRecoil();
			else
				fp_Timer.In(MotionRecoilDelay, ApplyRecoil);

			base.Fire();

			// keep 'ProjectileSpawnDelay' untouched outside of this scope
			// since other logics may rely on it
			ProjectileSpawnDelay = m_OriginalProjectileSpawnDelay;
		}

		protected override void ShowMuzzleFlash()
		{
			if (m_MuzzleFlash == null)
				return;

			if (MuzzleFlashFirstShotMaxDeviation == 180.0f      // this logic is disabled by default ...
				|| Player.IsFirstPerson.Get()                   // ... and only for 3rd person ...
				|| m_WeaponWasInAttackStateLastFrame            // ... and only for when firing the first shot of a salvo
				)
			{
				base.ShowMuzzleFlash();                         // so no muzzleflash hiding needed here: show it normally
				return;
			}

			m_MuzzleFlashWeaponAngle = Transform.eulerAngles.x + 90;
			m_MuzzleFlashFireAngle = m_CurrentFireRotation.eulerAngles.x + 90;
			m_MuzzleFlashWeaponAngle = ((m_MuzzleFlashWeaponAngle >= 360) ? (m_MuzzleFlashWeaponAngle - 360) : m_MuzzleFlashWeaponAngle);
			m_MuzzleFlashFireAngle = ((m_MuzzleFlashFireAngle >= 360) ? (m_MuzzleFlashFireAngle - 360) : m_MuzzleFlashFireAngle);

			if (Mathf.Abs(m_MuzzleFlashWeaponAngle - m_MuzzleFlashFireAngle) > MuzzleFlashFirstShotMaxDeviation)
				m_MuzzleFlash.SendMessage("ShootLightOnly", SendMessageOptions.DontRequireReceiver);    // show muzzle flash mesh + light
			else
				base.ShowMuzzleFlash();             // show muzzle flash light only
		}


		/// <summary>
		/// applies some advanced recoil motions on the weapon when fired
		/// </summary>
		protected virtual void ApplyRecoil()
		{
            // add a positional and angular force to the weapon for one frame
            if (MotionRotationRecoil.z == 0.0f)
                Weapon.AddForce2(MotionPositionRecoil, MotionRotationRecoil);
            else
            {
                // if we have rotation recoil around the z vector, also do dead zone logic
                Weapon.AddForce2(MotionPositionRecoil,
                    Vector3.Scale(MotionRotationRecoil, (Vector3.one + Vector3.back)) + // recoil around x & y
                    (((Random.value < 0.5f) ? Vector3.forward : Vector3.back) * // spin direction (left / right around z)
                    Random.Range(MotionRotationRecoil.z * MotionRotationRecoilDeadZone, MotionRotationRecoil.z)));
            }
        }

		public virtual void DryFire()
		{

			if (Audio != null)
			{
				Audio.pitch = Time.timeScale;
				Audio.PlayOneShot(SoundDryFire);
			}

			DisableFiring();

			m_LastFireTime = Time.time;

			// apply dryfire recoil
			Weapon.AddForce2(MotionPositionRecoil * MotionDryFireRecoil, MotionRotationRecoil * MotionDryFireRecoil);
		}

		public void OnMessage_DryFire()
		{
			DryFire();
		}

		protected virtual void OnStop_Attack()
		{
			if (ProjectileFiringRate == 0)
			{
				EnableFiring();
				return;
			}

			DisableFiring(ProjectileTapFiringRate - (Time.time - m_LastFireTime));
		}

		protected virtual bool OnAttempt_Fire()
		{
			// weapon can only be fired when firing rate allows it
			if (Time.time < m_NextAllowedFireTime)
				return false;

			// weapon can only be fired if it has ammo (or doesn't require ammo).
			// NOTE: on success this call will remove ammo, so it's done only once
			// everything else checks out
			//if (!Player.DepleteAmmo.Try())
			//{
			//	DryFire();
			//	return false;
			//}

			Fire();

			return true;
		}
	}
}
