using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace HyperShoot.Manager
{
    public class MissonSpawn : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                MessageBroker.Default.Publish(new BaseMessage.SpawnMissonMessage
                {

                });
                gameObject.SetActive(false);
            }
        }
    }
}
