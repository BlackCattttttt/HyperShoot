using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HyperShoot.Player
{
    public class FPInput : fp_Component
    {
        public Vector2 MouseLookSensitivity = new Vector2(5.0f, 5.0f);
        public bool MouseLookMutePitch = false;             // use this to make the 'InputSmoothLook' and 'InputRawLook' events always return zero pitch / yaw , regardless of sensitivity
        public bool MouseLookMuteYaw = false;
        public int MouseLookSmoothSteps = 10;               // allowed range: 1-20
        public float MouseLookSmoothWeight = 0.5f;			// allowed range: 0.0f - 1.0f// -		"	-
        protected Vector2 m_MouseLookSmoothMove = Vector2.zero;     // distance moved since last frame (smoothed and accelerated)
        protected Vector2 m_MouseLookRawMove = Vector2.zero;        // distance moved since last frame (raw unity input)
        protected List<Vector2> m_MouseLookSmoothBuffer = new List<Vector2>();
        protected Vector2 m_CurrentMouseLook = Vector2.zero;
        public bool MouseLookInvert = false;
        // mouse cursor
        public Rect[] MouseCursorZones = null;          // screen regions where mouse arrow remains visible when clicking. may be set up in the Inspector
                                                        // NOTE: these do not currently get saved to presets (!)
        public bool MouseCursorForced = false;          // when true, the mouse arrow is enabled all over the screen and firing is disabled
        public bool MouseCursorBlocksMouseLook = true;  // if true, mouselook will be disabled while the mouse arrow is visible

        protected Vector2 m_MousePos = Vector2.zero;    // current mouse position in GUI coordinates (Y flipped)

        // move vector
        protected Vector2 m_MoveVector = Vector2.zero;

        protected FPCharacterEventHandler m_FPPlayer = null;
        public FPCharacterEventHandler FPPlayer
        {
            get
            {
                if (m_FPPlayer == null)
                    m_FPPlayer = transform.root.GetComponentInChildren<FPCharacterEventHandler>();
                return m_FPPlayer;
            }
        }

        /// </summary>
        protected override void OnEnable()
        {
            if (FPPlayer != null)
                FPPlayer.Register(this);
        }
        protected override void OnDisable()
        {
            if (FPPlayer != null)
                FPPlayer.Unregister(this);
        }

        protected override void Update()
        {
            // manage input for GUI
            UpdateCursorLock();

            // toggle pausing and abort if paused
            UpdatePause();
            InputInteract();

            if (FPPlayer.Pause.Get() == true)
                return;

            //// --- NOTE: everything below this line will be disabled on pause! ---

            //if (!m_AllowGameplayInput)
            //    return;

            // interaction

            // manage input for moving
            InputMove();
            InputRun();
            InputJump();
            InputCrouch();

            // manage input for weapons
            InputAttack();
            InputReload();
            InputSetWeapon();

            // manage camera related input
            InputCamera();

        }

        protected virtual void InputInteract()
        {
            if (InputManager.GetButtonDown("Misson"))
            {
                //fp_Utility.LockCursor = false;
                Time.timeScale = 1 - Time.timeScale;
                if (Time.timeScale == 1)
                    PopupMisson.Instance.Close();
                else
                    PopupMisson.Show();
            }
        }

        protected virtual void InputMove()
        {
            FPPlayer.InputMoveVector.Set(new Vector2(InputManager.GetAxisRaw("Horizontal"), InputManager.GetAxisRaw("Vertical")));
        }

        protected virtual void InputRun()
        {

            if (InputManager.GetButton("Run"))
                FPPlayer.Run.TryStart();
            else
                FPPlayer.Run.TryStop();

        }
        protected virtual void InputJump()
        {
            if (InputManager.GetButton("Jump"))
                FPPlayer.Jump.TryStart();
            else
                FPPlayer.Jump.Stop();
        }

        protected virtual void InputCrouch()
        {
            if (InputManager.GetButton("Crouch"))
                FPPlayer.Crouch.TryStart();
            else
                FPPlayer.Crouch.TryStop();
        }

        protected virtual void InputCamera()
        {
            // zoom / ADS
            if (InputManager.GetButton("Zoom"))
                FPPlayer.Zoom.TryStart();
            else
                FPPlayer.Zoom.TryStop();
        }

        protected virtual void InputAttack()
        {
            if (FPPlayer.Run.Active)
                return;

            // if mouse cursor is visible, an extra click is needed
            // before we can attack
            if (!fp_Utility.LockCursor)
                return;

            if (InputManager.GetButton("Attack"))
                FPPlayer.Attack.TryStart();
            else
                FPPlayer.Attack.TryStop();
        }

        protected virtual void InputReload()
        {
            if (InputManager.GetButtonDown("Reload"))
                FPPlayer.Reload.TryStart();
        }

        protected virtual void InputSetWeapon()
        {
            //if (InputManager.GetButtonDown("SetPrevWeapon"))
            //    FPPlayer.SetPrevWeapon.Try();

            // if (InputManager.GetButtonDown("SetNextWeapon"))
            //     FPPlayer.SetNextWeapon.Try();

            if (InputManager.GetButtonDown("SetWeapon1")) FPPlayer.SetWeapon.TryStart(1);
            if (InputManager.GetButtonDown("SetWeapon2")) FPPlayer.SetWeapon.TryStart(2);
            if (InputManager.GetButtonDown("SetWeapon3")) FPPlayer.SetWeapon.TryStart(3);
            if (InputManager.GetButtonDown("SetWeapon4")) FPPlayer.SetWeapon.TryStart(4);
            if (InputManager.GetButtonDown("SetWeapon5")) FPPlayer.SetWeapon.TryStart(5);
            if (InputManager.GetButtonDown("SetWeapon6")) FPPlayer.SetWeapon.TryStart(6);
            if (InputManager.GetButtonDown("SetWeapon7")) FPPlayer.SetWeapon.TryStart(7);
            if (InputManager.GetButtonDown("SetWeapon8")) FPPlayer.SetWeapon.TryStart(8);
            if (InputManager.GetButtonDown("SetWeapon9")) FPPlayer.SetWeapon.TryStart(9);
            if (InputManager.GetButtonDown("SetWeapon10")) FPPlayer.SetWeapon.TryStart(10);

            // --- unwield current weapon by direct button press ---

            if (InputManager.GetButtonDown("ClearWeapon"))
                FPPlayer.SetWeapon.TryStart(0);
        }

        protected virtual void UpdatePause()
        {
            if (InputManager.GetButtonDown("Pause"))
            {
                Time.timeScale = 1 - Time.timeScale;
            }
        }

        protected virtual void UpdateCursorLock()
        {

            // store the current mouse position as GUI coordinates
            m_MousePos.x = Input.mousePosition.x;
            m_MousePos.y = (Screen.height - Input.mousePosition.y);

            // if 'ForceCursor' is active, the cursor will always be visible
            // across the whole screen and firing will be disabled
            if (MouseCursorForced)
            {
                if (fp_Utility.LockCursor)
                    fp_Utility.LockCursor = false;
                return;
            }

            // see if any of the mouse buttons are being held down
            if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))
            {

                // if we have defined mouse cursor zones, check to see if the
                // mouse cursor is inside any of them
                if (MouseCursorZones.Length > 0)
                {
                    foreach (Rect r in MouseCursorZones)
                    {
                        if (r.Contains(m_MousePos))
                        {
                            // mouse is being held down inside a mouse cursor zone, so make
                            // sure the cursor is not locked and don't lock it this frame
                            if (fp_Utility.LockCursor)
                                fp_Utility.LockCursor = false;
                            goto DontLock;
                        }
                    }
                }

                // no zones prevent firing the current weapon. hide mouse cursor
                // and lock it at the center of the screen
                if (!fp_Utility.LockCursor)
                    fp_Utility.LockCursor = true;

            }

        DontLock:

            // if user presses 'ENTER', toggle mouse cursor on / off
            if (InputManager.GetButtonUp("Accept1")
                || InputManager.GetButtonUp("Accept2")
                || InputManager.GetButtonUp("Menu")
                )
            {
#if UNITY_EDITOR && UNITY_5
			if(Input.GetKeyUp(KeyCode.Escape))
				fp_Utility.LockCursor = false;
			else
#endif
                fp_Utility.LockCursor = !fp_Utility.LockCursor;
            }

        }

        protected virtual Vector2 GetMouseLook()
        {
            // don't allow mouselook if we are using the mouse cursor
            if (MouseCursorBlocksMouseLook && !fp_Utility.LockCursor)
                return Vector2.zero;

            // NOTE: this directive addresses an issue with bluetooth gamepads
            // when developing for GearVR. please report if it causes any trouble
#if (!UNITY_ANDROID || (UNITY_ANDROID && UNITY_EDITOR))

            // don't allow mouselook if we are using the mouse cursor
            if (MouseCursorBlocksMouseLook && !fp_Utility.LockCursor)
                return Vector2.zero;

#endif

            // --- fetch mouse input ---

            m_MouseLookSmoothMove.x = InputManager.GetAxisRaw("Mouse X") * Time.timeScale;
            m_MouseLookSmoothMove.y = InputManager.GetAxisRaw("Mouse Y") * Time.timeScale;

            // --- mouse smoothing ---

            // make sure the defined smoothing vars are within range
            MouseLookSmoothSteps = Mathf.Clamp(MouseLookSmoothSteps, 1, 20);
            MouseLookSmoothWeight = Mathf.Clamp01(MouseLookSmoothWeight);

            // keep mousebuffer at a maximum of (MouseSmoothSteps + 1) values
            while (m_MouseLookSmoothBuffer.Count > MouseLookSmoothSteps)
                m_MouseLookSmoothBuffer.RemoveAt(0);

            // add current input to mouse input buffer
            m_MouseLookSmoothBuffer.Add(m_MouseLookSmoothMove);

            // calculate mouse smoothing
            float weight = 1;
            Vector2 average = Vector2.zero;
            float averageTotal = 0.0f;
            for (int i = m_MouseLookSmoothBuffer.Count - 1; i > 0; i--)
            {
                average += m_MouseLookSmoothBuffer[i] * weight;
                averageTotal += (1.0f * weight);
                weight *= (MouseLookSmoothWeight / Delta);
            }

            // store the averaged input value
            averageTotal = Mathf.Max(1, averageTotal);
            m_CurrentMouseLook = fp_MathUtility.NaNSafeVector2(average / averageTotal);

            m_CurrentMouseLook.x *= (MouseLookSensitivity.x);
            m_CurrentMouseLook.y *= (MouseLookSensitivity.y);

            m_CurrentMouseLook.y = (MouseLookInvert ? m_CurrentMouseLook.y : -m_CurrentMouseLook.y);

            return m_CurrentMouseLook;

        }

        protected virtual Vector2 GetMouseLookRaw()
        {
            // TEST: this directive addresses an issue with bluetooth gamepads.
            // please report if it causes any trouble
#if ((!UNITY_ANDROID && !UNITY_IOS) || (UNITY_ANDROID && UNITY_EDITOR) || (UNITY_IOS && UNITY_EDITOR))

            // block mouselook when using the mouse cursor
            if (MouseCursorBlocksMouseLook && !fp_Utility.LockCursor)
                return Vector2.zero;

#endif

            m_MouseLookRawMove.x = InputManager.GetAxisRaw("Mouse X");
            m_MouseLookRawMove.y = InputManager.GetAxisRaw("Mouse Y");

            return m_MouseLookRawMove;

        }
        protected virtual Vector2 OnValue_InputMoveVector
        {
            get { return m_MoveVector; }
            // these platforms always use analog movement
#if UNITY_IOS || UNITY_ANDROID || UNITY_PS3 || UNITY_PS4 || UNITY_XBOXONE
		set	{	m_MoveVector = ((value.sqrMagnitude > 1) ? value.normalized : value);	}
#else
            // platform supports either analog or digital movement
            set
            {
                m_MoveVector = ((value != Vector2.zero) ? value.normalized : value);
            }
#endif
        }

        protected virtual float OnValue_InputClimbVector
        {
            get
            {
                return InputManager.GetAxisRaw("Vertical");
            }
        }


        ///// <summary>
        ///// allows or prevents first person gameplay input. NOTE:
        ///// gui (menu) input is still allowed
        ///// </summary>
        //protected virtual bool OnValue_InputAllowGameplay
        //{
        //	get { return m_AllowGameplayInput; }
        //	set { m_AllowGameplayInput = value; }
        //}


        ///// <summary>
        ///// pauses the game by setting timescale to zero, or unpauses
        ///// it by resuming the timescale that was active upon pause.
        ///// NOTE: will not work in multiplayer
        ///// </summary>
        //protected virtual bool OnValue_Pause
        //{
        //	get { return fp_Gameplay.IsPaused; }
        //	set { fp_Gameplay.IsPaused = value; }
        //}

        protected virtual bool OnMessage_InputGetButton(string button)
        {
            return InputManager.GetButton(button);
        }

        protected virtual bool OnMessage_InputGetButtonUp(string button)
        {
            return InputManager.GetButtonUp(button);
        }

        protected virtual bool OnMessage_InputGetButtonDown(string button)
        {
            return InputManager.GetButtonDown(button);
        }

        protected virtual Vector2 OnValue_InputSmoothLook
        {
            get
            {
                Vector2 ml = GetMouseLook();
                ml.x *= (MouseLookMuteYaw ? 0 : 1);
                ml.y *= (MouseLookMutePitch ? 0 : 1);
                return ml;
            }
        }

        protected virtual Vector2 OnValue_InputRawLook
        {
            get
            {
                Vector2 ml = GetMouseLookRaw();
                ml.x *= (MouseLookMuteYaw ? 0 : 1);
                ml.y *= (MouseLookMutePitch ? 0 : 1);
                return ml;
            }
        }
    }
}
