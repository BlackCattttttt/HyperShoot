using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace HyperShoot.Weapon
{
    public class FPWeaponThrower : WeaponThrower
    {
        public Vector3 FirePositionOffset = new Vector3(0.35f, 0.0f, 0.0f);

        protected fp_Timer.Handle m_Timer1 = new fp_Timer.Handle();
        protected fp_Timer.Handle m_Timer2 = new fp_Timer.Handle();
        protected fp_Timer.Handle m_Timer3 = new fp_Timer.Handle();
        protected fp_Timer.Handle m_Timer4 = new fp_Timer.Handle();

        protected Transform m_FirePosition;
        protected Transform FirePosition
        {
            get
            {
                if (m_FirePosition == null)
                {
                    GameObject g = new GameObject("ThrownWeaponFirePosition");
                    m_FirePosition = g.transform;
                    m_FirePosition.parent = Camera.main.transform;
                }
                return m_FirePosition;
            }
        }

        public Animation WeaponAnimation
        {
            get
            {
                if (m_WeaponAnimation == null)
                {
                    if (FPWeapon == null)
                        return null;
                    if (FPWeapon.WeaponModel == null)
                        return null;
                    m_WeaponAnimation = FPWeapon.WeaponModel.GetComponent<Animation>();
                }
                return m_WeaponAnimation;
            }
        }
        Animation m_WeaponAnimation = null;

        protected override void Start()
        {
            base.Start();
        }

        protected virtual void RewindAnimation()
        {
            if (!Player.IsFirstPerson.Get())
                return;

            if (FPWeapon == null)
                return;

            if (FPWeapon.WeaponModel == null)
                return;

            if (WeaponAnimation == null)
                return;

            if (FPWeaponShooter == null)
                return;

            if (FPWeaponShooter.AnimationFire == null)
                return;

            WeaponAnimation[FPWeaponShooter.AnimationFire.name].time = 0.0f;

            WeaponAnimation.Play();
            WeaponAnimation.Sample();
            WeaponAnimation.Stop();
        }

        protected override void OnStart_Attack()
        {
            base.OnStart_Attack();

            // set spawn position for the projectile
            if (Player.IsFirstPerson.Get())
            {
                Shooter.m_ProjectileSpawnPoint = FirePosition.gameObject;
                FirePosition.localPosition = FirePositionOffset;
                FirePosition.localEulerAngles = Vector3.zero;
            }

            fp_Timer.In(Shooter.ProjectileSpawnDelay, delegate ()
            {
                if (!HaveAmmoForCurrentWeapon)
                {
                    FPWeapon.SetState("ReWield");
                    FPWeapon.Refresh();
                }
                else
                {
                    if (Player.IsFirstPerson.Get())
                    {
                        FPWeapon.SetState("ReWield");
                        FPWeapon.Refresh();

                        fp_Timer.In(1.0f, delegate ()
                            {
                                RewindAnimation();
                                FPWeapon.Rendering = true;
                                FPWeapon.SetState("ReWield", false);
                                FPWeapon.Refresh();
                            }, m_Timer3);
                    }
                    else
                    {
                        fp_Timer.In(0.5f, delegate ()
                            {
                                Player.Attack.Stop();
                            }, m_Timer4);
                    }

                }
            }, m_Timer1);
        }

        protected virtual void OnStart_SetWeapon()
        {
            RewindAnimation();
        }

        protected override void OnStop_Attack()
        {
            base.OnStop_Attack();
        }
    }
}