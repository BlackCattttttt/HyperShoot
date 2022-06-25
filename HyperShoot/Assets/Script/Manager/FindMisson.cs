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
            Debug.Log("xoai");
        }
        private void OnDisable()
        {
            _disposables.Dispose();
        }
    }
}
