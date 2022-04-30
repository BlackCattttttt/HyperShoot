using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HyperShoot.Player
{
    public class FPCharacterController : FPController
    {
        // general
        protected Vector3 m_FixedPosition = Vector3.zero;       // exact position. updates at a fixed interval and is used for gameplay
        protected Vector3 m_SmoothPosition = Vector3.zero;      // smooth position. updates as often as possible and is only used for the camera
        public Vector3 SmoothPosition { get { return m_SmoothPosition; } }  // a version of the controller position calculated in 'Update' to get smooth camera motion
        public Vector3 Velocity { get { return characterController.velocity; } }
        protected bool m_IsFirstPerson = true;

        public Vector3 GroundNormal { get { return m_GroundHit.normal; } }
        public float GroundAngle { get { return Vector3.Angle(m_GroundHit.normal, Vector3.up); } }
        protected RaycastHit m_CeilingHit;                  // contains info about any ceilings we may have bumped into
        protected RaycastHit m_WallHit;						// contains info about any horizontal blockers we may have collided with

        // physics trigger
        protected CapsuleCollider m_TriggerCollider = null;     // trigger collider for incoming objects to detect us
        public bool PhysicsHasCollisionTrigger = true;          // whether to automatically generate a child object with a trigger on startup
        protected GameObject m_Trigger = null;					// trigger gameobject for detection of incoming objects

        // motor
        public float MotorAcceleration = 0.18f;
        public float MotorDamping = 0.17f;
        public float MotorBackwardsSpeed = 0.65f;
        public float MotorAirSpeed = 0.35f;
        public float MotorSlopeSpeedUp = 1.0f;
        public float MotorSlopeSpeedDown = 1.0f;
        protected Vector3 m_MoveDirection = Vector3.zero;
        protected float m_SlopeFactor = 1.0f;
        protected Vector3 m_MotorThrottle = Vector3.zero;
        protected float m_MotorAirSpeedModifier = 1.0f;
        protected float m_CurrentAntiBumpOffset = 0.0f;
        // jump
        public float MotorJumpForce = 0.18f;
        public float MotorJumpForceDamping = 0.08f;
        public float MotorJumpForceHold = 0.003f;
        public float MotorJumpForceHoldDamping = 0.5f;
        protected int m_MotorJumpForceHoldSkipFrames = 0;
        protected float m_MotorJumpForceAcc = 0.0f;
        protected bool m_MotorJumpDone = true;
        // physics
        public float PhysicsForceDamping = 0.05f;           // damping of external forces
        public float PhysicsSlopeSlideLimit = 30.0f;        // steepness in angles above which controller will start to slide
        public float PhysicsSlopeSlidiness = 0.15f;         // slidiness of the surface that we're standing on. will be additive if steeper than CharacterController.slopeLimit
        public float PhysicsWallBounce = 0.0f;              // how much to bounce off walls
        public float PhysicsWallFriction = 0.0f;
        protected Vector3 m_ExternalForce = Vector3.zero;   // current velocity from external forces (explosion knockback, jump pads, rocket packs)
        protected Vector3[] m_SmoothForceFrame = new Vector3[120];
        protected bool m_Slide = false;                     // are sliding on a steep surface without moving?
        protected bool m_SlideFast = false;                 // have we accumulated a quick speed from standing on a slope above 'slopeLimit'
        protected float m_SlideFallSpeed = 0.0f;            // fall speed resulting from sliding fast into free fall
        protected float m_OnSteepGroundSince = 0.0f;        // the point in time at which we started standing on a slope above 'slopeLimit'. used to calculate slide speed accumulation
        protected float m_SlopeSlideSpeed = 0.0f;           // current velocity from sliding
        protected Vector3 m_PredictedPos = Vector3.zero;
        protected Vector3 m_PrevDir = Vector3.zero;
        protected Vector3 m_NewDir = Vector3.zero;
        protected float m_ForceImpact = 0.0f;
        protected float m_ForceMultiplier = 0.0f;
        protected Vector3 CapsuleBottom = Vector3.zero;
        protected Vector3 CapsuleTop = Vector3.zero;

        protected override void Start()
        {
            base.Start();

            SetPosition(Transform.position);

            if (PhysicsHasCollisionTrigger)
            {

                m_Trigger = new GameObject("Trigger");
                m_Trigger.transform.parent = m_Transform;
                m_Trigger.layer = fp_Layer.LocalPlayer;
                m_Trigger.transform.localPosition = Vector3.zero;

                m_TriggerCollider = m_Trigger.AddComponent<CapsuleCollider>();
                m_TriggerCollider.isTrigger = true;
                m_TriggerCollider.radius = characterController.radius + SkinWidth;
                m_TriggerCollider.height = characterController.height + (SkinWidth * 2.0f);
                m_TriggerCollider.center = characterController.center;

              //  m_Trigger.gameObject.AddComponent<vp_DamageTransfer>();

                // if we have a SurfaceIdentifier, copy it along with its values onto the trigger.
                // this will make the trigger emit the same fx as the controller when hit by bullets
                //if (SurfaceIdentifier != null)
                //{
                //    fp_Timer.In(0.05f, () =>    // wait atleast one frame for this to take effect properly
                //    {
                //        vp_SurfaceIdentifier triggerSurfaceIdentifier = m_Trigger.gameObject.AddComponent<vp_SurfaceIdentifier>();
                //        triggerSurfaceIdentifier.SurfaceType = SurfaceIdentifier.SurfaceType;
                //        triggerSurfaceIdentifier.AllowDecals = SurfaceIdentifier.AllowDecals;
                //    });
                //}

            }
        }
        protected override void RefreshCollider()
        {

            base.RefreshCollider();

            // update physics trigger size
            if (m_TriggerCollider != null)
            {
                m_TriggerCollider.radius = characterController.radius + SkinWidth;
                m_TriggerCollider.height = characterController.height + (SkinWidth * 2.0f);
                m_TriggerCollider.center = characterController.center;
            }

        }
        public override void EnableCollider(bool isEnabled = true)
        {

            if (characterController != null)
                characterController.enabled = isEnabled;

        }
        protected override void Update()
        {

            base.Update();

            // simulate high-precision movement for smoothest possible camera motion
            SmoothMove();

            // TIP: uncomment either of these lines to debug print the
            // speed of the character controller
            //Debug.Log(Velocity.magnitude);		// speed in meters per second
            //Debug.Log(Controller.Velocity.sqrMagnitude);	// speed as used by the camera bob

        }
        protected override void FixedUpdate()
        {

            if (Time.timeScale == 0.0f)
                return;

            // convert user input to motor throttle
            UpdateMotor();

            // apply motion generated by tapping or holding the jump button
            //UpdateJump();

            // handle external forces like gravity, explosion shockwaves or wind
            UpdateForces();

            // apply sliding in slopes
            //UpdateSliding();

            // detect when player falls, slides or gets pushed out of control
            //UpdateOutOfControl();

            // update controller position based on current motor- & external forces
            FixedMove();

            // respond to environment collisions that may have happened during the move
            UpdateCollisions();

            // move and rotate player along with rigidbodies & moving platforms
            //UpdatePlatformMove();

            // store final position and velocity for next frame's physics calculations
            UpdateVelocity();

        }

        protected virtual void UpdateMotor()
        {
            UpdateThrottleWalk();
            // snap super-small values to zero to avoid floating point issues
            m_MotorThrottle = fp_MathUtility.SnapToZero(m_MotorThrottle);

        }
        protected virtual void UpdateThrottleWalk()
        {

            // if on the ground, make movement speed dependent on ground slope
            UpdateSlopeFactor();

            // update air speed modifier
            // (at 1.0, this will completely prevent the controller from altering
            // its trajectory while in the air, and will disable motor damping)
            m_MotorAirSpeedModifier = (m_Grounded ? 1.0f : MotorAirSpeed);

            // convert horizontal input to forces in the motor
            m_MotorThrottle +=
                ((Player.InputMoveVector.Get().y > 0) ? Player.InputMoveVector.Get().y : // if moving forward or sideways: use normal speed
                (Player.InputMoveVector.Get().y * MotorBackwardsSpeed))     // if moving backwards: apply backwards-modifier
                * (Transform.TransformDirection(Vector3.forward * (MotorAcceleration * 0.1f) * m_MotorAirSpeedModifier) * m_SlopeFactor);
            m_MotorThrottle += Player.InputMoveVector.Get().x * (Transform.TransformDirection(Vector3.right * (MotorAcceleration * 0.1f) * m_MotorAirSpeedModifier) * m_SlopeFactor);

            // dampen motor force
            m_MotorThrottle.x /= (1.0f + (MotorDamping * m_MotorAirSpeedModifier * Time.timeScale));
            m_MotorThrottle.z /= (1.0f + (MotorDamping * m_MotorAirSpeedModifier * Time.timeScale));
        }
        protected override void UpdateForces()
        {

            base.UpdateForces();

            // apply smooth force (forces applied over several frames)
            if (m_SmoothForceFrame[0] != Vector3.zero)
            {
                AddForceInternal(m_SmoothForceFrame[0]);
                for (int v = 0; v < 120; v++)
                {
                    m_SmoothForceFrame[v] = (v < 119) ? m_SmoothForceFrame[v + 1] : Vector3.zero;
                    if (m_SmoothForceFrame[v] == Vector3.zero)
                        break;
                }
            }

            // dampen external forces
            m_ExternalForce /= (1.0f + (PhysicsForceDamping * fp_TimeUtility.AdjustedTimeScale));

        }
        protected virtual void AddForceInternal(Vector3 force)
        {
            m_ExternalForce += force;
        }
        /// <summary>
        /// this method calculates a controller velocity multiplier
        /// depending on ground slope. at 'MotorSlopeSpeed' 1.0,
        /// velocity in slopes will be kept roughly the same as on
        /// flat ground. values lower or higher than 1 will make the
        /// controller slow down / speed up, depending on whether
        /// we're moving uphill or downhill
        /// </summary>
        protected virtual void UpdateSlopeFactor()
        {

            if (!m_Grounded)
            {
                m_SlopeFactor = 1.0f;
                return;
            }

            // determine if we're moving uphill or downhill
            m_SlopeFactor = 1 + (1.0f - (Vector3.Angle(m_GroundHit.normal, m_MotorThrottle) / 90.0f));

            if (Mathf.Abs(1 - m_SlopeFactor) < 0.01f)
                m_SlopeFactor = 1.0f;       // standing still or moving on flat ground, or moving perpendicular to a slope
            else if (m_SlopeFactor > 1.0f)
            {
                // moving downhill
                if (MotorSlopeSpeedDown == 1.0f)
                {
                    // 1.0 means 'no change' so we'll alter the value to get
                    // roughly the same velocity as if ground was flat
                    m_SlopeFactor = 1.0f / m_SlopeFactor;
                    m_SlopeFactor *= 1.2f;
                }
                else
                    m_SlopeFactor *= MotorSlopeSpeedDown;   // apply user defined multiplier
            }
            else
            {
                // moving uphill
                if (MotorSlopeSpeedUp == 1.0f)
                {
                    // 1.0 means 'no change' so we'll alter the value to get
                    // roughly the same velocity as if ground was flat
                    m_SlopeFactor *= 1.2f;
                }
                else
                    m_SlopeFactor *= MotorSlopeSpeedUp; // apply user defined multiplier

                // kill motor if moving into a slope steeper than 'slopeLimit'. this serves
                // to prevent exploits with being able to walk up steep surfaces and walls
                m_SlopeFactor = (GroundAngle > Player.SlopeLimit.Get()) ? 0.0f : m_SlopeFactor;

            }

        }
        protected override void FixedMove()
        {

            // --- apply forces ---
            m_MoveDirection = Vector3.zero;
            m_MoveDirection += m_ExternalForce;
            m_MoveDirection += m_MotorThrottle;
            m_MoveDirection.y += m_FallSpeed;

            // --- apply anti-bump offset ---
            // this pushes the controller towards the ground to prevent the character
            // from "bumpety-bumping" when walking down slopes or stairs. the strength
            // of this effect is determined by the character controller's 'Step Offset'
            m_CurrentAntiBumpOffset = 0.0f;
            if (m_Grounded && m_MotorThrottle.y <= 0.001f)
            {
                m_CurrentAntiBumpOffset = Mathf.Max(Player.StepOffset.Get(), Vector3.Scale(m_MoveDirection, (Vector3.one - Vector3.up)).magnitude);
                m_MoveDirection += m_CurrentAntiBumpOffset * Vector3.down;
            }

            // --- predict move result ---
            // do some prediction in order to detect blocking and deflect forces on collision
            m_PredictedPos = Transform.position + fp_MathUtility.NaNSafeVector3(m_MoveDirection * Delta * Time.timeScale);

            // --- move the charactercontroller ---

            // ride along with movable objects
            //if (m_Platform != null && PositionOnPlatform != Vector3.zero)
            //	Player.Move.Send(vp_MathUtility.NaNSafeVector3(m_Platform.TransformPoint(PositionOnPlatform) -
            //															m_Transform.position));

            // move on our own
            Player.Move.Send(fp_MathUtility.NaNSafeVector3(m_MoveDirection * Delta * Time.timeScale));

            // while there is an active death event, block movement input
            if (Player.Dead.Active)
            {
                Player.InputMoveVector.Set(Vector2.zero);
                return;
            }

            // --- store ground info ---
            StoreGroundInfo();

            // --- store head contact info ---
            // spherecast upwards for some info on the surface touching the top of the collider, if any
            //if (!m_Grounded && (Player.Velocity.Get().y > 0.0f))
            //{
            //	Physics.SphereCast(new Ray(Transform.position, Vector3.up),
            //								Player.Radius.Get(), out m_CeilingHit,
            //								Player.Height.Get() - (Player.Radius.Get() - SkinWidth) + 0.01f,
            //								vp_Layer.Mask.ExternalBlockers);
            //	m_HeadContact = (m_CeilingHit.collider != null);
            //}
            //else
            //	m_HeadContact = false;

            // --- handle loss of grounding ---
            if ((m_GroundHitTransform == null) && (m_LastGroundHitTransform != null))
            {

                // if we lost contact with a moving object, inherit its speed
                // then forget about it
                //if (m_Platform != null && PositionOnPlatform != Vector3.zero)
                //{
                //	AddForce(m_Platform.position - m_LastPlatformPos);
                //	m_Platform = null;
                //}

                // undo anti-bump offset to make the fall smoother
                if (m_CurrentAntiBumpOffset != 0.0f)
                {
                    Player.Move.Send(fp_MathUtility.NaNSafeVector3(m_CurrentAntiBumpOffset * Vector3.up) * Delta * Time.timeScale);
                    m_PredictedPos += fp_MathUtility.NaNSafeVector3(m_CurrentAntiBumpOffset * Vector3.up) * Delta * Time.timeScale;
                    m_MoveDirection += m_CurrentAntiBumpOffset * Vector3.up;
                }

            }
        }
        protected override void UpdateCollisions()
        {

            base.UpdateCollisions();

            if (m_OnNewGround)
            {

                // deflect the controller sideways under some circumstances
                if (m_WasFalling)
                {

                    DeflectDownForce();

                    // sync camera y pos
                    m_SmoothPosition.y = Transform.position.y;

                    // reset all the jump variables
                    m_MotorThrottle.y = 0.0f;
                    m_MotorJumpForceAcc = 0.0f;
                    m_MotorJumpForceHoldSkipFrames = 0;
                }
                // detect and store moving platforms	// TODO: should be in base class for AI
                //if (m_GroundHit.collider.gameObject.layer == fp_Layer.MovingPlatform)
                //{
                //    m_Platform = m_GroundHitTransform;
                //    m_LastPlatformAngle = m_Platform.eulerAngles.y;
                //}
               // else
                //    m_Platform = null;

            }

            // --- respond to wall collision ---
            // if the controller didn't end up at the predicted position, some
            // external object has blocked its way, so deflect the movement forces
            // to avoid getting stuck at walls
            if ((m_PredictedPos.x != Transform.position.x) ||
                (m_PredictedPos.z != Transform.position.z) &&
                (m_ExternalForce != Vector3.zero))
                DeflectHorizontalForce();

        }
        public virtual void DeflectDownForce()
        {

            // if we land on a surface tilted above the slide limit, convert
            // fall speed into slide speed on impact
            //if (GroundAngle > PhysicsSlopeSlideLimit)
            //{
            //    m_SlopeSlideSpeed = m_FallImpact * (0.25f * Time.timeScale);
            //}

            // deflect away from nearly vertical surfaces. this serves to make
            // falling along walls smoother, and to prevent the controller
            // from getting stuck on vertical walls when falling into them
            if (GroundAngle > 85)
            {
                m_MotorThrottle += (fp_3DUtility.HorizontalVector((GroundNormal * m_FallImpact)));
                m_Grounded = false;
            }

        }
        protected virtual void DeflectHorizontalForce()
        {
            // flatten positions (this is 2d) and get our direction at point of impact
            m_PredictedPos.y = Transform.position.y;
            m_PrevPosition.y = Transform.position.y;
            m_PrevDir = (m_PredictedPos - m_PrevPosition).normalized;

            // get the origins of the controller capsule's spheres at prev position
            CapsuleBottom = m_PrevPosition + Vector3.up * (Player.Radius.Get());
            CapsuleTop = CapsuleBottom + Vector3.up * (Player.Height.Get() - (Player.Radius.Get() * 2));

            // capsule cast from the previous position to the predicted position to find
            // the exact impact point. this capsule cast does not include the skin width
            // (it's not really needed plus we don't want ground collisions)
            if (!(Physics.CapsuleCast(CapsuleBottom, CapsuleTop, Player.Radius.Get(), m_PrevDir,
                out m_WallHit, Vector3.Distance(m_PrevPosition, m_PredictedPos), fp_Layer.Mask.ExternalBlockers)))
                return;

            // the force will be deflected perpendicular to the impact normal, and to the
            // left or right depending on whether the previous position is to our left or
            // right when looking back at the impact point from the current position
            m_NewDir = Vector3.Cross(m_WallHit.normal, Vector3.up).normalized;
            if ((Vector3.Dot(Vector3.Cross((m_WallHit.point - Transform.position),
                (m_PrevPosition - Transform.position)), Vector3.up)) > 0.0f)
                m_NewDir = -m_NewDir;

            // calculate how the current force gets absorbed depending on angle of impact.
            // if we hit a wall head-on, almost all force will be absorbed, but if we
            // barely glance it, force will be almost unaltered (depending on friction)
            m_ForceMultiplier = Mathf.Abs(Vector3.Dot(m_PrevDir, m_NewDir)) * (1.0f - (PhysicsWallFriction));

            // if the controller has wall bounciness, apply it
            if (PhysicsWallBounce > 0.0f)
            {
                m_NewDir = Vector3.Lerp(m_NewDir, Vector3.Reflect(m_PrevDir, m_WallHit.normal), PhysicsWallBounce);
                m_ForceMultiplier = Mathf.Lerp(m_ForceMultiplier, 1.0f, (PhysicsWallBounce * (1.0f - (PhysicsWallFriction))));
            }

            // deflect current force and report the impact
            m_ForceImpact = 0.0f;
            float yBak = m_ExternalForce.y;
            m_ExternalForce.y = 0.0f;
            m_ForceImpact = m_ExternalForce.magnitude;
            m_ExternalForce = m_NewDir * m_ExternalForce.magnitude * m_ForceMultiplier;
            m_ForceImpact = m_ForceImpact - m_ExternalForce.magnitude;
            for (int v = 0; v < 120; v++)
            {
                if (m_SmoothForceFrame[v] == Vector3.zero)
                    break;
                m_SmoothForceFrame[v] = m_SmoothForceFrame[v].magnitude * m_NewDir * m_ForceMultiplier;
            }
            m_ExternalForce.y = yBak;

            // TIP: the force that was absorbed by the bodies during the impact can be used for
            // things like damage, so an event could be sent here with the amount of absorbed force

        }
        /// <summary>
        /// since the controller is moved in FixedUpdate and the
        /// camera in Update there will be noticeable camera jitter.
        /// this method simulates the controller move in Update and
        /// stores the smooth position for the camera to read
        /// </summary>
        protected virtual void SmoothMove()
        {

            if (Time.timeScale == 0.0f)
                return;

            // restore last smoothpos
            m_FixedPosition = Transform.position;   // backup fixedpos
            Transform.position = m_SmoothPosition;

            // move controller to get the smooth position
            Player.Move.Send(fp_MathUtility.NaNSafeVector3((m_MoveDirection * Delta * Time.timeScale)));
            m_SmoothPosition = Transform.position;
            Transform.position = m_FixedPosition;   // restore fixedpos

            // reset smoothpos in these cases
            if ((Vector3.Distance(Transform.position, m_SmoothPosition) > Player.Radius.Get())) // smoothpos deviates too much
                                                                                                //|| (m_Platform != null) && ((m_LastPlatformPos != m_Platform.position)))        // we're on a platform thas is moving (causes jitter)
                m_SmoothPosition = Transform.position;

            // lerp smoothpos back to fixedpos slowly over time
            m_SmoothPosition = Vector3.Lerp(m_SmoothPosition, Transform.position, Time.deltaTime);

        }

        public override void SetPosition(Vector3 position)
        {

            base.SetPosition(position);
            m_SmoothPosition = position;

        }

        public override void Stop()
        {

            base.Stop();

            m_MotorThrottle = Vector3.zero;
            m_MotorJumpDone = true;
            m_MotorJumpForceAcc = 0.0f;
            m_ExternalForce = Vector3.zero;
            //	StopSoftForce();
            m_SmoothPosition = Transform.position;

        }

        protected virtual Vector3 OnValue_MotorThrottle
        {
            get { return m_MotorThrottle; }
            set { m_MotorThrottle = value; }
        }


        /// <summary>
        /// returns true if the current jump has ended, false if not
        /// </summary>
        protected virtual bool OnValue_MotorJumpDone
        {
            get { return m_MotorJumpDone; }
        }


        /// <summary>
        /// always returns true if the player is in 1st person mode,
        /// and false in 3rd person
        /// </summary>
        protected virtual bool OnValue_IsFirstPerson
        {

            get
            {
                return m_IsFirstPerson;
            }
            set
            {
                m_IsFirstPerson = value;
            }

        }
    }
}