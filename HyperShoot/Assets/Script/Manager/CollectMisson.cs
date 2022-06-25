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
                        if (currentCollect >= numberOfCollect)
                        {
                            Debug.Log("xoai");
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
