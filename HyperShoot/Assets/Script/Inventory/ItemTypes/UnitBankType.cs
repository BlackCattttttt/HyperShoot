using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace HyperShoot.Inventory
{
	[System.Serializable]
	public class UnitBankType : ItemType
	{
		[SerializeField]
		public UnitType Unit = null;
		[SerializeField]
		public int Capacity = 10;
		[SerializeField]
		public bool Reloadable = true;
		[SerializeField]
		public bool RemoveWhenDepleted = false;
	}
}



