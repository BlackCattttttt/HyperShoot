using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace HyperShoot.Manager
{
    public class FindMisson : BaseMisson
    {
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private void Start()
        {
            MessageBroker.Default.Receive<BaseMessage.FindMessage>()
               .Subscribe(FindArtifact)
               .AddTo(_disposables);
        }
        public void FindArtifact(BaseMessage.FindMessage message)
        {
            MessageBroker.Default.Publish(new BaseMessage.MissonComplete
            {

            });
            PlayScreen.Instance.Updatecount(1, 1);
            SimplePool.Despawn(gameObject);
        }
        private void OnDisable()
        {
            _disposables.Dispose();
        }
    }
}
