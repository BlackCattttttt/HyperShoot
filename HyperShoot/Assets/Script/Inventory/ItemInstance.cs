using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace HyperShoot.Inventory
{
	[System.Serializable]
	public class ItemInstance
	{
		[SerializeField]
		public ItemType Type;
		[SerializeField]
		public int ID = 0;

		[SerializeField]
		public ItemInstance(ItemType type, int id)
		{
			ID = id;
			Type = type;
		}

		public virtual void SetUniqueID()
		{
			ID = fp_Utility.UniqueID;
		}
	}
}




