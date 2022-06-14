using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HyperShoot.Player;
using HyperShoot.Weapon;

namespace HyperShoot.Inventory
{
    public class PlayerInventory : Inventory
    {
        protected Dictionary<ItemType, object> m_PreviouslyOwnedItems = new Dictionary<ItemType, object>();
        protected ItemIdentifier m_WeaponIdentifierResult;
        protected Dictionary<ItemType, UnitBankInstance> m_ThrowingWeaponUnitBankInstances = new Dictionary<ItemType, UnitBankInstance>();
        protected Dictionary<UnitType, UnitBankType> m_ThrowingWeaponUnitBankTypes = new Dictionary<UnitType, UnitBankType>();
        protected List<UnitType> m_ThrowingWeaponUnitTypes = new List<UnitType>();
        protected bool m_HaveThrowingWeaponInfo = false;

        protected Dictionary<BaseWeapon, ItemIdentifier> m_WeaponIdentifiers = null;
        public Dictionary<BaseWeapon, ItemIdentifier> WeaponIdentifiers
        {
            get
            {
                if (m_WeaponIdentifiers == null)
                {
                    m_WeaponIdentifiers = new Dictionary<BaseWeapon, ItemIdentifier>();
                    foreach (BaseWeapon w in WeaponHandler.Weapons)
                    {
                        ItemIdentifier i = w.GetComponent<ItemIdentifier>();
                        if (i != null)
                        {
                            m_WeaponIdentifiers.Add(w, i);
                        }
                    }
                }
                return m_WeaponIdentifiers;
            }
        }

        protected Dictionary<UnitType, List<BaseWeapon>> m_WeaponsByUnit = null;
        public Dictionary<UnitType, List<BaseWeapon>> WeaponsByUnit
        {
            get
            {
                if (m_WeaponsByUnit == null)
                {
                    m_WeaponsByUnit = new Dictionary<UnitType, List<BaseWeapon>>();
                    foreach (BaseWeapon w in WeaponHandler.Weapons)
                    {
                        ItemIdentifier i;
                        if (WeaponIdentifiers.TryGetValue(w, out i) && (i != null))
                        {
                            UnitBankType uType = i.Type as UnitBankType;
                            if ((uType != null) && (uType.Unit != null))
                            {
                                List<BaseWeapon> weaponsWithUnitType;
                                if (m_WeaponsByUnit.TryGetValue(uType.Unit, out weaponsWithUnitType))
                                {
                                    if (weaponsWithUnitType == null)
                                        weaponsWithUnitType = new List<BaseWeapon>();
                                    m_WeaponsByUnit.Remove(uType.Unit);
                                }
                                else
                                    weaponsWithUnitType = new List<BaseWeapon>();
                                weaponsWithUnitType.Add(w);
                                m_WeaponsByUnit.Add(uType.Unit, weaponsWithUnitType);
                            }
                        }
                    }

                }
                return m_WeaponsByUnit;
            }
        }

        protected ItemInstance m_CurrentWeaponInstance = null;
        protected virtual ItemInstance CurrentWeaponInstance
        {
            get
            {
                if (Application.isPlaying && (WeaponHandler.CurrentWeaponIndex == 0))
                {
                    m_CurrentWeaponInstance = null;
                    return null;
                }

                if (m_CurrentWeaponInstance == null)
                {
                    if (CurrentWeaponIdentifier == null)
                    {
                        MissingIdentifierError();
                        m_CurrentWeaponInstance = null;
                        return null;
                    }
                    m_CurrentWeaponInstance = GetItem(CurrentWeaponIdentifier.Type, CurrentWeaponIdentifier.ID);
                }

                return m_CurrentWeaponInstance;

            }
        }
        private CharacterEventHandler m_Player = null;  // should never be referenced directly
        protected CharacterEventHandler Player  // lazy initialization of the event handler field
        {
            get
            {
                if (this == null)
                    return null;
                if (m_Player == null)
                    m_Player = transform.GetComponent<CharacterEventHandler>();
                return m_Player;
            }
        }
        private WeaponHandler m_WeaponHandler = null;    // should never be referenced directly
        protected WeaponHandler WeaponHandler    // lazy initialization of the weapon handler field
        {
            get
            {
                if (m_WeaponHandler == null)
                    m_WeaponHandler = transform.GetComponent<WeaponHandler>();
                return m_WeaponHandler;
            }
        }
        public ItemIdentifier CurrentWeaponIdentifier
        {
            get
            {
                if (!Application.isPlaying)
                    return null;
                return GetWeaponIdentifier(WeaponHandler.CurrentWeapon);
            }
        }
        protected virtual ItemIdentifier GetWeaponIdentifier(BaseWeapon weapon)
        {
            if (!Application.isPlaying)
                return null;

            if (weapon == null)
                return null;

            if (!WeaponIdentifiers.TryGetValue(weapon, out m_WeaponIdentifierResult))
            {

                if (weapon == null)
                    return null;

                m_WeaponIdentifierResult = weapon.GetComponent<ItemIdentifier>();

                if (m_WeaponIdentifierResult == null)
                    return null;

                if (m_WeaponIdentifierResult.Type == null)
                    return null;

                WeaponIdentifiers.Add(weapon, m_WeaponIdentifierResult);

            }
            return m_WeaponIdentifierResult;
        }
        [System.Serializable]
        public class AutoWieldSection
        {
            public bool Always = false;
            public bool IfUnarmed = true;
            public bool IfOutOfAmmo = true;
            public bool IfNotPresent = true;
            public bool FirstTimeOnly = true;
        }
        [SerializeField]
        protected AutoWieldSection m_AutoWield;

        protected override void Awake()
        {
            base.Awake();
        }
        protected override void Start()
        {
            base.Start();
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            // allow this monobehaviour to talk to the player event handler
            if (Player != null)
                Player.Register(this);
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            // unregister this monobehaviour from the player event handler
            if (Player != null)
                Player.Unregister(this);
        }
        protected virtual bool MissingIdentifierError(int weaponIndex = 0)
        {
            if (!Application.isPlaying)
                return false;

            if (weaponIndex < 1)
                return false;

            if (WeaponHandler == null)
                return false;

            if (!(WeaponHandler.Weapons.Count > weaponIndex - 1))
                return false;

            return false;
        }
        protected override void DoAddItem(ItemType type, int id)
        {
            bool alreadyHaveIt = HaveItem(type, id);

            base.DoAddItem(type, id);

            TryWieldNewItem(type, alreadyHaveIt);
        }
        protected override void DoRemoveItem(ItemInstance item)
        {
            Unwield(item);
            base.DoRemoveItem(item);
        }
        protected override void DoAddUnitBank(UnitBankType unitBankType, int id, int unitsLoaded)
        {
            bool alreadyHaveIt = HaveItem(unitBankType, id); 

            base.DoAddUnitBank(unitBankType, id, unitsLoaded);

            TryWieldNewItem(unitBankType, alreadyHaveIt);
        }
        protected virtual void TryWieldNewItem(ItemType type, bool alreadyHaveIt)
        {
            bool haveHadItBefore = m_PreviouslyOwnedItems.ContainsKey(type);
            if (!haveHadItBefore)
                m_PreviouslyOwnedItems.Add(type, null);

            // --- see if we should try to wield a weapon because of this item pickup ---

            if ((m_AutoWield != null) && m_AutoWield.Always)
                goto tryWield;

            if ((m_AutoWield != null) && m_AutoWield.IfUnarmed && (WeaponHandler.CurrentWeaponIndex < 1))
                goto tryWield;

            if ((m_AutoWield != null) && m_AutoWield.IfOutOfAmmo
                && (WeaponHandler.CurrentWeaponIndex > 0)
                && (WeaponHandler.CurrentWeapon.AnimationType != (int)BaseWeapon.Type.Melee)
                && m_Player.CurrentWeaponAmmoCount.Get() < 1)
                goto tryWield;

            if ((m_AutoWield != null) && m_AutoWield.IfNotPresent && !m_AutoWield.FirstTimeOnly && !alreadyHaveIt)
                goto tryWield;

            if ((m_AutoWield != null) && m_AutoWield.FirstTimeOnly && !haveHadItBefore)
                goto tryWield;

            return;

        tryWield:

            if ((type is UnitBankType))
                TryWield(GetItem(type));
            else if (type is UnitType)
                TryWieldByUnit(type as UnitType);
            else if (type is ItemType)   // tested last since the others derive from it
                TryWield(GetItem(type));
            else
            {
                System.Type baseType = type.GetType();
                if (baseType == null)
                    return;
                baseType = baseType.BaseType;
                if ((baseType == typeof(UnitBankType)))
                    TryWield(GetItem(type));
                else if (baseType == typeof(UnitType))
                    TryWieldByUnit(type as UnitType);
                else if (baseType == typeof(ItemType))
                    TryWield(GetItem(type));
            }
        }
        protected override void DoRemoveUnitBank(UnitBankInstance bank)
        {
            Unwield(bank);
            base.DoRemoveUnitBank(bank);
        }
        public override bool DoAddUnits(UnitBankInstance bank, int amount)
        {
            if (bank == null)
                return false;

            int prevUnitCount = GetUnitCount(bank.UnitType);

            bool result = base.DoAddUnits(bank, amount);

            // if units were added to the inventory (and not to a weapon)
            if ((result == true && bank.IsInternal))
            {

                try
                {
                    TryWieldNewItem(bank.UnitType, (prevUnitCount != 0));
                }
                catch
                {
                    
                }

                if (!((Application.isPlaying) && WeaponHandler.CurrentWeaponIndex == 0))
                {

                    UnitBankInstance curBank = (CurrentWeaponInstance as UnitBankInstance);
                    if (curBank != null)
                    {

                        if ((bank.UnitType == curBank.UnitType) && (curBank.Count == 0))
                        {
                            Player.AutoReload.Try();    // try to auto-reload (success determined by weaponhandler)
                        }
                    }
                }
            }
            return result;
        }
        public override bool DoRemoveUnits(UnitBankInstance bank, int amount)
        {
            bool result = base.DoRemoveUnits(bank, amount);

            if (bank.Count == 0)
                fp_Timer.In(0.3f, delegate () { Player.AutoReload.Try(); });        // try to auto-reload (success determined by weaponhandler)

            return result;
        }
        public UnitBankInstance GetUnitBankInstanceOfWeapon(BaseWeapon weapon)
        {
            return GetItemInstanceOfWeapon(weapon) as UnitBankInstance;
        }
        public ItemInstance GetItemInstanceOfWeapon(BaseWeapon weapon)
        {
            ItemIdentifier itemIdentifier = GetWeaponIdentifier(weapon);
            if (itemIdentifier == null)
                return null;

            ItemInstance ii = GetItem(itemIdentifier.Type);

            return ii;
        }
        public int GetAmmoInWeapon(BaseWeapon weapon)
        {
            UnitBankInstance unitBank = GetUnitBankInstanceOfWeapon(weapon);
            if (unitBank == null)
                return 0;

            return unitBank.Count;
        }
        public int GetAmmoInCurrentWeapon()
        {
            return OnValue_CurrentWeaponAmmoCount;
        }
        public int GetExtraAmmoForCurrentWeapon()
        {
            return OnValue_CurrentWeaponClipCount;
        }
        protected virtual void StoreThrowingWeaponInfo()
        {
            foreach (BaseWeapon weapon in WeaponHandler.Weapons)
            {
                if (!(weapon.AnimationType == (int)BaseWeapon.Type.Thrown))
                    continue;

                // ... and it has an item identifier ...
                ItemIdentifier identifier = weapon.GetComponent<ItemIdentifier>();
                if (identifier == null)
                    continue;

                // ... and the identifier is for a unitbank ...
                UnitBankType unitBankType = (identifier.GetItemType() as UnitBankType);
                if (unitBankType == null)
                    continue;

                // --- then consider it a throwing weapon unitbank type ---

                // store the unitbank type under its unit type
                if (!m_ThrowingWeaponUnitBankTypes.ContainsKey(unitBankType.Unit))
                    m_ThrowingWeaponUnitBankTypes.Add(unitBankType.Unit, unitBankType);

                // store the unit type as known throwing weapon ammo
                if (!m_ThrowingWeaponUnitTypes.Contains(unitBankType.Unit))
                    m_ThrowingWeaponUnitTypes.Add(unitBankType.Unit);

                // if the inventory has a unitbank instance for this weapon ...
                UnitBankInstance unitBankInstance = GetUnitBankInstanceOfWeapon(weapon);
                if (unitBankInstance == null)
                    continue;

                // ... then store it by its unitbank type
                if (!m_ThrowingWeaponUnitBankInstances.ContainsKey(unitBankType))
                    m_ThrowingWeaponUnitBankInstances.Add(unitBankType, unitBankInstance);

            }
            m_HaveThrowingWeaponInfo = true;
        }
        public virtual UnitBankInstance GetThrowingWeaponUnitBankInstance(UnitBankType unitBankType)
        {
            if (WeaponHandler == null)
                return null;

            if (!m_HaveThrowingWeaponInfo)
                StoreThrowingWeaponInfo();

            UnitBankInstance unitBankInstance = null;
            m_ThrowingWeaponUnitBankInstances.TryGetValue(unitBankType, out unitBankInstance);

            return unitBankInstance;
        }
        public virtual UnitBankType GetThrowingWeaponUnitBankType(UnitType unitType)
        {
            if (!m_HaveThrowingWeaponInfo)
                StoreThrowingWeaponInfo();
            UnitBankType unitBankType = null;
            m_ThrowingWeaponUnitBankTypes.TryGetValue(unitType, out unitBankType);
            return unitBankType;
        }
        public virtual bool IsThrowingUnit(UnitType unitType)
        {
            if (!m_HaveThrowingWeaponInfo)
                StoreThrowingWeaponInfo();

            return (m_ThrowingWeaponUnitTypes.Contains(unitType));
        }
        protected virtual void UnwieldMissingWeapon()
        {
            if (!Application.isPlaying)
                return;

            if (WeaponHandler.CurrentWeaponIndex < 1)
                return;

            if ((CurrentWeaponIdentifier != null) &&
                HaveItem(CurrentWeaponIdentifier.Type, CurrentWeaponIdentifier.ID))
                return;

            if (CurrentWeaponIdentifier == null)
                MissingIdentifierError(WeaponHandler.CurrentWeaponIndex);

            Player.SetWeapon.TryStart(0);
        }
        protected bool TryWieldByUnit(UnitType unitType)
        {
            // try to find a weapon with this unit type
            List<BaseWeapon> weaponsWithUnitType;
            if (WeaponsByUnit.TryGetValue(unitType, out weaponsWithUnitType)
                && (weaponsWithUnitType != null)
                && (weaponsWithUnitType.Count > 0)
                )
            {
                // try to set the first weapon we find that uses this unit type
                foreach (BaseWeapon w in weaponsWithUnitType)
                {
                    if (m_Player.SetWeapon.TryStart(WeaponHandler.Weapons.IndexOf(w) + 1))
                        return true;    // found matching weapon: stop looking
                }
            }

            return false;
        }
        protected virtual void TryWield(ItemInstance item)
        {
            if (!Application.isPlaying)
                return;

            if (Player.Dead.Active)
                return;

            if (!WeaponHandler.enabled)
                return;

            int index;
            ItemIdentifier identifier;
            for (index = 1; index < WeaponHandler.Weapons.Count + 1; index++)
            {

                identifier = GetWeaponIdentifier(WeaponHandler.Weapons[index - 1]);

                if (identifier == null)
                    continue;

                if (item.Type != identifier.Type)
                    continue;

                if (identifier.ID == 0)
                    goto found;

                if (item.ID != identifier.ID)
                    continue;

                goto found;

            }
            return;

        found:

            Player.SetWeapon.TryStart(index);
        }
        protected virtual void Unwield(ItemInstance item)
        {
            if (!Application.isPlaying)
                return;

            if (WeaponHandler.CurrentWeaponIndex == 0)
                return;

            if (CurrentWeaponIdentifier == null)
            {
                MissingIdentifierError();
                return;
            }

            if (item.Type != CurrentWeaponIdentifier.Type)
                return;

            if ((CurrentWeaponIdentifier.ID != 0) && (item.ID != CurrentWeaponIdentifier.ID))
                return;

            Player.SetWeapon.Start(0);

            fp_Timer.In(1.0f, UnwieldMissingWeapon);
        }
        public override void Refresh()
        {
            base.Refresh();

            UnwieldMissingWeapon();
        }
        protected virtual bool CanStart_SetWeapon()
        {
            int index = (int)Player.SetWeapon.Argument;
            if (index == 0)
                return true;

            if ((index < 1) || index > (WeaponHandler.Weapons.Count))
                return false;

            ItemIdentifier weaponIdentifier = GetWeaponIdentifier(WeaponHandler.Weapons[index - 1]);
            if (weaponIdentifier == null)
                return MissingIdentifierError(index);

            bool haveItem = HaveItem(weaponIdentifier.Type, weaponIdentifier.ID);

            // see if weapon is thrown
            if (haveItem && (BaseWeapon.Type)WeaponHandler.Weapons[index - 1].AnimationType == BaseWeapon.Type.Thrown)
            {
                if (GetAmmoInWeapon(WeaponHandler.Weapons[index - 1]) < 1)
                {
                    UnitBankType uType = weaponIdentifier.Type as UnitBankType;
                    if (uType == null)
                    {
                        Debug.LogError("Error (" + this + ") Tried to wield thrown weapon " + WeaponHandler.Weapons[index - 1] + " but its item identifier does not point to a UnitBank.");
                        return false;
                    }
                    else
                    {
                        if (!TryReload(uType, weaponIdentifier.ID)) 
                        {
                            return false;
                        }
                    }
                }
            }

            return haveItem;
        }
        protected virtual bool OnAttempt_DepleteAmmo()
        {
            if (CurrentWeaponIdentifier == null)
                return MissingIdentifierError();

            if (WeaponHandler.CurrentWeapon.AnimationType == (int)BaseWeapon.Type.Melee)
                return true;

            if (WeaponHandler.CurrentWeapon.AnimationType == (int)BaseWeapon.Type.Thrown)
                TryReload(CurrentWeaponInstance as UnitBankInstance);

            return TryDeduct(CurrentWeaponIdentifier.Type as UnitBankType, CurrentWeaponIdentifier.ID, 1);
        }
        protected virtual bool OnAttempt_RefillCurrentWeapon()
        {
            if (CurrentWeaponIdentifier == null)
                return MissingIdentifierError();

            return TryReload(CurrentWeaponIdentifier.Type as UnitBankType, CurrentWeaponIdentifier.ID);

        }
        public override void Reset()
        {
            m_PreviouslyOwnedItems.Clear();
            m_CurrentWeaponInstance = null;

            base.Reset();
        }
        protected virtual int OnValue_CurrentWeaponAmmoCount
        {
            get
            {
                UnitBankInstance weapon = CurrentWeaponInstance as UnitBankInstance;
                if (weapon == null)
                    return 0;
                return weapon.Count;
            }
            set
            {
                UnitBankInstance weapon = CurrentWeaponInstance as UnitBankInstance;
                if (weapon == null)
                    return;
                weapon.TryGiveUnits(value);
            }
        }
        protected virtual int OnValue_CurrentWeaponMaxAmmoCount
        {
            get
            {
                UnitBankInstance weapon = CurrentWeaponInstance as UnitBankInstance;
                if (weapon == null)
                    return 0;
                return weapon.Capacity;
            }
        }
        protected virtual int OnValue_CurrentWeaponClipCount
        {
            get
            {
                UnitBankInstance weapon = CurrentWeaponInstance as UnitBankInstance;
                if (weapon == null)
                    return 0;

                return GetUnitCount(weapon.UnitType);
            }
        }
        protected virtual int OnMessage_GetItemCount(string itemTypeObjectName)
        {
            ItemInstance item = GetItem(itemTypeObjectName);
            if (item == null)
                return 0;

            // if item is an internal unitbank, return its unit count
            UnitBankInstance unitBank = (item as UnitBankInstance);
            if ((unitBank != null) && (unitBank.IsInternal))
                return GetItemCount(unitBank.UnitType);

            // if it's a regular item or unitbank, return the amount
            // of similar instances
            return GetItemCount(item.Type);
        }
        protected virtual bool OnAttempt_AddItem(object args)
        {
            object[] arr = (object[])args;

            // fail if item type is unknown
            ItemType type = arr[0] as ItemType;
            if (type == null)
                return false;

            int amount = (arr.Length == 2) ? (int)arr[1] : 1;

            if (type is UnitType)
                return TryGiveUnits((type as UnitType), amount);

            return TryGiveItems(type, amount);
        }
        protected virtual bool OnAttempt_RemoveItem(object args)
        {
            object[] arr = (object[])args;

            // fail if item type is unknown
            ItemType type = arr[0] as ItemType;
            if (type == null)
                return false;

            int amount = (arr.Length == 2) ? (int)arr[1] : 1;

            if (type is UnitType)
                return TryRemoveUnits((type as UnitType), amount);

            return TryRemoveItems(type, amount);
        }
        protected virtual void OnStop_SetWeapon()
        {
            m_CurrentWeaponInstance = null;
        }
    }
}
