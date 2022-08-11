using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperShoot.Weapon
{
    public class BaseShooter : fp_Component
    {
        protected CharacterController m_CharacterController = null;

        public GameObject m_ProjectileSpawnPoint = null;
        public GameObject ProjectileSpawnPoint
        {
            get
            {
                return m_ProjectileSpawnPoint;
            }
        }

        protected GameObject m_ProjectileDefaultSpawnpoint = null;

        // projectile
        public GameObject ProjectilePrefab = null;          // prefab with a mesh and projectile script
        public float ProjectileScale = 1.0f;                // scale of the projectile decal
        public float ProjectileFiringRate = 0.3f;           // delay between shots fired when fire button is held down
        public float ProjectileSpawnDelay = 0.0f;           // delay between fire button pressed and projectile launched
        public int ProjectileCount = 1;                     // amount of projectiles to fire at once
        public float ProjectileSpread = 0.0f;               // accuracy deviation in degrees (0 = spot on)
        public bool ProjectileSourceIsRoot = true;          // whether to report this projectile as being sent from this transform or from its root
        public string FireMessage = "";                     // OPTIONAL: if this is set, a regular Unity message will be sent to the root gameobject every time the shooter fires
        protected float m_NextAllowedFireTime = 0.0f;       // the next time firing will be allowed after having recently fired a shot

        // muzzle flash
        public Vector3 MuzzleFlashPosition = Vector3.zero;  // position of the muzzle in relation to the parent
        public Vector3 MuzzleFlashScale = Vector3.one;      // scale of the muzzleflash
        public float MuzzleFlashFadeSpeed = 0.075f;         // the amount of muzzle flash alpha to deduct each frame
        public GameObject MuzzleFlashPrefab = null;         // muzzleflash prefab, typically with a mesh and vp_MuzzleFlash script
        public float MuzzleFlashDelay = 0.0f;               // delay between fire button pressed and muzzleflash appearing
        protected GameObject m_MuzzleFlash = null;          // the instantiated muzzle flash. one per weapon that's always there

        public Transform m_MuzzleFlashSpawnPoint = null;

        public GameObject MuzzleFlash
        {
            get
            {
                // instantiate muzzleflash
                if ((m_MuzzleFlash == null) && (MuzzleFlashPrefab != null) && (ProjectileSpawnPoint != null))
                {
                    m_MuzzleFlash = (GameObject)fp_Utility.Instantiate(MuzzleFlashPrefab,
                                                                    ProjectileSpawnPoint.transform.position,
                                                                    ProjectileSpawnPoint.transform.rotation);
                    m_MuzzleFlash.name = transform.name + "MuzzleFlash";
                    m_MuzzleFlash.transform.parent = ProjectileSpawnPoint.transform;
                }
                return m_MuzzleFlash;
            }
        }

        public delegate Vector3 FirePositionFunc();
        public FirePositionFunc GetFirePosition = null;
        public delegate Quaternion FireRotationFunc();
        public FireRotationFunc GetFireRotation = null;
        public delegate int FireSeedFunc();
        public FireSeedFunc GetFireSeed = null;

        // work variables for the current shot being fired
        protected Vector3 m_CurrentFirePosition = Vector3.zero;             // spawn position
        protected Quaternion m_CurrentFireRotation = Quaternion.identity;   // spawn rotation
        protected int m_CurrentFireSeed;                                    // unique number used to generate a random spread for every projectile

        public Vector3 FirePosition = Vector3.zero;

        // sound
        public AudioClip SoundFire = null;                          // sound to play upon firing
        public float SoundFireDelay = 0.0f;                         // delay between fire button pressed and fire sound played
        public Vector2 SoundFirePitch = new Vector2(1.0f, 1.0f);    // random pitch range for firing sound

        protected override void Awake()
        {
            base.Awake();

            if (m_ProjectileSpawnPoint == null)
                m_ProjectileSpawnPoint = gameObject;    // NOTE: may also be set by derived classes

            m_ProjectileDefaultSpawnpoint = m_ProjectileSpawnPoint;

            // if firing delegates haven't been set by a derived or external class yet, set them now
            if (GetFirePosition == null) GetFirePosition = delegate () { return FirePosition; };
            if (GetFireRotation == null) GetFireRotation = delegate () { return m_ProjectileSpawnPoint.transform.rotation; };
            if (GetFireSeed == null) GetFireSeed = delegate () { return Random.Range(0, 100); };

            m_CharacterController = m_ProjectileSpawnPoint.transform.root.GetComponentInChildren<CharacterController>();

            // reset the next allowed fire time
            m_NextAllowedFireTime = Time.time;
            ProjectileSpawnDelay = Mathf.Min(ProjectileSpawnDelay, (ProjectileFiringRate - 0.1f));
        }
        protected override void Start()
        {
            base.Start();

            // audio defaults
            Audio.playOnAwake = false;
            Audio.dopplerLevel = 0.0f;

            RefreshDefaultState();

            Refresh();
        }
        protected override void LateUpdate()
        {
            FirePosition = m_ProjectileSpawnPoint.transform.position;
        }
        public virtual bool TryFire()
        {
            // return if we can't fire yet
            if (Time.time < m_NextAllowedFireTime)
                return false;

            Fire();

            return true;
        }
        protected virtual void Fire()
        {
            // update firing rate
            m_NextAllowedFireTime = Time.time + ProjectileFiringRate;

            //play fire sound
            if (SoundFireDelay == 0.0f)
                PlayFireSound();
            else
                fp_Timer.In(SoundFireDelay, PlayFireSound);

            // spawn projectiles
            if (ProjectileSpawnDelay == 0.0f)
                SpawnProjectiles();
            else
                fp_Timer.In(ProjectileSpawnDelay, delegate () { SpawnProjectiles(); });

            // show muzzle flash
            if (MuzzleFlashDelay == 0.0f)
                ShowMuzzleFlash();
            else
                fp_Timer.In(MuzzleFlashDelay, ShowMuzzleFlash);
        }
        protected virtual void SpawnProjectiles()
        {

            if (ProjectilePrefab == null)
                return;

            m_CurrentFirePosition = GetFirePosition();
            m_CurrentFireRotation = GetFireRotation();
            m_CurrentFireSeed = GetFireSeed();

            for (int v = 0; v < ProjectileCount; v++)
            {

                GameObject p = null;

                p = (GameObject)fp_Utility.Instantiate(ProjectilePrefab, m_CurrentFirePosition, m_CurrentFireRotation);

                p.SendMessage("SetSource", (ProjectileSourceIsRoot ? Root : Transform), SendMessageOptions.DontRequireReceiver);
                p.transform.localScale = new Vector3(ProjectileScale, ProjectileScale, ProjectileScale);    // preset defined scale

                SetSpread(m_CurrentFireSeed * (v + 1), p.transform);
            }
        }
        protected virtual void PlayFireSound()
        {
            if (!GameManager.Instance.Data.Sound)
                return;
            if (Audio == null)
                return;

            Audio.pitch = Random.Range(SoundFirePitch.x, SoundFirePitch.y) * Time.timeScale;
            Audio.clip = SoundFire;
            Audio.Play();
        }
        public void SetSpread(int seed, Transform target)
        {
            fp_MathUtility.SetSeed(seed);

            target.Rotate(0, 0, Random.Range(0, 360));                                  // first, rotate up to 360 degrees around z for circular spread
            target.Rotate(0, Random.Range(-ProjectileSpread, ProjectileSpread), 0);     // then rotate around y with user defined deviation
        }
        protected virtual void ShowMuzzleFlash()
        {
            if (MuzzleFlash == null)
                return;

            if (m_MuzzleFlashSpawnPoint != null && ProjectileSpawnPoint != null)
            {
                MuzzleFlash.transform.position = m_MuzzleFlashSpawnPoint.transform.position;
                MuzzleFlash.transform.rotation = m_MuzzleFlashSpawnPoint.transform.rotation;
            }

            MuzzleFlash.SendMessage("Shoot", SendMessageOptions.DontRequireReceiver);
        }
        public virtual void DisableFiring(float seconds = 10000000)
        {
            m_NextAllowedFireTime = Time.time + seconds;
        }
        public virtual void EnableFiring()
        {
            m_NextAllowedFireTime = Time.time;

        }
        public override void Refresh()
        {

            if (!Application.isPlaying)
                return;

            // update muzzle flash position, scale and fadespeed from preset
            if (MuzzleFlash != null)
            {
                if (m_MuzzleFlashSpawnPoint == null)
                {
                    if (ProjectileSpawnPoint == m_ProjectileDefaultSpawnpoint)
                        m_MuzzleFlashSpawnPoint = fp_Utility.GetTransformByNameInChildren(ProjectileSpawnPoint.transform, "muzzle");
                    else
                        m_MuzzleFlashSpawnPoint = fp_Utility.GetTransformByNameInChildren(Transform, "muzzle");
                }

                if (m_MuzzleFlashSpawnPoint != null)
                {
                    m_MuzzleFlash.transform.parent = ProjectileSpawnPoint.transform;
                }
                else
                {
                    m_MuzzleFlash.transform.parent = ProjectileSpawnPoint.transform;
                    MuzzleFlash.transform.localPosition = MuzzleFlashPosition;
                    MuzzleFlash.transform.rotation = ProjectileSpawnPoint.transform.rotation;
                }

                MuzzleFlash.transform.localScale = MuzzleFlashScale;
                MuzzleFlash.SendMessage("SetFadeSpeed", MuzzleFlashFadeSpeed, SendMessageOptions.DontRequireReceiver);
            }
        }
        public override void Activate()
        {
            base.Activate();

            if (MuzzleFlash != null)
                fp_Utility.Activate(MuzzleFlash);
        }
        public override void Deactivate()
        {
            base.Deactivate();

            if (MuzzleFlash != null)
                fp_Utility.Activate(MuzzleFlash, false);
        }
    }
}