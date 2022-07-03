using HyperShoot.Combat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperShoot.Player
{
    public class InflictDamageOnUnderwater : MonoBehaviour
    {
        [SerializeField] private float damageInterval = 0.5f; // inflict damage discreted instead of continuous
        [SerializeField] private float damagePerSecond = 10;
        private float timeStartInterval = 0f;

        [SerializeField] private Transform playerCamera = null;
        [SerializeField] private Transform waterPlane = null;

        [SerializeField] private FPPlayerDamagerHander playerHealth = null;

        private bool isUnderWater = false;
        public bool IsUnderWater { get => isUnderWater; }
        public event System.Action enteredWater = null;
        public event System.Action leavedWater = null;

        private void Start()
        {
            isUnderWater = playerCamera.transform.position.y < waterPlane.position.y;
            timeStartInterval = Time.time;
        }
        private void Update()
        {
            bool isUnderWaterPrev = isUnderWater;
            isUnderWater = playerCamera.transform.position.y < waterPlane.position.y;
            if (isUnderWaterPrev && !isUnderWater)
            {
                leavedWater?.Invoke();
            }
            else if (!isUnderWaterPrev && isUnderWater)
            {
                enteredWater?.Invoke();
            }

            if (isUnderWater)
            {
                if (Time.time > timeStartInterval + damageInterval)
                {
                    playerHealth.TakeDamage(new DamageData { 
                        Type = DamageType.Unknown,
                        Damage = damagePerSecond * damageInterval
                    });
                    timeStartInterval = Time.time;
                }
            }
        }
    }
}