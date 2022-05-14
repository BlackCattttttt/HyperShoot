using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HyperShoot.Player;

namespace HyperShoot.Weapon
{
    public class FPWeaponReloader : WeaponReloader
    {
		public AnimationClip AnimationReload = null;

		FPWeapon m_FPWeapon = null;
		FPWeapon FPWeapon
		{
			get
			{
				if (m_FPWeapon == null)
					m_FPWeapon = (m_Weapon as FPWeapon);
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

		protected override void OnStart_Reload()
		{
			base.OnStart_Reload();

			if (AnimationReload == null)
				return;

			// if reload duration is zero, fetch duration from the animation
			if (m_Player.Reload.AutoDuration == 0.0f)
				m_Player.Reload.AutoDuration = AnimationReload.length;

			WeaponAnimation.CrossFade(AnimationReload.name);
		}
	}
}
