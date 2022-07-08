using HyperShoot.Combat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperShoot.Bullet
{
    public class BaseBullet : MonoBehaviour
    {
        // gameplay
        public float Range = 100.0f;                // max travel distance of this type of bullet in meters
        public float Force = 100.0f;                // force applied to any rigidbody hit by the bullet
        public float Damage = 1.0f;                 // the damage transmitted to target by the bullet
                                                  
        // components
        protected Transform m_Transform = null;
        protected Renderer m_Renderer = null;
        protected AudioSource m_Audio = null;

        // internal state
        protected bool m_Initialized = false;
        protected Transform m_Source = null;                        // inflictor / source of the damage
                                                                    //protected static fp_DamageHandler m_TargetDHandler = null;

        // raycasting
        protected Ray m_Ray;
        protected RaycastHit m_Hit;
        protected int LayerMask = fp_Layer.Mask.IgnoreWalkThru;

#if UNITY_EDITOR
        private bool m_DidWarnAboutBothMethodName = false;
#endif

        protected virtual void Awake()
        {
            m_Transform = transform;
            m_Renderer = GetComponent<Renderer>();
            m_Audio = GetComponent<AudioSource>();
        }

        protected virtual void Start()
        {
            m_Initialized = true;

            StartCoroutine(TryHitOnEndOfFrame());
        }

        protected virtual void OnEnable()
        {
            if (!m_Initialized)
                return;

            StartCoroutine(TryHitOnEndOfFrame());
        }

        protected virtual bool TryHit()
        {
            m_Ray = new Ray(m_Transform.position, m_Transform.forward);

            // if this bullet was fired by the local player: don't allow it to hit the local player!
            if ((m_Source != null) && (m_Source.gameObject.layer == fp_Layer.LocalPlayer))
                LayerMask = fp_Layer.Mask.BulletBlockers;
            else
                LayerMask = fp_Layer.Mask.IgnoreWalkThru;

            if (!Physics.Raycast(m_Ray, out m_Hit, Range, LayerMask))
                return false;

            DoHit();

            return true;
        }

        protected virtual void DoHit()
        {
            // spawn particle effects and decals
            TrySpawnFX();

            // play sound if we have an audio source + clip
            TryPlaySound();

            // if hit object has physics, add the bullet force to it
            TryAddForce();

            // try to make damage in the best supported way
            TryDamage();

            // remove the bullet - as long as it's invisible and silent
            TryDestroy();
        }

        protected virtual void TrySpawnFX()
        {
            m_Transform.position = m_Hit.point;

            // adopt the normal of the surface hit
            m_Transform.rotation = Quaternion.LookRotation(m_Hit.normal);
        }

        protected virtual void TryPlaySound()
        {
            if (m_Audio == null)
                return;

            if (m_Audio.clip == null)
                return;
            if (!GameManager.Instance.Data.Sound)
                return;

            m_Audio.pitch = Time.timeScale;
            m_Audio.Stop();
            m_Audio.Play();
        }

        protected virtual void TryAddForce()
        {
            Rigidbody body = m_Hit.collider.attachedRigidbody;

            if (body == null)
                return;

            if (body.isKinematic)
                return;

            body.AddForceAtPosition(((m_Ray.direction * Force) / Time.timeScale) / fp_TimeUtility.AdjustedTimeScale, m_Hit.point);
        }
        protected virtual void TryDamage()
        {
            if (m_Hit.collider.CompareTag("Player"))
            {
                var damageable = m_Hit.collider.GetComponentInParent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(new DamageData
                    {
                        Type = DamageType.Bullet,
                        Damage = Damage,
                        ImpactObject = gameObject
                    });
                }
            }
            else
            {
                var damageable = m_Hit.collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(new DamageData
                    {
                        Type = DamageType.Bullet,
                        Damage = Damage,
                        ImpactObject = gameObject
                    });
                }
            }        
        }
        protected virtual void TryDestroy()
        {
            if (this == null)
                return;

            if ((m_Renderer != null) && m_Renderer.enabled)
                return;

            if ((m_Audio != null) && (m_Audio.isPlaying))
            {
                fp_Timer.In(1, TryDestroy);
                return;
            }
            // restore the renderer for pooling (recycling)
            if (m_Renderer != null)
                m_Renderer.enabled = true;

            fp_Utility.Destroy(gameObject);
        }

        protected IEnumerator TryHitOnEndOfFrame()
        {
            yield return new WaitForEndOfFrame();
            if (!TryHit())
                StartCoroutine(DestroyOnNextFrame());
        }

        protected IEnumerator DestroyOnNextFrame()
        {
            yield return 0;
            fp_Utility.Destroy(gameObject);
        }

        public virtual void SetSource(Transform source)
        {
            m_Source = source;
        }
    }
}