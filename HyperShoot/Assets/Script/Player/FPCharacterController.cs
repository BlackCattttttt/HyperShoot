using HyperShoot.Manager;
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

        private GamePlayController m_GamePlayController = null;
        public GamePlayController gamePlayController
        {
            get
            {
                if (m_GamePlayController == null)
                    m_GamePlayController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GamePlayController>();
                return m_GamePlayController;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            fp_TargetEvent<Vector3>.Register(m_Transform, "ForceImpact", AddForce);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            fp_TargetEvent<Vector3>.Unregister(m_Root, "ForceImpact", AddForce);
        }
        protected override void Start()
        {
            base.Start();

            SetPosition(Transform.position);

            if (PhysicsHasCollisionTrigger)
            {
                m_Trigger = new GameObject("Trigger");
                m_Trigger.tag = "Player";
                m_Trigger.transform.parent = m_Transform;
                m_Trigger.layer = fp_Layer.LocalPlayer;
                m_Trigger.transform.localPosition = Vector3.zero;

                m_TriggerCollider = m_Trigger.AddComponent<CapsuleCollider>();
                m_TriggerCollider.isTrigger = true;
                m_TriggerCollider.radius = characterController.radius + SkinWidth;
                m_TriggerCollider.height = characterController.height + (SkinWidth * 2.0f);
                m_TriggerCollider.center = characterController.center;
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
        }
        protected override void FixedUpdate()
        {
            if (Time.timeScale == 0.0f)
                return;

            // convert user input to motor throttle
            UpdateMotor();

            // apply motion generated by tapping or holding the jump button
            UpdateJump();

            // handle external forces like gravity, explosion shockwaves or wind
            UpdateForces();

            // apply sliding in slopes
            UpdateSliding();

            // detect when player falls, slides or gets pushed out of control
            UpdateOutOfControl();

            // update controller position based on current motor- & external forces
            FixedMove();

            // respond to environment collisions that may have happened during the move
            UpdateCollisions();

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
        protected virtual void UpdateJump()
        {
            UpdateJumpForceWalk();

            // apply accumulated 'hold jump' force
            m_MotorThrottle.y += m_MotorJumpForceAcc * Time.timeScale;

            // dampen forces
            m_MotorJumpForceAcc /= (1.0f + (MotorJumpForceHoldDamping * Time.timeScale));
            m_MotorThrottle.y /= (1.0f + (MotorJumpForceDamping * Time.timeScale));
        }
        protected virtual void UpdateJumpForceWalk()
        {
            if (Player.Jump.Active)
            {
                if (!m_Grounded)
                {
                    // accumulate 'hold jump' force if the jump button is still being held
                    // down 2 fixed frames after the impulse jump
                    if (m_MotorJumpForceHoldSkipFrames > 2)
                    {
                        // but only if jump button hasn't been released on the way down
                        if (!(Player.Velocity.Get().y < 0.0f))
                            m_MotorJumpForceAcc += MotorJumpForceHold;
                    }
                    else
                        m_MotorJumpForceHoldSkipFrames++;
                }
            }
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
        public virtual void AddForce(float x, float y, float z)
        {
            AddForce(new Vector3(x, y, z));
        }

        public virtual void AddForce(Vector3 force)
        {
            AddForceInternal(force);
        }
        protected virtual void UpdateSliding()
        {
            bool wasSlidingFast = m_SlideFast;
            bool wasSliding = m_Slide;

            // --- handle slope sliding ---
            m_Slide = false;
            if (!m_Grounded)
            {
                m_OnSteepGroundSince = 0.0f;
                m_SlideFast = false;
            }
            // start sliding if ground is steep enough in angles
            else if (GroundAngle > PhysicsSlopeSlideLimit)
            {
                m_Slide = true;

                // if ground angle is within slopelimit, slide at a constant speed
                if (GroundAngle <= Player.SlopeLimit.Get())
                {
                    m_SlopeSlideSpeed = Mathf.Max(m_SlopeSlideSpeed, (PhysicsSlopeSlidiness * 0.01f));
                    m_OnSteepGroundSince = 0.0f;
                    m_SlideFast = false;
                    // apply slope speed damping (and snap to zero if miniscule, to avoid
                    // floating point errors)
                    m_SlopeSlideSpeed = (Mathf.Abs(m_SlopeSlideSpeed) < 0.0001f) ? 0.0f :
                        (m_SlopeSlideSpeed / (1.0f + (0.05f * fp_TimeUtility.AdjustedTimeScale)));
                }
                else    // if steeper than slopelimit, slide with accumulating slide speed
                {
                    if ((m_SlopeSlideSpeed) > 0.01f)
                        m_SlideFast = true;
                    if (m_OnSteepGroundSince == 0.0f)
                        m_OnSteepGroundSince = Time.time;
                    m_SlopeSlideSpeed += (((PhysicsSlopeSlidiness * 0.01f) * ((Time.time - m_OnSteepGroundSince) * 0.125f)) * fp_TimeUtility.AdjustedTimeScale);
                    m_SlopeSlideSpeed = Mathf.Max((PhysicsSlopeSlidiness * 0.01f), m_SlopeSlideSpeed);  // keep minimum slidiness
                }

                // add horizontal force in the slope direction, multiplied by slidiness
                AddForce(Vector3.Cross(Vector3.Cross(GroundNormal, Vector3.down), GroundNormal) *
                    m_SlopeSlideSpeed * fp_TimeUtility.AdjustedTimeScale);

            }
            else
            {
                m_OnSteepGroundSince = 0.0f;
                m_SlideFast = false;
                m_SlopeSlideSpeed = 0.0f;
            }

            if (m_MotorThrottle != Vector3.zero)
                m_Slide = false;

            // handle fast sliding into free fall
            if (m_SlideFast)
                m_SlideFallSpeed = Transform.position.y;    // store y to calculate difference next frame
            else if (wasSlidingFast && !Grounded)
                m_FallSpeed = Transform.position.y - m_SlideFallSpeed;  // lost grounding while sliding fast: kickstart gravity at slide fall speed

            // detect whether the slide variables have changed, and broadcast
            // messages so external components can update accordingly

            if (wasSliding != m_Slide)
                Player.SetState("Slide", m_Slide);
        }
        void UpdateOutOfControl()
        {
            if ((m_ExternalForce.magnitude > 0.2f) ||       // TODO: make 0.2 a constant
                (m_FallSpeed < -0.2f) ||    // TODO: make 0.2 a constant
                    (m_SlideFast == true))
                Player.OutOfControl.Start();
            else if (Player.OutOfControl.Active)
                Player.OutOfControl.Stop();

        }
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
            if (Player.Dead.Active)
            {
                Player.InputMoveVector.Set(Vector2.zero);
                return;
            }
            // move on our own
            Player.Move.Send(fp_MathUtility.NaNSafeVector3(m_MoveDirection * Delta * Time.timeScale));

            // --- store ground info ---
            StoreGroundInfo();

            // --- handle loss of grounding ---
            if ((m_GroundHitTransform == null) && (m_LastGroundHitTransform != null))
            {
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
            }

            // --- respond to wall collision ---
            if ((m_PredictedPos.x != Transform.position.x) ||
                (m_PredictedPos.z != Transform.position.z) &&
                (m_ExternalForce != Vector3.zero))
                DeflectHorizontalForce();
        }
        public virtual void DeflectDownForce()
        {
            if (GroundAngle > PhysicsSlopeSlideLimit)
            {
                m_SlopeSlideSpeed = m_FallImpact * (0.25f * Time.timeScale);
            }
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

            if (!(Physics.CapsuleCast(CapsuleBottom, CapsuleTop, Player.Radius.Get(), m_PrevDir,
                out m_WallHit, Vector3.Distance(m_PrevPosition, m_PredictedPos), fp_Layer.Mask.ExternalBlockers)))
                return;

            m_NewDir = Vector3.Cross(m_WallHit.normal, Vector3.up).normalized;
            if ((Vector3.Dot(Vector3.Cross((m_WallHit.point - Transform.position),
                (m_PrevPosition - Transform.position)), Vector3.up)) > 0.0f)
                m_NewDir = -m_NewDir;

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
        }

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
            m_SmoothPosition = Transform.position;

        }
        protected virtual bool CanStart_Jump()
        {
            // can't jump without ground contact
            if (!m_Grounded)
                return false;

            // can't jump until the previous jump has stopped
            if (!m_MotorJumpDone)
                return false;

            // can't bunny-hop up steep surfaces
            if (GroundAngle > Player.SlopeLimit.Get())
                return false;

            // passed the test!
            return true;

        }
        protected virtual void OnStart_Jump()
        {
            m_MotorJumpDone = false;

            // perform impulse jump
            m_MotorThrottle.y = (MotorJumpForce / Time.timeScale);

            // sync camera y pos
            m_SmoothPosition.y = Transform.position.y;
        }

        protected virtual void OnStop_Jump()
        {
            m_MotorJumpDone = true;

        }
        protected virtual Vector3 OnValue_MotorThrottle
        {
            get { return m_MotorThrottle; }
            set { m_MotorThrottle = value; }
        }

        protected virtual bool CanStart_Run()
        {
            // can't start running while crouching
            if (Player.Crouch.Active)
                return false;
            if (gamePlayController.CurrenMisson != null && gamePlayController.CurrenMisson.skillType == MissonData.MissonAtribute.MissonType.SURVIVAL)
            {
                return false;
            }
            return true;

        }

        protected virtual bool CanStop_Crouch()
        {
            // can't stop crouching if there is a blocking object above us
            if (Physics.SphereCast(new Ray(Transform.position, Vector3.up),
                    Player.Radius.Get(),
                    (m_NormalHeight - Player.Radius.Get() + 0.01f),
                    fp_Layer.Mask.ExternalBlockers))
            {

                // regulate stop test interval to reduce amount of sphere casts
                Player.Crouch.NextAllowedStopTime = Time.time + 1.0f;

                // found a low ceiling above us - abort getting up
                return false;

            }

            // nothing above us - okay to get up!
            return true;

        }
        protected virtual bool OnValue_MotorJumpDone
        {
            get { return m_MotorJumpDone; }
        }

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
        protected virtual void OnMessage_ForceImpact(Vector3 force)
        {
            AddForce(force);
        }

        protected virtual void OnStop_Dead()
        {
            Player.OutOfControl.Stop();
        }
    }
}