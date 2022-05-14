using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperShoot.Player
{
    public class CharacterEventHandler : fp_StateEventHandler
    {
        // player type
        public fp_Value<bool> IsFirstPerson;    // always returns true if this a local player in 1st person mode, false if 3rd person mode or multiplayer remote player or AI
        public fp_Value<bool> IsLocal;          // returns true if a fp_FPCamera is present on this player
        public fp_Value<bool> IsAI;             // should return true if this player is controlled by an AI script

        // health
        public fp_Value<float> Health;
        public fp_Value<float> MaxHealth;

        // position and rotation
        public fp_Value<Vector3> Position;
        public fp_Value<Vector2> Rotation;      // world XY (pitch, yaw) rotation of head
        public fp_Value<float> BodyYaw;         // world Y (yaw) rotation of lower body

        // headlook
        public fp_Value<Vector3> LookPoint;
        public fp_Value<Vector3> HeadLookDirection;     // head forward vector. NOTE: this will be different from 'CameraLookDirection' in 3rd person
        public fp_Value<Vector3> AimDirection;          // direction between weapon and lookpoint. NOTE: this is different from direction between head and lookpoint

        // motor
        public fp_Value<Vector3> MotorThrottle;
        public fp_Value<bool> MotorJumpDone;

        // input
        public fp_Value<Vector2> InputMoveVector;
        public fp_Value<float> InputClimbVector;

        // activities
        public fp_Activity Dead;
        public fp_Activity Run;
        public fp_Activity Jump;
        public fp_Activity Crouch;
        public fp_Activity Zoom;
        public fp_Activity Attack;
        public fp_Activity Reload;
        //public fp_Activity Climb;
        //public fp_Activity Interact;
        public fp_Activity<int> SetWeapon;
        public fp_Activity OutOfControl;

        // weapon object events
        public fp_Message<int> Wield;
        public fp_Message Unwield;
        public fp_Attempt Fire;
        public fp_Message DryFire;

        //// weapon handler events
        //public fp_Attempt SetPrevWeapon;
        //public fp_Attempt SetNextWeapon;
        //public fp_Attempt<string> SetWeaponByName;
        //public fp_Value<int> CurrentWeaponID;   // renamed to avoid confusion with fp_ItemType.ID
        public fp_Value<int> CurrentWeaponIndex;
        //public fp_Value<string> CurrentWeaponName;
        public fp_Value<bool> CurrentWeaponWielded;
        public fp_Attempt AutoReload;
        public fp_Value<float> CurrentWeaponReloadDuration;

        //// inventory
        //public fp_Message<string, int> GetItemCount;
        public fp_Attempt RefillCurrentWeapon;
        public fp_Value<int> CurrentWeaponAmmoCount;
        public fp_Value<int> CurrentWeaponMaxAmmoCount;
        public fp_Value<int> CurrentWeaponClipCount;
        //public fp_Value<int> CurrentWeaponType;
        //public fp_Value<int> CurrentWeaponGrip;
        //public fp_Attempt<object> AddItem;
        //public fp_Attempt<object> RemoveItem;
        public fp_Attempt DepleteAmmo;

        // physics
        public fp_Message<Vector3> Move;
        public fp_Value<Vector3> Velocity;
        public fp_Value<float> SlopeLimit;
        public fp_Value<float> StepOffset;
        public fp_Value<float> Radius;
        public fp_Value<float> Height;
        public fp_Value<float> FallSpeed;
        public fp_Message<float> FallImpact;
        public fp_Message<float> HeadImpact;
        public fp_Message<Vector3> ForceImpact;
        public fp_Message Stop;
        public fp_Value<Transform> Platform;
        public fp_Value<Vector3> PositionOnPlatform;
        public fp_Value<bool> Grounded;

        // interaction
        //public fp_Value<fp_Interactable> Interactable;
        public fp_Value<bool> CanInteract;

        protected override void Awake()
        {

            base.Awake();

            BindStateToActivity(Run);
            BindStateToActivity(Jump);
            BindStateToActivity(Crouch);
            BindStateToActivity(Zoom);
            BindStateToActivity(Reload);
            BindStateToActivity(Dead);
            //BindStateToActivity(Climb);
            BindStateToActivity(OutOfControl);
            BindStateToActivityOnStart(Attack);

            SetWeapon.AutoDuration = 1.0f;
            Reload.AutoDuration = 1.0f; 

            Zoom.MinDuration = 0.2f;
            Crouch.MinDuration = 0.5f;

            Jump.MinPause = 0.0f;           // increase this to enforce a certain delay between jumps
            SetWeapon.MinPause = 0.2f;

        }
    }
}
