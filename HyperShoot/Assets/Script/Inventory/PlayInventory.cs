using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HyperShoot.Player;
using HyperShoot.Weapon;

namespace HyperShoot.Inventory
{
    public class PlayInventory : fp_Component
    {
        public int maxAmo = 10;
        public int currentAmo;
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
        protected override void Start()
        {
            base.Start();
            currentAmo = maxAmo;
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
        protected virtual bool OnAttempt_DepleteAmmo()
        {
            currentAmo--;
            if (currentAmo <= 0)
                return false;
            else
                return true;

        }
        protected virtual bool OnAttempt_RefillCurrentWeapon()
        {
            currentAmo = maxAmo;
            return true;

        }
        protected virtual int OnValue_CurrentWeaponAmmoCount
        {
            get
            {
                return currentAmo;
            }
            set
            {
                
            }
        }
        protected virtual int OnValue_CurrentWeaponMaxAmmoCount
        {
            get
            {            
                return maxAmo;
            }
        }
    }
}
