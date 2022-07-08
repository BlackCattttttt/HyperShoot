using UnityEngine;
using System.Collections.Generic;
using HyperShoot.Combat;

[RequireComponent(typeof(AudioSource))]

public class Explosion : MonoBehaviour
{
    // gameplay
    public float Radius = 15.0f;                    // any objects within radius will be affected by the explosion
    public float Force = 1000.0f;                   // amount of positional force to apply to affected objects
    public float UpForce = 10.0f;                   // how much to push affected objects up in the air
    public float Damage = 10;                       // amount of damage to apply to objects via their 'Damage' method
    public bool AllowCover = false;                 // if true, damage can only be done with line of sight between explosion center and target top or center
    public float CameraShake = 1.0f;                // how much of a shockwave impulse to apply to the camera
    protected bool m_HaveExploded = false;          // when true, the explosion is flagged for removal / recycling

    // sound
    public AudioClip Sound = null;
    public float SoundMinPitch = 0.8f;              // random pitch range for explosion sound
    public float SoundMaxPitch = 1.2f;

    // fx
    public List<GameObject> FXPrefabs = new List<GameObject>(); // list of special effects objects to spawn

    // physics
    protected Ray m_Ray;
    protected RaycastHit m_RaycastHit;
    protected Collider m_TargetCollider = null;
    protected Transform m_TargetTransform = null;
    protected Rigidbody m_TargetRigidbody = null;
    protected float m_DistanceModifier = 0.0f;

    // a dictionary to make sure we don't damage the same object several times in a frame
    protected Dictionary<Transform, object> m_RootTransformsHitByThisExplosion = new Dictionary<Transform, object>(50);

#if UNITY_EDITOR
    private bool m_DidWarnAboutBothMethodName = false;
#endif

    protected float DistanceModifier
    {
        get
        {
            if (m_DistanceModifier == 0.0f)
                m_DistanceModifier = (1 - Vector3.Distance(Transform.position, m_TargetTransform.position) / Radius);
            return m_DistanceModifier;
        }
    }

    protected Transform m_Transform = null;
    protected Transform Transform
    {
        get
        {
            if (m_Transform == null)
                m_Transform = transform;
            return m_Transform;
        }
    }

    protected Transform m_Source = null;
    protected Transform Source
    {
        get
        {
            if (m_Source == null)
                m_Source = transform;
            return m_Source;
        }
        set
        {
            m_Source = value;
        }
    }

    protected Transform m_OriginalSource = null;
    protected Transform OriginalSource
    {
        get
        {
            if (m_OriginalSource == null)
                m_OriginalSource = transform;
            return m_OriginalSource;
        }
        set
        {
            m_OriginalSource = value;
        }
    }

    protected AudioSource m_Audio = null;
    protected AudioSource Audio
    {
        get
        {
            if (m_Audio == null)
                m_Audio = GetComponent<AudioSource>();
            return m_Audio;
        }
    }

    protected virtual void OnEnable()
    {
        Source = transform;
        OriginalSource = null;
        fp_TargetEvent<Transform>.Register(transform, "SetSource", SetSource);
    }

    protected virtual void OnDisable()
    {
        Source = null;
        OriginalSource = null;
        fp_TargetEvent<Transform>.Unregister(transform, "SetSource", SetSource);
    }

    void Update()
    {
        if (m_HaveExploded)
        {
            if (!Audio.isPlaying)
            {
                m_HaveExploded = false;
                m_RootTransformsHitByThisExplosion.Clear();
                fp_Utility.Destroy(gameObject);
            }
            return;
        }
        DoExplode();
    }

    void DoExplode()
    {
        m_HaveExploded = true;
        foreach (GameObject fx in FXPrefabs)
        {
            if (fx != null)
            {
                fp_Utility.Instantiate(fx, Transform.position, Transform.rotation);
            }
        }

        Collider[] colliders = Physics.OverlapSphere(Transform.position, Radius, fp_Layer.Mask.IgnoreWalkThru);
        foreach (Collider hit in colliders)
        {
            if (hit.gameObject.isStatic)
                continue;

            m_DistanceModifier = 0.0f;

            if ((hit != null) && (hit.transform.root != transform.root))
            {
                m_TargetCollider = hit;
                m_TargetTransform = hit.transform;

                if (TargetInCover())
                    continue;

                m_TargetRigidbody = hit.GetComponent<Rigidbody>();
                if (m_TargetRigidbody != null)      // target has a rigidbody: apply force using Unity physics
                    AddRigidbodyForce();
                else                                // target has no rigidbody. try and apply force using UFPS physics
                    AddForce();
                TryDamage();
            }
        }

        // play explosion sound
        if (GameManager.Instance.Data.Sound)
        {
            Audio.clip = Sound;
            Audio.pitch = Random.Range(SoundMinPitch, SoundMaxPitch) * Time.timeScale;
            if (!Audio.playOnAwake)
                Audio.Play();
        }
    }

    protected virtual void TryDamage()
    {
        var damageble = m_TargetCollider.GetComponent<IDamageable>();
        if (damageble != null)
        {
            damageble.TakeDamage(new DamageData
            {
                Type = DamageType.Explosion,
                Damage = Damage,
                ImpactObject = gameObject
            });
        }
    }

    protected virtual bool TargetInCover()
    {
        m_Ray.origin = transform.position;  // center of explosion

        m_Ray.direction = (m_TargetCollider.bounds.center - transform.position).normalized;
        if (Physics.Raycast(m_Ray, out m_RaycastHit, Radius + 1.0f) && (m_RaycastHit.transform.root.GetComponent<Collider>() == m_TargetCollider))
            return false;   // target's center / waist exposed

        // --- top / head ---
        m_Ray.direction = ((fp_3DUtility.HorizontalVector(m_TargetCollider.bounds.center) + (Vector3.up * (m_TargetCollider.bounds.max.y))) - transform.position).normalized;
        if (Physics.Raycast(m_Ray, out m_RaycastHit, Radius + 1.0f) && (m_RaycastHit.transform.root.GetComponent<Collider>() == m_TargetCollider))
            return false;   // target's top / head exposed

        return true;
    }

    protected virtual void AddRigidbodyForce()
    {
        if (m_TargetRigidbody.isKinematic)
            return;

        m_Ray.origin = m_TargetTransform.position;
        m_Ray.direction = -Vector3.up;
        if (!Physics.Raycast(m_Ray, out m_RaycastHit, 1))
            UpForce = 0.0f;

        // bash the found object
        m_TargetRigidbody.AddExplosionForce((Force / Time.timeScale) / fp_TimeUtility.AdjustedTimeScale, Transform.position, Radius, UpForce);
    }

    protected virtual void AddForce()
    {
        fp_TargetEvent<Vector3>.Send(m_TargetTransform.root, "ForceImpact", (m_TargetTransform.position -
                                                                                Transform.position).normalized *
                                                                                Force * 0.001f * DistanceModifier);
    }

    public void SetSource(Transform source)
    {
        m_OriginalSource = source;      // who set off this explosion
    }
}

