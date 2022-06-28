using HyperShoot.Enemy;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace HyperShoot.Manager
{
    public class KillEnemyMisson : BaseMisson
    {
        [SerializeField] private int numberOfSkill;
        [SerializeField] private EnemyType enemyType;

        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private int currentSkill;

        private void Start()
        {
            currentSkill = 0;
            MessageBroker.Default.Receive<BaseMessage.EnemyDieMessage>()
               .Subscribe(CountEnemy)
               .AddTo(_disposables);
        }
        public void CountEnemy(BaseMessage.EnemyDieMessage message)
        {
            if (message.enemyType == enemyType)
            {
                currentSkill++;
                if (currentSkill >= numberOfSkill)
                {
                    MessageBroker.Default.Publish(new BaseMessage.MissonComplete
                    {
                        
                    });
                    SimplePool.Despawn(gameObject);
                }
            }
        }
        private void OnDisable()
        {
            _disposables.Dispose();
        }
    }
}
