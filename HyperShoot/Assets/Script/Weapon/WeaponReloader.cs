using HyperShoot.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperShoot.Weapon
{
	public class WeaponReloader : MonoBehaviour
	{
		protected BaseWeapon m_Weapon = null;
		protected CharacterEventHandler m_Player = null;

		//protected AudioSource m_Audio = null;
		//public AudioClip SoundReload = null;

		public float ReloadDuration = 1.0f;

		protected virtual void Awake()
		{
		//	m_Audio = GetComponent<AudioSource>();

			// store the first player event handler found in the top of our transform hierarchy
			m_Player = (CharacterEventHandler)transform.root.GetComponentInChildren(typeof(CharacterEventHandler));
		}

		protected virtual void Start()
		{
			// store a reference to the FPSWeapon
			m_Weapon = transform.GetComponent<BaseWeapon>();
		}

		protected virtual void OnEnable()
		{
			// allow this monobehaviour to talk to the player event handler
			if (m_Player != null)
				m_Player.Register(this);
		}

		protected virtual void OnDisable()
		{
			// unregister this monobehaviour from the player event handler
			if (m_Player != null)
				m_Player.Unregister(this);
		}

		protected virtual bool CanStart_Reload()
		{
			// can't reload if current weapon isn't fully wielded
			if (m_Player.CurrentWeaponWielded.Get() == false)
				return false;

			// can't reload if weapon is full
			if (m_Player.CurrentWeaponMaxAmmoCount.Get() != 0 &&    // only check if max capacity is reported
				(m_Player.CurrentWeaponAmmoCount.Get() == m_Player.CurrentWeaponMaxAmmoCount.Get()))
				return false;

			// can't reload if the inventory has no additional ammo for this weapon
			if (m_Player.CurrentWeaponClipCount.Get() < 1)
			{
				return false;
			}

			return true;
		}

		protected virtual void OnStart_Reload()
		{
			// end the Reload activity in 'ReloadDuration' seconds
			m_Player.Reload.AutoDuration = m_Player.CurrentWeaponReloadDuration.Get();

			//if (m_Audio != null)
			//{
			//	m_Audio.pitch = Time.timeScale;
			//	m_Audio.PlayOneShot(SoundReload);
			//}
		}

		protected virtual void OnStop_Reload()
		{
			m_Player.RefillCurrentWeapon.Try();
		}

		protected virtual float OnValue_CurrentWeaponReloadDuration
		{
			get
			{
				return ReloadDuration;
			}
		}
	}
}
