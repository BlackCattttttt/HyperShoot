using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class Artifact : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            MessageBroker.Default.Publish(new BaseMessage.FindMessage
            {
                
            });
            Destroy(gameObject);
        }
    }
}
