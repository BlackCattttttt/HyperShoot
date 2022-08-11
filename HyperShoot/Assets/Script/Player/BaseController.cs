using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperShoot.Player
{
    public abstract class BaseController : fp_Component
    {
        // ground collision
        public bool Grounded { get { return m_Grounded; } }
        public Transform GroundTransform { get { return m_GroundHitTransform; } }   // current transform of the collider we're standing on
        protected bool m_Grounded = false;
        protected RaycastHit m_GroundHit;                   // contains info about the ground we're standing on, if any
        protected Transform m_LastGroundHitTransform;       // ground hit from last frame: used to detect ground collision changes
        protected Transform m_GroundHitTransform;           // ground hit from last frame: used to detect ground collision changes
        protected float m_FallStartHeight = NOFALL;         // used for calculating fall impact
        protected float m_FallImpact = 0.0f;
        protected bool m_OnNewGround = false;
        protected bool m_WasFalling = false;

        // gravity
        public float PhysicsGravityModifier = 0.2f;         // affects fall speed
        protected float m_FallSpeed = 0.0f;                 // determines how quickly the controller falls in the world
        protected const float PHYSICS_GRAVITY_MODIFIER_INTERNAL = 0.002f;	// retained for backwards compatibility

        // crouching
        protected float m_NormalHeight = 0.0f;              // height of the player controller when not crouching (stored from the character controller in Start)
        protected Vector3 m_NormalCenter = Vector3.zero;    // forced to half of the controller height (for crouching logic)
        protected float m_CrouchHeight = 0.0f;              // height of the player controller when crouching (calculated in Start)
        protected Vector3 m_CrouchCenter = Vector3.zero;    // will be half of the crouch height, but no smaller than the crouch radius

        // constants (for backwards compatibility and special cases)
        protected const float KINETIC_PUSHFORCE_MULTIPLIER = 15.0f;     // makes 'kinetic' push force roughly similar to 'simplified' when pushing a 1x1 m cube with mass 1
        protected const float CHARACTER_CONTROLLER_SKINWIDTH = 0.08f;   // NOTE: should be kept the same as the Unity CharacterController's 'Skin Width' parameter, which is unfortunately not exposed to script
        protected const float DEFAULT_RADIUS_MULTIPLIER = 0.25f;        // forces width of controller capsule to a percentage of its height
        protected const float FALL_IMPACT_MULTIPLIER = 0.075f;          // for backwards compatibility (pre 1.5.0)
        protected const float NOFALL = -99999;                          // when fall height is set to this value it means no fall impact will be reported

        //physic
        public float PhysicsPushForce = 5.0f;				// mass for pushing around rigidbodies
        public float PhysicsCrouchHeightModifier = 0.5f;	// how much to downscale the controller when crouching
        protected Vector3 m_Velocity = Vector3.zero;            // velocity calculated in same way as unity's character controller
        protected Vector3 m_PrevPosition = Vector3.zero;    // position on end of each fixed timestep
        protected Vector3 m_PrevVelocity = Vector3.zero;    // used for calculating velocity, and detecting the start of a fall 

        public float SkinWidth { get { return CHARACTER_CONTROLLER_SKINWIDTH; } }

        private CharacterEventHandler m_Player = null;
        protected CharacterEventHandler Player
        {
            get
            {
                if (m_Player == null)
                {
                    if (EventHandler != null)
                        m_Player = (CharacterEventHandler)EventHandler;
                }
                return m_Player;
            }
        }

        protected override void Awake()
        {

            base.Awake();

            InitCollider();

        }

        protected override void Start()
        {
            base.Start();

           RefreshCollider();
        }

        protected override void Update()
        {
            base.Update();
        }

        protected override void FixedUpdate()
        {

            if (Time.timeScale == 0.0f)
                return;

            // updates external forces like gravity
            UpdateForces();

            // update controller position based on current motor- & external forces
            FixedMove();

            // respond to environment collisions that may have happened during the move
            UpdateCollisions();

            // store final position and velocity for next frame's physics calculations
            UpdateVelocity();

        }
        protected virtual void UpdateForces()
        {
            // store ground for detecting fall impact and loss of grounding this frame
            m_LastGroundHitTransform = m_GroundHitTransform;

            // accumulate gravity
            if (m_Grounded && (m_FallSpeed <= 0.0f))
            // when not falling, stick controller to the ground by a small, fixed gravity
            {
                m_FallSpeed = (Physics.gravity.y * (PhysicsGravityModifier * PHYSICS_GRAVITY_MODIFIER_INTERNAL) * fp_TimeUtility.AdjustedTimeScale);
                return;
            }
            else
            {
                m_FallSpeed += (Physics.gravity.y * (PhysicsGravityModifier * PHYSICS_GRAVITY_MODIFIER_INTERNAL) * fp_TimeUtility.AdjustedTimeScale);

                // detect starting to fall MID-JUMP (for fall impact)
                if ((m_Velocity.y < 0) && (m_PrevVelocity.y >= 0.0f))
                    SetFallHeight(Transform.position.y);
            }
        }
        protected virtual void UpdateCollisions()
        {
            m_FallImpact = 0.0f;
            m_OnNewGround = false;
            m_WasFalling = false;

            // respond to ground collision
            if ((m_GroundHitTransform != null)
             && (m_GroundHitTransform != m_LastGroundHitTransform))
            {
                m_OnNewGround = true;

                if (m_LastGroundHitTransform == null)
                {
                    m_WasFalling = true;
                    if ((m_FallStartHeight > Transform.position.y) && m_Grounded)
                    {
                        m_FallImpact = FallDistance * FALL_IMPACT_MULTIPLIER;
                        Player.FallImpact.Send(m_FallImpact);
                        //Debug.Log("DISTANCE: " + FallDistance);
                    }
                }
               m_FallStartHeight = NOFALL;
            }
        }

        protected virtual void UpdateVelocity()
        {
            m_PrevVelocity = m_Velocity;
            m_Velocity = (transform.position - m_PrevPosition) / Time.deltaTime;
            m_PrevPosition = Transform.position;
        }

        public virtual void Stop()
        {
            Player.Move.Send(Vector3.zero);
            Player.InputMoveVector.Set(Vector2.zero);
            m_FallSpeed = 0.0f;
            m_FallStartHeight = NOFALL;
        }

        protected virtual void InitCollider()
        {
        }

        protected virtual void RefreshCollider()
        {
        }

        public virtual void EnableCollider(bool enabled)
        {
        }

        protected virtual void StoreGroundInfo()
        {
            m_LastGroundHitTransform = m_GroundHitTransform;

            m_Grounded = false;
            m_GroundHitTransform = null;
            // spherecast to just below feet to see if we're grounded - and if so store ground info
            if (
            Physics.SphereCast(new Ray(Transform.position + Vector3.up * (Player.Radius.Get()), Vector3.down),
                                        (Player.Radius.Get()), out m_GroundHit, (CHARACTER_CONTROLLER_SKINWIDTH + 0.1f),
                                        fp_Layer.Mask.ExternalBlockers))
            {

                m_GroundHitTransform = m_GroundHit.transform;

                m_Grounded = true;

                //Debug.Log(m_GroundHitTransform);
            }

            // detect walking OFF AN EDGE into a fall (for fall impact)
            if ((m_Velocity.y < 0) && (m_GroundHitTransform == null)
                && (m_LastGroundHitTransform != null)
                && !Player.Jump.Active)
                SetFallHeight(Transform.position.y);

            return;
        }

        protected void SetFallHeight(float height)
        {
            // we can only track one fall at a time
            if (m_FallStartHeight != NOFALL)
                return;

            // can't set fall height if grounded
            if (m_Grounded || m_GroundHitTransform != null)
                return;

            m_FallStartHeight = height;

        }

        protected virtual void FixedMove()
        {
            StoreGroundInfo();
        }

        float FallDistance
        {
            get
            {
                return ((m_FallStartHeight != NOFALL) ? // only report positive fall distance if we have stored a fall height
                        Mathf.Max(0.0f, (m_FallStartHeight - Transform.position.y)) : 0);
            }
        }

        protected override void LateUpdate()
        {

            base.LateUpdate();

        }
        public virtual void SetPosition(Vector3 position)
        {
            Transform.position = position;
            m_PrevPosition = position;

        }

        protected virtual void OnMessage_Stop()
        {
            Stop();
        }

        protected virtual void OnMessage_Moved(Vector2 dir)
        {

        }

        protected virtual void OnStart_Crouch()
        {
            // force-stop the run activity
            Player.Run.Stop();

            // modify collider size
            RefreshCollider();
        }

        protected virtual void OnStop_Crouch()
        {
            // modify collider size
            RefreshCollider();
        }

        protected virtual Vector3 OnValue_Position
        {
            get { return Transform.position; }
            set { SetPosition(value); }
        }

        protected abstract float OnValue_Radius { get; }

        protected abstract float OnValue_Height { get; }

        protected virtual bool OnValue_Grounded
        {
            get { return m_Grounded; }
        }
        protected virtual float OnValue_FallSpeed
        {
            get { return m_FallSpeed; }
            set { m_FallSpeed = value; }
        }

        protected virtual Vector3 OnValue_Velocity
        {
            get { return m_Velocity; }
            set { m_Velocity = value; }
        }

    }
}
