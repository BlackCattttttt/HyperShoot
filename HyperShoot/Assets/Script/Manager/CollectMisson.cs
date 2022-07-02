using HyperShoot.Enemy;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace HyperShoot.Manager
{
    public class CollectMisson : BaseMisson
    {
        [SerializeField] private int numberOfCollect;
        [SerializeField] private int rate;
        [SerializeField] private  List<EnemyType> enemyTypes;

        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private int currentCollect;

        public int NumberOfCollect { get => numberOfCollect; set => numberOfCollect = value; }
        public int CurrentCollect { get => currentCollect; set => currentCollect = value; }

        private void Start()
        {
            currentCollect = 0;
            MessageBroker.Default.Receive<BaseMessage.EnemyDieMessage>()
               .Subscribe(Collect)
               .AddTo(_disposables);
        }
        public void Collect(BaseMessage.EnemyDieMessage message)
        {
            for (int i = 0; i < enemyTypes.Count; i++)
            {
                if (message.enemyType == enemyTypes[i])
                {
                    if (Random.Range(0,100) < rate)
                    {
                        currentCollect++;
                        PlayScreen.Instance.Updatecount(currentCollect, numberOfCollect);
                        if (currentCollect >= numberOfCollect)
                        {
                            MessageBroker.Default.Publish(new BaseMessage.MissonComplete
                            {

                            });
                            SimplePool.Despawn(gameObject);
                        }
                    }
                }
            }
        }
        private void OnDisable()
        {
            _disposables.Dispose();
        }
    }
}
