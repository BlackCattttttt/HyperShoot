using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperShoot.Inventory
{
    public class Inventory : MonoBehaviour
    {
        [System.Serializable]
        public class ItemRecordsSection
        {

#if UNITY_EDITOR
            [InventoryItems]
            public float itemList;

#endif

        }
        [SerializeField]
        protected ItemRecordsSection m_ItemRecords;
        [System.Serializable]
        public class ItemCap
        {

            [SerializeField]
            public ItemType Type = null;
            [SerializeField]
            public int Cap = 0;
            [SerializeField]
            public ItemCap(ItemType type, int cap)
            {
                Type = type;
                Cap = cap;
            }
        }

        [SerializeField]
        [HideInInspector]
        public List<ItemInstance> ItemInstances = new List<ItemInstance>();

        [SerializeField]
        [HideInInspector]
        public List<ItemCap> m_ItemCapInstances = new List<ItemCap>();

        [SerializeField]
        [HideInInspector]
        protected List<UnitBankInstance> m_UnitBankInstances = new List<UnitBankInstance>();
        public List<UnitBankInstance> UnitBankInstances
        {
            get
            {
                return m_UnitBankInstances;
            }
        }
        [SerializeField]
        [HideInInspector]
        protected List<UnitBankInstance> m_InternalUnitBanks = new List<UnitBankInstance>();
        public List<UnitBankInstance> InternalUnitBanks
        {
            get
            {
                return m_InternalUnitBanks;
            }
        }

        protected const int UNLIMITED = -1;
        protected const int UNIDENTIFIED = -1;
        protected const int MAXCAPACITY = -1;

        [SerializeField]
        [HideInInspector]
        public bool CapsEnabled = false;

        [SerializeField]
        [HideInInspector]
        public bool AllowOnlyListed = false;

        protected struct StartItemRecord
        {
            public ItemType Type;
            public int ID;
            public int Amount;
            public StartItemRecord(ItemType type, int id, int amount)
            {
                Type = type;
                ID = id;
                Amount = amount;
            }
        }
        protected bool m_Result;
        protected List<StartItemRecord> m_StartItems = new List<StartItemRecord>();
        protected bool m_FirstItemsDirty = true;
        protected Dictionary<ItemType, ItemInstance> m_FirstItemsOfType = new Dictionary<ItemType, ItemInstance>(100);
        protected ItemInstance m_GetFirstItemInstanceResult;
        protected bool m_ItemDictionaryDirty = true;
        protected Dictionary<int, ItemInstance> m_ItemDictionary = new Dictionary<int, ItemInstance>();
        protected ItemInstance m_GetItemResult;

        protected virtual void Awake()
        {
            SaveInitialState();
        }
        protected virtual void Start()
        {
            Refresh();
        }
        protected virtual void OnEnable()
        {
            fp_TargetEventReturn<Inventory>.Register(transform, "GetInventory", GetInventory);
            fp_TargetEventReturn<ItemType, int, bool>.Register(transform, "TryGiveItem", TryGiveItem);
            fp_TargetEventReturn<ItemType, int, bool>.Register(transform, "TryGiveItems", TryGiveItems);
            fp_TargetEventReturn<UnitBankType, int, int, bool>.Register(transform, "TryGiveUnitBank", TryGiveUnitBank);
            fp_TargetEventReturn<UnitType, int, bool>.Register(transform, "TryGiveUnits", TryGiveUnits);
            fp_TargetEventReturn<UnitBankType, int, int, bool>.Register(transform, "TryDeduct", TryDeduct);
            fp_TargetEventReturn<ItemType, int>.Register(transform, "GetItemCount", GetItemCount);
        }
        protected virtual void OnDisable()
        {
            fp_TargetEventReturn<Inventory>.Unregister(transform, "GetInventory", GetInventory);
            fp_TargetEventReturn<ItemType, int, bool>.Unregister(transform, "TryGiveItem", TryGiveItem);
            fp_TargetEventReturn<ItemType, int, bool>.Unregister(transform, "TryGiveItems", TryGiveItems);
            fp_TargetEventReturn<UnitBankType, int, int, bool>.Unregister(transform, "TryGiveUnitBank", TryGiveUnitBank);
            fp_TargetEventReturn<UnitType, int, bool>.Unregister(transform, "TryGiveUnits", TryGiveUnits);
            fp_TargetEventReturn<UnitBankType, int, int, bool>.Unregister(transform, "TryDeduct", TryDeduct);
            fp_TargetEventReturn<ItemType, int>.Unregister(transform, "GetItemCount", GetItemCount);
        }
        protected virtual Inventory GetInventory()
        {
            return this;
        }
        public virtual bool TryGiveItems(ItemType type, int amount)
        {
            bool result = false;
            while (amount > 0)
            {
                if (TryGiveItem(type, 0))
                    result = true;
                amount--;
            }
            return result;
        }
        public virtual bool TryGiveItem(ItemType itemType, int id)
        {
            if (itemType == null)
            {
                return false;
            }

            // forward to the correct method if this was a unit type
            UnitType unitType = itemType as UnitType;
            if (unitType != null)
                return TryGiveUnits(unitType, id);  // in this case treat int argument as 'amount'

            // forward to the correct method if this was a unitbank type
            UnitBankType unitBankType = itemType as UnitBankType;
            if (unitBankType != null)
                return TryGiveUnitBank(unitBankType, unitBankType.Capacity, id);

            // enforce item cap for this type of item
            if (CapsEnabled)
            {
                int capacity = GetItemCap(itemType);
                if ((capacity != UNLIMITED) && (GetItemCount(itemType) >= capacity))
                    return false;
            }

            DoAddItem(itemType, id);

            return true;
        }
        protected virtual void DoAddItem(ItemType type, int id)
        {
            //Debug.Log("DoAddItem");
            ItemInstances.Add(new ItemInstance(type, id));
            m_FirstItemsDirty = true;
            m_ItemDictionaryDirty = true;
        }
        protected virtual void DoRemoveItem(ItemInstance item)
        {
            //Debug.Log("DoRemoveItem");
            if (item as UnitBankInstance != null)
            {
                DoRemoveUnitBank(item as UnitBankInstance);
                return;
            }

            ItemInstances.Remove(item);

            m_FirstItemsDirty = true;
            m_ItemDictionaryDirty = true;
        }
        protected virtual void DoAddUnitBank(UnitBankType unitBankType, int id, int unitsLoaded)
        {
            //Debug.Log("DoAddUnitBank");
            UnitBankInstance bank = new UnitBankInstance(unitBankType, id, this);
            m_UnitBankInstances.Add(bank);
            m_FirstItemsDirty = true;
            m_ItemDictionaryDirty = true;
            bank.TryGiveUnits(unitsLoaded);
        }
        protected virtual void DoRemoveUnitBank(UnitBankInstance bank)
        {
            //Debug.Log("DoRemoveUnitBank");
            if (!bank.IsInternal)
            {
                m_UnitBankInstances.RemoveAt(m_UnitBankInstances.IndexOf(bank));
                m_FirstItemsDirty = true;
                m_ItemDictionaryDirty = true;
            }
            else
                m_InternalUnitBanks.RemoveAt(m_InternalUnitBanks.IndexOf(bank));
        }
        public virtual bool DoAddUnits(UnitBankInstance bank, int amount)
        {
            //Debug.Log("DoAddUnits");
            return bank.DoAddUnits(amount);
        }
        public virtual bool DoRemoveUnits(UnitBankInstance bank, int amount)
        {
            //Debug.Log("DoRemoveUnits");
            return bank.DoRemoveUnits(amount);
        }
        public virtual bool TryGiveUnits(UnitType unitType, int amount)
        {
            //Debug.Log("TryGiveUnits: " + unitType + ", " + amount);
            if (GetItemCap(unitType) == 0)
                return false;

            return TryGiveUnits(GetInternalUnitBank(unitType), amount);
        }
        public virtual bool TryGiveUnits(UnitBankInstance bank, int amount)
        {
            if (bank == null)
                return false;

            amount = Mathf.Max(0, amount);

            return DoAddUnits(bank, amount);
        }
        public virtual bool TryRemoveUnits(UnitType unitType, int amount)
        {
            UnitBankInstance bank = GetInternalUnitBank(unitType);
            if (bank == null)
                return false;

            return DoRemoveUnits(bank, amount);
        }
        public virtual bool TryGiveUnitBank(UnitBankType unitBankType, int unitsLoaded, int id)
        {
            //Debug.Log("TryGiveUnitBank: " + unitBankType + ", " + unitsLoaded);
            if (unitBankType == null)
            {
                return false;
            }

            if (CapsEnabled)
            {
                // enforce item cap for this type of unitbank
                int capacity = GetItemCap(unitBankType);
                if ((capacity != UNLIMITED) && (GetItemCount(unitBankType) >= capacity))
                    return false;

                // enforce unit capacity of the unitbank type
                if (unitBankType.Capacity != UNLIMITED)
                    unitsLoaded = Mathf.Min(unitsLoaded, unitBankType.Capacity);

            }

            DoAddUnitBank(unitBankType, id, unitsLoaded);

            return true;
        }
        public virtual bool TryRemoveItems(ItemType type, int amount)
        {
            bool result = false;
            while (amount > 0)
            {
                if (TryRemoveItem(type, UNIDENTIFIED))
                    result = true;
                amount--;
            }
            return result;
        }
        public virtual bool TryRemoveItem(ItemType type, int id)
        {
            return TryRemoveItem(GetItem(type, id) as ItemInstance);
        }
        public virtual bool TryRemoveItem(ItemInstance item)
        {
            if (item == null)
                return false;

            DoRemoveItem(item);

            return true;
        }
        public virtual bool TryRemoveUnitBank(UnitBankType type, int id)
        {
            return TryRemoveUnitBank(GetItem(type, id) as UnitBankInstance);
        }
        public virtual bool TryRemoveUnitBank(UnitBankInstance unitBank)
        {
            if (unitBank == null)
                return false;

            DoRemoveUnitBank(unitBank);

            return true;
        }
        public virtual bool TryReload(ItemType itemType, int unitBankId)
        {
            return TryReload(GetItem(itemType, unitBankId) as UnitBankInstance, MAXCAPACITY);
        }
        public virtual bool TryReload(UnitBankInstance bank)
        {
            return TryReload(bank, MAXCAPACITY);
        }
        public virtual bool TryReload(UnitBankInstance bank, int amount)
        {
            if ((bank == null) || (bank.IsInternal) || (bank.ID == UNIDENTIFIED))
            {
                return false;
            }

            // fetch the amount of units in the unitbank prior to reloading
            int preLoadedUnits = bank.Count;

            // if unitbank is full, there's no point in reloading
            if (preLoadedUnits >= bank.Capacity)
                return false;

            // fetch the current amount of suitable units in the inventory
            int prevInventoryCount = GetUnitCount(bank.UnitType);

            // if inventory is empty, there's not much more to do
            if (prevInventoryCount < 1)
                return false;

            // an amount of -1 means 'fill her up'
            if (amount == MAXCAPACITY)
                amount = bank.Capacity;

            // remove as many units as we can from the inventory
            TryRemoveUnits(bank.UnitType, amount);

            // figure out how many units we managed to remove from inventory
            int unitsRemoved = Mathf.Max(0, (prevInventoryCount - GetUnitCount(bank.UnitType)));

            if (!DoAddUnits(bank, unitsRemoved))
                return false;

            // let's see how many units we managed to transfer to the unitbank
            int unitsLoaded = Mathf.Max(0, (bank.Count - preLoadedUnits));

            // if we managed to load zero units, report failure
            if (unitsLoaded < 1)
                return false;

            if ((unitsLoaded > 0) && (unitsLoaded < unitsRemoved))
                TryGiveUnits(bank.UnitType, (unitsRemoved - unitsLoaded));

            return true;
        }
        public virtual bool TryDeduct(UnitBankType unitBankType, int unitBankId, int amount)
        {
            UnitBankInstance bank = ((unitBankId < 1) ? GetItem(unitBankType) as UnitBankInstance :
                                                            GetItem(unitBankType, unitBankId) as UnitBankInstance);

            if (bank == null)
                return false;

            if (!DoRemoveUnits(bank, amount))
                return false;

            if ((bank.Count <= 0) && ((bank.Type as UnitBankType).RemoveWhenDepleted))
                DoRemoveUnitBank(bank);

            return true;
        }
        public virtual ItemInstance GetItem(ItemType itemType)
        {
            if (m_FirstItemsDirty)
            {
                //Debug.Log("recreating the 'm_FirstItemsOfType' dictionary");
                m_FirstItemsOfType.Clear();
                foreach (ItemInstance itemInstance in ItemInstances)
                {
                    if (itemInstance == null)
                        continue;
                    if (!m_FirstItemsOfType.ContainsKey(itemInstance.Type))
                        m_FirstItemsOfType.Add(itemInstance.Type, itemInstance);
                }
                foreach (UnitBankInstance itemInstance in UnitBankInstances)
                {
                    if (itemInstance == null)
                        continue;
                    if (!m_FirstItemsOfType.ContainsKey(itemInstance.Type))
                        m_FirstItemsOfType.Add(itemInstance.Type, itemInstance);
                }
                m_FirstItemsDirty = false;
            }

            //Debug.Log("trying to fetch an instance of the target item type");
            if ((itemType == null) || !m_FirstItemsOfType.TryGetValue(itemType, out m_GetFirstItemInstanceResult))
            {
                //Debug.Log("no match: returning null");
                return null;
            }

            //Debug.Log("an instance of the target item type was found: perform a null check");
            if (m_GetFirstItemInstanceResult == null)
            {
                //Debug.Log("the instance was null: so refresh dictionary and run the method all over again");
                m_FirstItemsDirty = true;
                return GetItem(itemType);
            }

            //Debug.Log("item was found");
            return m_GetFirstItemInstanceResult;
        }
        public ItemInstance GetItem(ItemType itemType, int id)
        {
            if (itemType == null)
            {
                return null;
            }

            if (id < 1)
            {
                //Debug.Log("no ID was specified: returning the first item of matching type");
                return GetItem(itemType);
            }

            if (m_ItemDictionaryDirty)
            {
                //Debug.Log("resetting the dictionary");
                m_ItemDictionary.Clear();
                m_ItemDictionaryDirty = false;
            }

            //Debug.Log("we have an ID (" + id + "): try to fetch an associated instance from the dictionary");
            if (!m_ItemDictionary.TryGetValue(id, out m_GetItemResult))
            {
                //Debug.Log("DID NOT find item in the dictionary: trying to find it in the list");
                m_GetItemResult = GetItemFromList(itemType, id);
                if ((m_GetItemResult != null) && (id > 0))
                {
                    //Debug.Log("found id '"+id+"' in the list! adding it to dictionary");
                    m_ItemDictionary.Add(id, m_GetItemResult);
                }
            }
            else if (m_GetItemResult != null)
            {
                //Debug.Log("DID find a quick-match by ID ("+id+") in the dictionary: verifying the item type");
                if (m_GetItemResult.Type != itemType)
                {
                    m_GetItemResult = GetItemFromList(itemType, id);
                }
            }
            else
            {
                m_ItemDictionary.Remove(id);
                GetItem(itemType, id);
            }

            return m_GetItemResult;
        }
        public virtual ItemInstance GetItem(string itemTypeName)
        {
            //Debug.Log("itemTypeName: " + itemTypeName);
            for (int v = 0; v < InternalUnitBanks.Count; v++)
            {
                if (InternalUnitBanks[v].UnitType.name == itemTypeName)
                    return InternalUnitBanks[v];
            }

            for (int v = 0; v < m_UnitBankInstances.Count; v++)
            {
                if (m_UnitBankInstances[v].Type.name == itemTypeName)
                    return m_UnitBankInstances[v];
            }

            for (int v = 0; v < ItemInstances.Count; v++)
            {
                if (ItemInstances[v].Type.name == itemTypeName)
                    return ItemInstances[v];
            }

            return null;
        }
        protected virtual ItemInstance GetItemFromList(ItemType itemType, int id = UNIDENTIFIED)
        {
            for (int v = 0; v < m_UnitBankInstances.Count; v++)
            {
                if (m_UnitBankInstances[v].Type != itemType)
                    continue;
                if ((id == UNIDENTIFIED) || m_UnitBankInstances[v].ID == id)
                    return m_UnitBankInstances[v];
            }

            for (int v = 0; v < ItemInstances.Count; v++)
            {
                if (ItemInstances[v].Type != itemType)
                    continue;
                if ((id == UNIDENTIFIED) || ItemInstances[v].ID == id)
                    return ItemInstances[v];
            }

            return null;
        }
        public virtual bool HaveItem(ItemType itemType, int id = UNIDENTIFIED)
        {
            if (itemType == null)
                return false;
            return GetItem(itemType, id) != null;
        }
        public virtual UnitBankInstance GetInternalUnitBank(UnitType unitType)
        {
            for (int v = 0; v < m_InternalUnitBanks.Count; v++)
            {
                // is item a unit bank?
                if (m_InternalUnitBanks[v].GetType() != typeof(UnitBankInstance))
                    continue;

                if (m_InternalUnitBanks[v].Type != null)
                    continue;
                UnitBankInstance b = (UnitBankInstance)m_InternalUnitBanks[v];
                if (b.UnitType != unitType)
                    continue;
                return b;
            }

            UnitBankInstance bank = new UnitBankInstance(unitType, this);

            bank.Capacity = GetItemCap(unitType);

            m_InternalUnitBanks.Add(bank);

            return bank;
        }
        public virtual bool HaveInternalUnitBank(UnitType unitType)
        {
            for (int v = 0; v < m_InternalUnitBanks.Count; v++)
            {
                // is item a unit bank?
                if (m_InternalUnitBanks[v].GetType() != typeof(UnitBankInstance))
                    continue;

                // is item internal? (has no vp_ItemType)
                if (m_InternalUnitBanks[v].Type != null)
                    continue;
                UnitBankInstance b = (UnitBankInstance)m_InternalUnitBanks[v];
                if (b.UnitType != unitType)
                    continue;
                return true;
            }

            return false;
        }
        public virtual void Refresh()
        {
            for (int v = 0; v < m_InternalUnitBanks.Count; v++)
            {
                m_InternalUnitBanks[v].Capacity = GetItemCap(m_InternalUnitBanks[v].UnitType);
            }
        }
        public virtual int GetItemCount(ItemType type)
        {
            UnitType unitType = type as UnitType;
            if (unitType != null)
                return GetUnitCount(unitType);

            int count = 0;

            for (int v = 0; v < ItemInstances.Count; v++)
            {
                if (ItemInstances[v].Type == type)
                    count++;
            }

            for (int v = 0; v < m_UnitBankInstances.Count; v++)
            {
                if (m_UnitBankInstances[v].Type == type)
                    count++;
            }

            return count;
        }
        public virtual bool TrySetUnitCount(UnitType unitType, int amount)
        {
            return TrySetUnitCount(GetInternalUnitBank((UnitType)unitType), amount);
        }
        public virtual bool TrySetUnitCount(UnitBankInstance bank, int amount)
        {
            if (bank == null)
                return false;

            amount = Mathf.Max(0, amount);

            if (amount == bank.Count)
                return true;

            int prevInventoryCount = bank.Count;

            if (!DoRemoveUnits(bank, bank.Count))
                bank.Count = prevInventoryCount;

            if (amount == 0)
                return true;

            if (bank.IsInternal)
            {
                m_Result = TryGiveUnits(bank.UnitType, amount);
                if (m_Result == false)
                    bank.Count = prevInventoryCount;
                return m_Result;
            }

            m_Result = TryGiveUnits(bank, amount);
            if (m_Result == false)
                bank.Count = prevInventoryCount;
            return m_Result;
        }
        public virtual int GetItemCap(ItemType type)
        {
            if (!CapsEnabled)
                return UNLIMITED;

            for (int v = 0; v < m_ItemCapInstances.Count; v++)
            {
                if (m_ItemCapInstances[v].Type == type)
                    return m_ItemCapInstances[v].Cap;
            }

            if (AllowOnlyListed)
                return 0;

            return UNLIMITED;
        }
        public virtual void SetItemCap(ItemType type, int cap, bool clamp = false)
        {
            for (int v = 0; v < m_ItemCapInstances.Count; v++)
            {
                // if so, change the cap
                if (m_ItemCapInstances[v].Type == type)
                {
                    m_ItemCapInstances[v].Cap = cap;
                    goto found;
                }
            }

            m_ItemCapInstances.Add(new ItemCap(type, cap));

        found:

            // if type is a unit, update capacity of the unit bank
            if (type is UnitType)
            {
                for (int v = 0; v < m_InternalUnitBanks.Count; v++)
                {
                    if ((m_InternalUnitBanks[v].UnitType != null) && (m_InternalUnitBanks[v].UnitType == type))
                    {
                        m_InternalUnitBanks[v].Capacity = cap;
                        // clamp amount of units, if specified
                        if (clamp)
                            m_InternalUnitBanks[v].ClampToCapacity();
                    }
                }
            }
            // clamp amount of items, if specified
            else if (clamp)
            {
                if (GetItemCount(type) > cap)
                    TryRemoveItems(type, (GetItemCount(type) - cap));
            }
        }
        public virtual int GetUnitCount(UnitType unitType)
        {
            UnitBankInstance v = GetInternalUnitBank(unitType);
            if (v == null)
                return 0;
            return v.Count;
        }
        public virtual void SaveInitialState()
        {
            for (int v = 0; v < InternalUnitBanks.Count; v++)
            {
                m_StartItems.Add(new StartItemRecord(InternalUnitBanks[v].UnitType, 0, InternalUnitBanks[v].Count));
            }

            for (int v = 0; v < m_UnitBankInstances.Count; v++)
            {
                m_StartItems.Add(new StartItemRecord(m_UnitBankInstances[v].Type, m_UnitBankInstances[v].ID, m_UnitBankInstances[v].Count));
            }

            for (int v = 0; v < ItemInstances.Count; v++)
            {
                m_StartItems.Add(new StartItemRecord(ItemInstances[v].Type, ItemInstances[v].ID, 1));
            }
        }
        public virtual void Reset()
        {
            Clear();

            for (int v = 0; v < m_StartItems.Count; v++)
            {
                if (m_StartItems[v].Type.GetType() == typeof(ItemType))
                    TryGiveItem(m_StartItems[v].Type, m_StartItems[v].ID);
                else if (m_StartItems[v].Type.GetType() == typeof(UnitBankType))
                    TryGiveUnitBank((m_StartItems[v].Type as UnitBankType), m_StartItems[v].Amount, m_StartItems[v].ID);
                else if (m_StartItems[v].Type.GetType() == typeof(UnitType))
                    TryGiveUnits((m_StartItems[v].Type as UnitType), m_StartItems[v].Amount);
                else if (m_StartItems[v].Type.GetType().BaseType == typeof(ItemType))
                    TryGiveItem(m_StartItems[v].Type, m_StartItems[v].ID);
                else if (m_StartItems[v].Type.GetType().BaseType == typeof(UnitBankType))
                    TryGiveUnitBank((m_StartItems[v].Type as UnitBankType), m_StartItems[v].Amount, m_StartItems[v].ID);
                else if (m_StartItems[v].Type.GetType().BaseType == typeof(UnitType))
                    TryGiveUnits((m_StartItems[v].Type as UnitType), m_StartItems[v].Amount);
            }
        }
        public virtual void Clear()
        {
            for (int v = InternalUnitBanks.Count - 1; v > -1; v--)
            {
                DoRemoveUnitBank(InternalUnitBanks[v]);
            }
            for (int v = m_UnitBankInstances.Count - 1; v > -1; v--)
            {
                DoRemoveUnitBank(m_UnitBankInstances[v]);
            }
            for (int v = ItemInstances.Count - 1; v > -1; v--)
            {
                DoRemoveItem(ItemInstances[v]);
            }
        }
    }
}
