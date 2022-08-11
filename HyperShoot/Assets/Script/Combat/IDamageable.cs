using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperShoot.Combat
{
    public interface IDamageable
    {
        IObservable<DamageData> TakeDamageObservable { get; }
        void TakeDamage(DamageData data);
    }
    public enum DamageType
    {
        Unknown,
        Fall,
        Impact,
        Bullet,
        Explosion
    }
    public struct DamageData
    {
        public DamageType Type;
        public float Damage;
        public GameObject ImpactObject;
    }
}