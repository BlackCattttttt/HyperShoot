using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HyperShoot.Player;

namespace HyperShoot.Weapon
{
	public class WeaponThrower : MonoBehaviour
	{
		public float AttackMinDuration = 1.0f;
		protected float m_OriginalAttackMinDuration = 0.0f;

		protected FPWeapon m_FPWeapon = null;
		public FPWeapon FPWeapon
		{
			get
			{
				if (m_FPWeapon == null)
					m_FPWeapon = (FPWeapon)transform.GetComponent(typeof(FPWeapon));
				return m_FPWeapon;
			}
		}

		protected FPWeaponShooter m_FPWeaponShooter = null;
		public FPWeaponShooter FPWeaponShooter
		{
			get
			{
				if (m_FPWeaponShooter == null)
					m_FPWeaponShooter = (FPWeaponShooter)transform.GetComponent(typeof(FPWeaponShooter));
				return m_FPWeaponShooter;
			}
		}
		protected WeaponShooter m_Shooter = null;
		public WeaponShooter Shooter
		{
			get
			{
				if (m_Shooter == null)
					m_Shooter = (WeaponShooter)transform.GetComponent(typeof(WeaponShooter));
				return m_Shooter;
			}
		}
		//protected fp_UnitBankType m_UnitBankType = null;
		//public fp_UnitBankType UnitBankType
		//{
		//	get
		//	{
		//		if (ItemIdentifier == null)
		//			return null;
		//		fp_ItemType iType = m_ItemIdentifier.GetItemType();
		//		if (iType == null)
		//			return null;
		//		fp_UnitBankType uType;
		//		uType = iType as fp_UnitBankType;
		//		if (uType == null)
		//			return null;
		//		return uType;
		//	}
		//}

		//protected fp_UnitBankInstance m_UnitBank = null;
		//public fp_UnitBankInstance UnitBank
		//{
		//	get
		//	{
		//		if ((m_UnitBank == null) && (UnitBankType != null) && (Inventory != null))
		//		{
		//			foreach (fp_UnitBankInstance iu in Inventory.UnitBankInstances)
		//			{
		//				if (iu.UnitType == UnitBankType.Unit)
		//					m_UnitBank = iu;
		//			}
		//		}
		//		return m_UnitBank;
		//	}
		//}

		//protected fp_ItemIdentifier m_ItemIdentifier = null;
		//public fp_ItemIdentifier ItemIdentifier
		//{
		//	get
		//	{
		//		if (m_ItemIdentifier == null)
		//			m_ItemIdentifier = (fp_ItemIdentifier)Transform.GetComponent(typeof(fp_ItemIdentifier));
		//		return m_ItemIdentifier;
		//	}
		//}

		protected CharacterEventHandler m_Player = null;
		public CharacterEventHandler Player
		{
			get
			{
				if (m_Player == null)
					m_Player = (CharacterEventHandler)transform.root.GetComponentInChildren(typeof(CharacterEventHandler));
				return m_Player;
			}
		}

		//protected fp_PlayerInventory m_Inventory = null;
		//public fp_PlayerInventory Inventory
		//{
		//	get
		//	{
		//		if (m_Inventory == null)
		//			m_Inventory = (fp_PlayerInventory)Root.GetComponentInChildren(typeof(fp_PlayerInventory));
		//		return m_Inventory;
		//	}
		//}

		protected virtual void OnEnable()
		{
			if (Player == null)
				return;

			Player.Register(this);

			TryStoreAttackMinDuration();

			// cap the amount of weaponthrowers of this type to one
			//Inventory.SetItemCap(ItemIdentifier.Type, 1, true);
			//Inventory.CapsEnabled = true;
		}

		protected virtual void OnDisable()
		{
			TryRestoreAttackMinDuration();

			if (Player != null)
				Player.Unregister(this);
		}

		protected virtual void Start()
		{
			TryStoreAttackMinDuration();
		}

		void TryStoreAttackMinDuration()
		{
			if (Player.Attack == null)
				return;

			if (m_OriginalAttackMinDuration == 0.0f)
				return;

			m_OriginalAttackMinDuration = Player.Attack.MinDuration;
			Player.Attack.MinDuration = AttackMinDuration;
		}

		void TryRestoreAttackMinDuration()
		{

			if (Player.Attack == null)
				return;

			if (m_OriginalAttackMinDuration != 0.0f)
				return;

			Player.Attack.MinDuration = m_OriginalAttackMinDuration;

		}

		protected bool HaveAmmoForCurrentWeapon
		{
			get
			{
				return ((Player.CurrentWeaponAmmoCount.Get() > 0)
						|| (Player.CurrentWeaponClipCount.Get() > 0));
			}
		}

		//protected virtual bool TryReload()
		//{
		//	if (UnitBank == null)
		//		return false;

		//	return Inventory.TryReload(UnitBank);
		//}


		protected virtual void OnStart_Attack()
		{
			//if (Player.CurrentWeaponAmmoCount.Get() < 1)
			//	TryReload();

			fp_Timer.In(Shooter.ProjectileSpawnDelay + 0.5f, delegate ()
			{
				Player.Attack.Stop();
			});
		}

		protected virtual bool CanStart_Reload()
		{
			return false;
		}

		protected virtual void OnStop_Attack()
		{
		//	TryReload();
		}

		protected virtual void OnStop_SetWeapon()
		{
		//	m_UnitBank = null;

		}
	}
}