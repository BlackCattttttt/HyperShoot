using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace HyperShoot.Inventory
{
	[System.Serializable]
	public class UnitBankInstance : ItemInstance
	{
		private const int UNLIMITED = -1;

		[SerializeField]
		public UnitType UnitType;

		[SerializeField]
		public int Count = 0;
		[SerializeField]
		protected int m_Capacity = UNLIMITED;

		[SerializeField]
		protected Inventory m_Inventory;

		protected bool m_Result;
		protected int m_PrevCount = 0;

		public int Capacity
		{
			get
			{
				if (Type != null)
					m_Capacity = ((UnitBankType)Type).Capacity;
				return m_Capacity;
			}
			set
			{
				m_Capacity = Mathf.Max(UNLIMITED, value);
			}
		}

		[SerializeField]
		public UnitBankInstance(UnitBankType unitBankType, int id, Inventory inventory)
			: base(unitBankType, id)
		{
			UnitType = unitBankType.Unit;
			m_Inventory = inventory;
		}

		[SerializeField]
		public UnitBankInstance(UnitType unitType, Inventory inventory)
			: base(null, 0)
		{
			UnitType = unitType;
			m_Inventory = inventory;
		}

		public virtual bool TryRemoveUnits(int amount)
		{
			if (Count <= 0)
				return false;

			amount = Mathf.Max(0, amount);

			if (amount == 0)
				return false;

			Count = Mathf.Max(0, (Count - amount));

			return true;
		}

		public virtual bool TryGiveUnits(int amount)
		{
			if ((Type != null) && ((UnitBankType)Type).Reloadable == false)
				return false;

			if ((Capacity != UNLIMITED) && (Count >= Capacity))
				return false;

			amount = Mathf.Max(0, amount);

			if (amount == 0)
				return false;

			Count += amount;

			if (Count <= Capacity)
				return true;

			if (Capacity == UNLIMITED)
				return true;

			Count = Capacity;

			return true;
		}

		public virtual bool IsInternal
		{
			get
			{
				return Type == null;
			}
		}

		public virtual bool DoAddUnits(int amount)
		{
			m_PrevCount = Count;
			m_Result = TryGiveUnits(amount);
			return m_Result;
		}

		public virtual bool DoRemoveUnits(int amount)
		{
			m_PrevCount = Count;
			m_Result = TryRemoveUnits(amount);
			return m_Result;
		}

		public virtual int ClampToCapacity()
		{

			int prevCount = Count;

			if (Capacity != UNLIMITED)
				Count = Mathf.Clamp(Count, 0, Capacity);

			Count = Mathf.Max(0, Count);

			return prevCount - Count;

		}
	}

}

