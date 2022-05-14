using HyperShoot.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperShoot.Weapon
{
    public class WeaponHandler : MonoBehaviour
    {
        public int StartWeapon = 0;

        // weapon timing
        public float AttackStateDisableDelay = 0.5f;        // delay until weapon attack state is disabled after firing ends
        public float SetWeaponRefreshStatesDelay = 0.5f;    // delay until component states are refreshed after setting a new weapon
        public float SetWeaponDuration = 0.1f;              // amount of time between previous weapon disappearing and next weapon appearing

        // forced pauses in player activity
        public float SetWeaponReloadSleepDuration = 0.3f;   // amount of time to prohibit reloading during set weapon
        public float SetWeaponZoomSleepDuration = 0.3f;     // amount of time to prohibit zooming during set weapon
        public float SetWeaponAttackSleepDuration = 0.3f;   // amount of time to prohibit attacking during set weapon
        public float ReloadAttackSleepDuration = 0.3f;      // amount of time to prohibit attacking during reloading

        protected fp_Timer.Handle m_SetWeaponRefreshTimer = new fp_Timer.Handle();

        protected CharacterEventHandler m_Player = null;
        protected List<BaseWeapon> m_Weapons = null;// = new List<BaseWeapon>();
        public List<BaseWeapon> Weapons
        {
            get
            {
                if (m_Weapons == null)
                    InitWeaponLists();
                return m_Weapons;
            }
            set
            {
                m_Weapons = value;
            }
        }

        protected List<List<BaseWeapon>> m_WeaponLists = new List<List<BaseWeapon>>();

        protected int m_CurrentWeaponIndex = -1;
        public int CurrentWeaponIndex { get { return m_CurrentWeaponIndex; } }
        protected BaseWeapon m_CurrentWeapon = null;
        public BaseWeapon CurrentWeapon { get { return m_CurrentWeapon; } }
        protected BaseShooter m_CurrentShooter = null;
        public BaseShooter CurrentShooter
        {
            get
            {
                if (CurrentWeapon == null)
                    return null;

                if ((m_CurrentShooter != null) && ((!m_CurrentShooter.enabled) || (!fp_Utility.IsActive(m_CurrentShooter.gameObject))))
                    return null;

                return m_CurrentShooter;    // NOTE: this is set in 'ActivateWeapon'
            }
        }
        protected class WeaponComparer : IComparer
        {
            int IComparer.Compare(System.Object x, System.Object y)
            { return ((new CaseInsensitiveComparer()).Compare(((BaseWeapon)x).gameObject.name, ((BaseWeapon)y).gameObject.name)); }
        }
        public BaseWeapon WeaponBeingSet
        {
            get
            {
                if (!m_Player.SetWeapon.Active)
                    return null;

                if (m_Player.SetWeapon.Argument == null)
                    return null;

                return Weapons[Mathf.Max(0, (int)m_Player.SetWeapon.Argument - 1)];
            }
        }
        protected virtual void Awake()
        {
            // store the first player event handler found in the top of our transform hierarchy
            m_Player = (CharacterEventHandler)transform.root.GetComponentInChildren(typeof(CharacterEventHandler));

            if (Weapons != null)
                StartWeapon = Mathf.Clamp(StartWeapon, 0, Weapons.Count);
        }
        protected void InitWeaponLists()
        {
            // first off, always store all weapons contained under our main FPS camera (if any)
            List<BaseWeapon> camWeapons = null;
            FPCamera camera = transform.GetComponentInChildren<FPCamera>();
            if (camera != null)
            {
                camWeapons = GetWeaponList(camera.transform);
                if ((camWeapons != null) && (camWeapons.Count > 0))
                    m_WeaponLists.Add(camWeapons);

            }

            List<BaseWeapon> allWeapons = new List<BaseWeapon>(transform.GetComponentsInChildren<BaseWeapon>());

            // if the camera weapons were all the weapons we have, return
            if ((camWeapons != null) && (camWeapons.Count == allWeapons.Count))
            {
                Weapons = m_WeaponLists[0];
            }
        }
        public void EnableWeaponList(int index)
        {
            if (m_WeaponLists == null)
                return;

            if (m_WeaponLists.Count < 1)
                return;

            if ((index < 0) || (index > (m_WeaponLists.Count - 1)))
                return;

            Weapons = m_WeaponLists[index];
        }
        protected List<BaseWeapon> GetWeaponList(Transform target)
        {
            List<BaseWeapon> weapons = new List<BaseWeapon>();

            if (target.GetComponent<BaseWeapon>())
            {
                return weapons;
            }

            // add the gameobjects of any weapon components to the weapon list
            foreach (BaseWeapon w in target.GetComponentsInChildren<BaseWeapon>(true))
            {
                weapons.Insert(weapons.Count, w);
            }

            if (weapons.Count == 0)
            {
                return weapons;
            }

            // sort the weapons alphabetically
            IComparer comparer = new WeaponComparer();
            weapons.Sort(comparer.Compare);

            return weapons;
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
        protected virtual void Update()
        {
            InitWeapon();
            UpdateFiring();
        }
        public void InitWeapon()
        {

            if (m_CurrentWeaponIndex == -1)
            {
                SetWeapon(0);
                // set start weapon (if specified, and if inventory allows it)
                fp_Timer.In(SetWeaponDuration + 0.1f, delegate ()
                {
                    if (StartWeapon > 0 && (StartWeapon < (Weapons.Count + 1)))
                    {
                        if (!m_Player.SetWeapon.TryStart(StartWeapon))
                            return;
                    }
                });

            }
        }
        public virtual void SetWeapon(int weaponIndex)
        {
            if ((Weapons == null) || (Weapons.Count < 1))
            {
                return;
            }

            if (weaponIndex < 0 || weaponIndex > Weapons.Count)
            {
                return;
            }

            // before putting old weapon away, make sure it's in a neutral
            // state next time it is activated
            if (m_CurrentWeapon != null)
                m_CurrentWeapon.ResetState();

            // deactivate all weapons
            DeactivateAll(Weapons);

            // activate the new weapon
            ActivateWeapon(weaponIndex);
        }
        public void DeactivateAll(List<BaseWeapon> weaponList)
        {
            foreach (BaseWeapon weapon in weaponList)
            {
                weapon.ActivateGameObject(false);
            }

            m_CurrentShooter = null;
        }
        public void ActivateWeapon(int index)
        {
            m_CurrentWeaponIndex = index;
            m_CurrentWeapon = null;
            if (m_CurrentWeaponIndex > 0)
            {
                m_CurrentWeapon = Weapons[m_CurrentWeaponIndex - 1];
                if (m_CurrentWeapon != null)
                    m_CurrentWeapon.ActivateGameObject(true);
            }

             if (m_CurrentWeapon != null)
                  m_CurrentShooter = CurrentWeapon.GetComponent<BaseShooter>();

        }
        protected virtual void UpdateFiring()
        {
            //if (!m_Player.IsLocal.Get() && !m_Player.IsAI.Get())
            //    return;
            // we continuously try to fire the weapon while player is in attack
            // mode, but if it's not: bail out
            if (!m_Player.Attack.Active)
                return;

            // weapon can only be fired if fully wielded
            if (m_Player.SetWeapon.Active || ((m_CurrentWeapon != null) && !m_CurrentWeapon.Wielded))
                return;

            m_Player.Fire.Try();
        }
        protected virtual bool CanStart_SetWeapon()
        {
            // fetch weapon index from when 'SetWeapon.TryStart' was called
            int weapon = (int)m_Player.SetWeapon.Argument;

            // can't set a weapon that is already set
            if (weapon == m_CurrentWeaponIndex)
                return false;

            // can't set an unexisting weapon
            if (weapon < 0 || weapon > Weapons.Count)
                return false;

            // can't set a new weapon while reloading
           // if (m_Player.Reload.Active)
           //     return false;

            return true;
        }
        protected virtual void OnStart_SetWeapon()
        {
            // prevent these player activities during the weapon switch (unless switching to a melee weapon)
            if ((WeaponBeingSet == null))
            //|| (WeaponBeingSet.AnimationType != (int)BaseWeapon.Type.Melee))
            {
                // m_Player.Reload.Stop(SetWeaponDuration + SetWeaponReloadSleepDuration);
                m_Player.Zoom.Stop(SetWeaponDuration + SetWeaponZoomSleepDuration);
                m_Player.Attack.Stop(SetWeaponDuration + SetWeaponAttackSleepDuration);
            }

            if (m_CurrentWeapon != null)
                m_CurrentWeapon.Wield(false);

            m_Player.SetWeapon.AutoDuration = SetWeaponDuration;
        }
        protected virtual void OnStop_SetWeapon()
        {
            // fetch weapon index from when 'SetWeapon.TryStart' was called
            int weapon = 0;
            if (m_Player.SetWeapon.Argument != null)
                weapon = (int)m_Player.SetWeapon.Argument;

            // hides the old weapon and activates the new one (at its exit offset)
            SetWeapon(weapon);

            // smoothly moves the new weapon into view and plays a wield sound
            if (m_CurrentWeapon != null)
                m_CurrentWeapon.Wield();

            fp_Timer.In(SetWeaponRefreshStatesDelay, delegate ()
            {
                if ((this != null) && (m_Player != null))
                {
                    m_Player.RefreshActivityStates();

                    if (m_CurrentWeapon != null)
                    {
                        //  if (m_Player.CurrentWeaponAmmoCount.Get() == 0)
                        //  {
                        //     m_Player.AutoReload.Try();  // try to auto-reload
                        //  }
                    }
                }
            }, m_SetWeaponRefreshTimer);
        }
        protected virtual int OnValue_CurrentWeaponIndex
        {
            get
            {
                return m_CurrentWeaponIndex;
            }
        }
        public virtual void OnMessage_Unwield()
        {
            m_Player.SetWeapon.TryStart(0);
        }
    }
}
