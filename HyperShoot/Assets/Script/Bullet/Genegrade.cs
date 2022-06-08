using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperShoot.Bullet
{
    public class Genegrade : MonoBehaviour
    {
        public float LifeTime = 3.0f;
        public float RigidbodyForce = 10.0f;
        public float RigidbodySpin = 0.0f;
        public Explosion explosion;

        protected Rigidbody m_Rigidbody = null;
        protected Transform m_Source = null;                // immediate cause of the damage
        protected Transform m_OriginalSource = null;        // initial cause of the damage#

        protected virtual void Awake()
        {
            m_Rigidbody = GetComponent<Rigidbody>();
        }
        protected virtual void OnEnable()
        {
            if (m_Rigidbody == null)
                return;

            fp_Timer.In(LifeTime, () =>
            {
                SimplePool.Spawn(explosion.gameObject, transform.position, Quaternion.identity);
                Destroy(gameObject);
            });

            // apply force on spawn
            if (RigidbodyForce != 0.0f)
                m_Rigidbody.AddForce((transform.forward * RigidbodyForce), ForceMode.Impulse);
            if (RigidbodySpin != 0.0f)
                m_Rigidbody.AddTorque(Random.rotation.eulerAngles * RigidbodySpin);

        }
    }

}
