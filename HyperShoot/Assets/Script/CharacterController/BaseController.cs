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

        protected Vector3 m_Velocity = Vector3.zero;            // velocity calculated in same way as unity's character controller
        protected Vector3 m_PrevPosition = Vector3.zero;    // position on end of each fixed timestep
        protected Vector3 m_PrevVelocity = Vector3.zero;    // used for calculating velocity, and detecting the start of a fall 
                                                            // event handler property cast as a playereventhandler
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



        /// <summary>
        /// 
        /// </summary>
        protected override void Awake()
        {

            base.Awake();

            InitCollider();

        }


        /// <summary>
        /// 
        /// </summary>
        protected override void Start()
        {

            base.Start();

            RefreshCollider();

        }


        /// <summary>
        /// 
        /// </summary>
        protected override void Update()
        {

            base.Update();

            // platform rotation is done in Update rather than FixedUpdate for
            // smooth remote player movement on platforms in multiplayer
            //UpdatePlatformRotation();

        }


        /// <summary>
        /// 
        /// </summary>
        protected override void FixedUpdate()
        {

            if (Time.timeScale == 0.0f)
                return;

            // updates external forces like gravity
            //UpdateForces();

            // update controller position based on current motor- & external forces
            FixedMove();

            // respond to environment collisions that may have happened during the move
            //UpdateCollisions();

            // move and rotate player along with rigidbodies & moving platforms
            //UpdatePlatformMove();

            // store final position and velocity for next frame's physics calculations
            UpdateVelocity();

        }
        /// <summary>
        /// stores final position and velocity for next frame's physics
        /// calculations
        /// </summary>
        protected virtual void UpdateVelocity()
        {

            m_PrevVelocity = m_Velocity;
            m_Velocity = (transform.position - m_PrevPosition) / Time.deltaTime;
            m_PrevPosition = Transform.position;

        }

        /// <summary>
        /// override this to completely stop the controller in one frame
        /// IMPORTANT: remember to call this base method too
        /// </summary>
        public virtual void Stop()
        {

            Player.Move.Send(Vector3.zero);
            Player.InputMoveVector.Set(Vector2.zero);
            //m_FallSpeed = 0.0f;
            m_FallStartHeight = NOFALL;

        }


        /// <summary>
        /// this method should be overridden to initialize dynamic collider dimension
        /// variables for various states as needed, depending on whether the collider
        /// is a capsule collider, character controller or other type of collider
        /// </summary>
        protected virtual void InitCollider()
        {
        }


        /// <summary>
        /// this method should be overridden to refresh collider dimension variables
        /// depending on various states as needed
        /// </summary>
        protected virtual void RefreshCollider()
        {
        }


        /// <summary>
        /// this method should be overridden to enable or disable the collider, whether
        /// a capsule collider, character controller or other type of collider
        /// </summary>
        public virtual void EnableCollider(bool enabled)
        {
        }


        /// <summary>
        /// performs a sphere cast (as wide as the character) from ~knees to ground, and
        /// saves hit info in the 'm_GroundHit' variable. this gives access to lots of
        /// data on the object directly below us, object transform, ground angle etc.
        /// </summary>
        protected virtual void StoreGroundInfo()
        {

            // store ground hit for detecting fall impact and loss of grounding
            // in next frame
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
                // SNIPPET: use this if spherecast somehow returns the non-collider parent of your platform
                //if (m_GroundHitTransform.collider == null)
                //{
                //	Collider c = m_GroundHitTransform.GetComponentInChildren<Collider>();
                //	m_GroundHitTransform = c.transform;
                //}

                m_Grounded = true;

                //Debug.Log(m_GroundHitTransform);

            }

            // detect walking OFF AN EDGE into a fall (for fall impact)
            //if ((m_Velocity.y < 0) && (m_GroundHitTransform == null)
            //	&& (m_LastGroundHitTransform != null)
            //	&& !Player.Jump.Active)
            //	SetFallHeight(Transform.position.y);

            return;

        }


        /// <summary>
        /// sets the value used for fall impact calculation
        /// according to certain criteria
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
        protected virtual void FixedMove()
        {

            StoreGroundInfo();

        }


        /// <summary>
        /// returns current fall distance, calculated as the altitude
        /// where fall began minus current altitude 
        /// </summary>
        float FallDistance
        {
            get
            {
                return ((m_FallStartHeight != NOFALL) ? // only report positive fall distance if we have stored a fall height
                        Mathf.Max(0.0f, (m_FallStartHeight - Transform.position.y)) : 0);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void LateUpdate()
        {

            base.LateUpdate();

        }
        public virtual void SetPosition(Vector3 position)
        {

            Transform.position = position;
            m_PrevPosition = position;
            // must zero out 'm_PrevVelocity.y' at beginning of next frame in case
            // we're teleporting into free fall, or fall impact detection will break
            //fp_Timer.In(0, () => { m_PrevVelocity = vp_3DUtility.HorizontalVector(m_PrevVelocity); });

        }
        /// <summary>
        /// stops the controller in one frame, killing all forces
        /// acting upon it
        /// </summary>
        protected virtual void OnMessage_Stop()
        {
            Stop();
        }

        protected virtual void OnMessage_Moved(Vector2 dir)
        {

        }
        /// <summary>
        /// 
        /// </summary>
        protected virtual void OnStart_Crouch()
        {

            // force-stop the run activity
            Player.Run.Stop();

            // modify collider size
            RefreshCollider();

        }


        /// <summary>
        /// 
        /// </summary>
        protected virtual void OnStop_Crouch()
        {
            // modify collider size
            RefreshCollider();
        }
        /// <summary>
        /// gets or sets the world position of the controller
        /// </summary>
        protected virtual Vector3 OnValue_Position
        {
            get { return Transform.position; }
            set { SetPosition(value); }
        }
        /// <summary>
        /// this method must be overridden to get/set collider radius
        /// in a manner compatible with the current type of collider
        /// </summary>
        protected abstract float OnValue_Radius { get; }


        /// <summary>
        /// this method must be overridden to get/set collider height
        /// in a manner compatible with the current type of collider
        /// </summary>
        protected abstract float OnValue_Height { get; }


        /// <summary>
        /// returns whether the controller is grounded
        /// </summary>
        protected virtual bool OnValue_Grounded
        {
            get { return m_Grounded; }
        }

    }
}
