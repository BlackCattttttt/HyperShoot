using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace HyperShoot.Inventory
{
	public class ItemIdentifier : MonoBehaviour
	{
		public ItemType Type = null;
		public int ID;

		protected virtual void OnEnable()
		{
			fp_TargetEventReturn<ItemType>.Register(this.transform, "GetItemType", GetItemType);
			fp_TargetEventReturn<int>.Register(this.transform, "GetItemID", GetItemID);
		}

		protected virtual void OnDisable()
		{

		}

		public virtual ItemType GetItemType()
		{
			return Type;
		}

		public virtual int GetItemID()
		{
			return ID;
		}
	}
}