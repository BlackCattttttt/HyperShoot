#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using System.Collections;
using HyperShoot.Inventory;

public class InventoryItemsAttribute : PropertyAttribute
{
}


[CustomPropertyDrawer(typeof(InventoryItemsAttribute))]
public class InventoryItemsDrawer : PropertyDrawer
{

	private string m_InventoryFullCaption = "Failed to add item";
	private string m_InventoryFullMessage = "The inventory did not accept this item. This may be due to either an ITEM CAP or SPACE LIMIT.\n\nYou may want to review inventory settings under 'Item Caps' and 'Space Limit', aswell as the 'Inventory Space' setting of \"{0}\" and (in case that's a UnitBank) its 'Unit' type.";
	private string m_UnitBankAlreadyExistsCaption = "UnitBank already exists";
	private string m_UnitBankAlreadyExistsMessage = "This inventory already has an internal UnitBank for \"{0}\". You may adjust its unit count in the list.";

	private int NO_VALUE = -1;

	protected float m_InitialY = 0.0f;
	protected ItemInstance m_ItemToRemove;
	protected UnitBankInstance m_UnitBankToRemove;


	/// <summary>
	/// override to adjust with our own height. called by Unity
	/// </summary>
	public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
	{
		return prop.floatValue;
	}

	[SerializeField]
	private static bool m_ItemFoldout;
	[SerializeField]
	private static bool m_CapsFoldout;
	[SerializeField]
	private static bool m_SpaceFoldout;


	/// <summary>
	/// 
	/// </summary>
	public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
	{

		int indentLevelBak = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 1;

		prop.floatValue = DoItemsFoldout(pos, prop);
		EditorGUI.indentLevel = indentLevelBak;

	}


	/// <summary>
	/// 
	/// </summary>
	protected float DoItemsFoldout(Rect pos, SerializedProperty prop)
	{

		m_InitialY = pos.y;

		pos.height = 16;

		Inventory inventory = ((Inventory)prop.serializedObject.targetObject);

		GUI.backgroundColor = Color.white;

		pos.y += 
		((
		(inventory.ItemInstances.Count > 0) ||
		(inventory.UnitBankInstances.Count > 0) ||
		(inventory.InternalUnitBanks.Count > 0)) ?
		5 : -5); 
		pos.x += 20;
		pos.width -= 15;

		// --- draw internal unit banks ---

		for (int v = 0; v < inventory.InternalUnitBanks.Count; v++)
		{

			UnitBankInstance ibank = inventory.InternalUnitBanks[v];

			if ((ibank == null) || (ibank.UnitType == null))
			{
				inventory.InternalUnitBanks.Remove(ibank);
				inventory.Refresh();
				continue;
			}

			string name = ibank.UnitType.ToString();
			name = name.Remove(name.IndexOf(" (")) + "s (Internal UnitBank)";

			int unitCount = inventory.GetItemCount(ibank.UnitType);

			vp_PropertyDrawerUtility.ItemCard(pos,
				((ibank == null) ? null : ibank.UnitType.Icon),
				name,
				((ibank == null) ? null : ibank.UnitType),
				ref unitCount,
				"Units",
				delegate()
				{
					inventory.TrySetUnitCount(ibank.UnitType, unitCount);
				},
				ref NO_VALUE,
				"",
				null,
				delegate()
				{
					m_UnitBankToRemove = ibank;
				});

			pos.y += 21;
		}

		// --- draw item unit bank instances (such as weapons) ---

		for (int v = 0; v < inventory.UnitBankInstances.Count; v++)
		{

			UnitBankInstance bank = inventory.UnitBankInstances[v];

			int unitCount = bank.Count;

			if ((bank == null) || (bank.Type == null))
			{
				inventory.UnitBankInstances.Remove(bank);
				inventory.Refresh();
				continue;
			}

			string name = bank.Type.ToString();

			if (vp_PropertyDrawerUtility.ItemCard(pos,
				((bank == null) ? null : bank.Type.Icon),
				name,
				((bank == null) ? null : bank.Type),
				ref unitCount,
				"Units",
				delegate()
				{
					inventory.TrySetUnitCount(bank, unitCount);
				},
				ref bank.ID,
				"ID",
				null,	// no need to act on value change since it is handled by the ref above
				delegate()
				{
					m_UnitBankToRemove = bank;
				}))
			{
			}

			pos.y += 21;

		}

		// --- draw item instances ---

		for (int v = 0; v < inventory.ItemInstances.Count; v++)
		{

			ItemInstance item = inventory.ItemInstances[v];

			if ((item == null) || (item.Type == null))
			{
				inventory.ItemInstances.Remove(item);
				inventory.Refresh();
				continue;
			}

			string name = item.Type.ToString();

			if (vp_PropertyDrawerUtility.ItemCard(pos,
				((item == null) ? null : item.Type.Icon),
				name,
				((item == null) ? null : item.Type),
				ref item.ID,
				"ID",
				null,	// no need to act on value change since it is handled by the ref above
				ref NO_VALUE,
				"",
				null,
				delegate()
				{
					m_ItemToRemove = item;
				}, 45))
			{
			}

			pos.y += 21;

		}

		// --- draw 'add object' box ---

		pos.y += 16;

		vp_PropertyDrawerUtility.AddObjectBox(pos, "n ItemType", typeof(ItemType), delegate(Object itemType)
		{

			if (itemType is UnitBankType)
			{
				UnitBankType bank = (UnitBankType)itemType;
				int cap = inventory.GetItemCap((UnitBankType)itemType);

				if ((cap == -1) || (inventory.GetItemCount((UnitBankType)itemType) < cap))
				{
					if (!inventory.TryGiveUnitBank(bank, bank.Capacity, 0))
						EditorUtility.DisplayDialog(m_InventoryFullCaption, string.Format(m_InventoryFullMessage, itemType), "OK");
				}
				else
					EditorUtility.DisplayDialog(m_InventoryFullCaption, string.Format(m_InventoryFullMessage, itemType), "OK");
			}
			else if (itemType is UnitType)
			{
				if (!inventory.HaveInternalUnitBank((UnitType)itemType))
				{
					UnitType unitType = (UnitType)itemType;
					if (!inventory.TryGiveUnits(unitType, 10))
						EditorUtility.DisplayDialog(m_InventoryFullCaption, string.Format(m_InventoryFullMessage, itemType), "OK");
				}
				else
					EditorUtility.DisplayDialog(m_UnitBankAlreadyExistsCaption, string.Format(m_UnitBankAlreadyExistsMessage, itemType), "OK");
			}
			else
			{
				if (!inventory.TryGiveItem((ItemType)itemType, 0))
					EditorUtility.DisplayDialog(m_InventoryFullCaption, string.Format(m_InventoryFullMessage, itemType), "OK");
			}
			EditorUtility.SetDirty(inventory);
			inventory.Refresh();

		});

		pos.y += vp_PropertyDrawerUtility.CalcAddObjectBoxHeight - 5;

		// handle removed items
		if (m_UnitBankToRemove != null)
		{
			inventory.TryRemoveUnitBank(m_UnitBankToRemove);
			m_UnitBankToRemove = null;
		}
		else if (m_ItemToRemove != null)
		{
			inventory.TryRemoveItem(m_ItemToRemove);
			m_ItemToRemove = null;
		}

		return (pos.y - m_InitialY);

	}


}

#endif





