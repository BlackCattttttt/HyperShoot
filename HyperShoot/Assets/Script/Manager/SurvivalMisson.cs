using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace HyperShoot.Manager
{
    public class SurvivalMisson : BaseMisson
    {
        [SerializeField] private float timeSurvival;
        [SerializeField] private IsolatedSpace isolatedSpace;

        private float _timeSurvival;
        private bool _compelete;
        private bool _isActive = false;
        private IsolatedSpace _space;

        public float TimeSurvival { get => timeSurvival; set => timeSurvival = value; }

        private void Start()
        {
            _timeSurvival = timeSurvival;
            _compelete = false;
        }

        public void OnActive(Transform spawn)
        {
            _isActive = true;
            _space = SimplePool.Spawn(isolatedSpace, spawn.position, Quaternion.identity);
        }
        private void FixedUpdate()
        {
            if (_isActive)
            {
                _timeSurvival -= Time.fixedDeltaTime;
                if (_timeSurvival < 0 && !_compelete)
                {
                    _compelete = true;
                    SimplePool.Despawn(_space.gameObject);
                    MessageBroker.Default.Publish(new BaseMessage.MissonComplete
                    {
                        
                    });
                    SimplePool.Despawn(gameObject);
                }
            }
        }
    }
}
