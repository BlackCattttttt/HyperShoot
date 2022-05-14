using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HyperShoot.Player;

namespace HyperShoot.Weapon
{
    [RequireComponent(typeof(FPWeapon))]
    public class FPWeaponShooter : WeaponShooter
    {
		// motion
		public float MotionPositionReset = 0.5f;            // how much to reset weapon to its normal position upon firing (0-1)
		public float MotionRotationReset = 0.5f;
		public float MotionPositionPause = 1.0f;            // time interval over which to freeze and fade swaying forces back in upon firing
		public float MotionRotationPause = 1.0f;

		// animation
		public AnimationClip AnimationFire = null;
		public AnimationClip AnimationOutOfAmmo = null;

		// event handler property cast as an FPPlayerEventHandler
		protected FPCharacterEventHandler Player
		{
			get
			{
				if (m_Player == null)
				{
					if (EventHandler != null)
						m_Player = (FPCharacterEventHandler)EventHandler;
				}
				return (FPCharacterEventHandler)m_Player;
			}
		}

		protected FPWeapon m_FPWeapon = null;            // the weapon affected by the shooter
		public FPWeapon FPWeapon
		{
			get
			{
				if (m_FPWeapon == null)
					m_FPWeapon = transform.GetComponent<FPWeapon>();
				return m_FPWeapon;
			}
		}

		Animation m_WeaponAnimation = null;
		public Animation WeaponAnimation
		{
			get
			{
				if (m_WeaponAnimation == null)
				{
					if (FPWeapon == null)
						return null;
					if (FPWeapon.WeaponModel == null)
						return null;
					m_WeaponAnimation = FPWeapon.WeaponModel.GetComponent<Animation>();
				}
				return m_WeaponAnimation;
			}
		}

		protected FPCamera m_FPCamera = null;
		public FPCamera FPCamera
		{
			get
			{
				if (m_FPCamera == null)
					m_FPCamera = transform.root.GetComponentInChildren<FPCamera>();
				return m_FPCamera;
			}
		}

		protected override void Awake()
		{
			base.Awake();

			if (m_ProjectileSpawnPoint == null)
				m_ProjectileSpawnPoint = FPCamera.gameObject;

			m_ProjectileDefaultSpawnpoint = m_ProjectileSpawnPoint;

			// reset the next allowed fire time
			m_NextAllowedFireTime = Time.time;

			ProjectileSpawnDelay = Mathf.Min(ProjectileSpawnDelay, (ProjectileFiringRate - 0.1f));
		}

		protected override void OnEnable()
		{
			RefreshFirePoint();
			base.OnEnable();
		}

		protected override void Start()
		{

			base.Start();

			// defaults for using animation length as the fire and reload delay
			if (ProjectileFiringRate == 0.0f && AnimationFire != null)
				ProjectileFiringRate = AnimationFire.length;
		}

		protected override void Fire()
		{
			m_LastFireTime = Time.time;

			// play fire animation
			if (AnimationFire != null)
			{
				if (WeaponAnimation[AnimationFire.name] != null)
				{
					WeaponAnimation[AnimationFire.name].time = 0.0f;
					WeaponAnimation.Sample();
					WeaponAnimation.Play(AnimationFire.name);
				}
			}

			// apply recoil
			if (MotionRecoilDelay == 0.0f)
				ApplyRecoil();
			else
				fp_Timer.In(MotionRecoilDelay, ApplyRecoil);

			base.Fire();

			//if (AnimationOutOfAmmo != null)
			//{
			//	if (m_Player.CurrentWeaponAmmoCount.Get() == 0)
			//	{
			//		if (WeaponAnimation[AnimationOutOfAmmo.name] == null)
			//			Debug.LogError("Error (" + this + ") No animation named '" + AnimationOutOfAmmo.name + "' is listed in this prefab. Make sure the prefab has an 'Animation' component which references all the clips you wish to play on the weapon.");
			//		else
			//		{
			//			WeaponAnimation[AnimationOutOfAmmo.name].time = 0.0f;
			//			WeaponAnimation.Sample();
			//			WeaponAnimation.Play(AnimationOutOfAmmo.name);
			//		}
			//	}
			//}
		}

		protected override void ApplyRecoil()
		{
			base.ApplyRecoil();
			FPWeapon.ResetSprings(MotionPositionReset, MotionRotationReset,
								MotionPositionPause, MotionRotationPause);
		}

		void RefreshFirePoint()
		{
			if (Player.IsFirstPerson == null)
				return;

			// --- 1st PERSON ---
			if (Player.IsFirstPerson.Get())
			{
				m_ProjectileSpawnPoint = FPCamera.gameObject;
				if (MuzzleFlash != null)
					MuzzleFlash.layer = fp_Layer.Weapon;
				m_MuzzleFlashSpawnPoint = null;
				Refresh();
			}
		}
	}
}
