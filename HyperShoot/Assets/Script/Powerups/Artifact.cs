using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class Artifact : MonoBehaviour
{
    [SerializeField] private ParticleSystem particleSystem;

    private bool _isActive;

    private readonly CompositeDisposable _disposables = new CompositeDisposable();
    private void Start()
    {
        _isActive = false;
        particleSystem.gameObject.SetActive(false);
        MessageBroker.Default.Receive<BaseMessage.ActiveArtifact>()
                .Subscribe(SetActive)
                .AddTo(_disposables);
    }
    public void SetActive(BaseMessage.ActiveArtifact message)
    {
        _isActive = true;
        particleSystem.gameObject.SetActive(true);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (_isActive && other.CompareTag("Player"))
        {
            MessageBroker.Default.Publish(new BaseMessage.FindMessage
            {
                
            });
            Destroy(gameObject);
        }
    }
    private void OnDisable()
    {
        _disposables.Dispose();
    }
}
