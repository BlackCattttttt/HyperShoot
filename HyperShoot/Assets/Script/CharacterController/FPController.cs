using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperShoot.Player
{
    public class FPController : BaseController
    {
        private CharacterController m_CharacterController = null;
        public CharacterController characterController
        {
            get
            {
                if (m_CharacterController == null)
                    m_CharacterController = gameObject.GetComponent<CharacterController>();
                return m_CharacterController;
            }
        }


        /// <summary>
        /// sets up various collider dimension variables for dynamic crouch
        /// logic, depending on whether the collider is a capsule or a character
        /// controller
        /// </summary>
        protected override void InitCollider()
        {

            // NOTES:
            // 1) by default, collider width is half the height, with pivot at the feet
            // 2) don't change radius in-game (it may cause missed wall collisions)
            // 3) controller height can never be smaller than radius

            m_NormalHeight = characterController.height;
            characterController.center = m_NormalCenter = (m_NormalHeight * (Vector3.up * 0.5f));
            characterController.radius = m_NormalHeight * DEFAULT_RADIUS_MULTIPLIER;
            //	m_CrouchHeight = m_NormalHeight * PhysicsCrouchHeightModifier;
            //	m_CrouchCenter = m_NormalCenter * PhysicsCrouchHeightModifier;

            //Collider.transform.localPosition = Vector3.zero;

        }


        /// <summary>
        /// updates charactercontroller and physics trigger sizes
        /// depending on player Crouch activity
        /// </summary>
        protected override void RefreshCollider()
        {

            //if (Player.Crouch.Active && !(MotorFreeFly && !Grounded))   // crouching & not flying
            //{
            //	characterController.height = m_NormalHeight * PhysicsCrouchHeightModifier;
            //	characterController.center = m_NormalCenter * PhysicsCrouchHeightModifier;
            //}
            //else    // standing up (whether flying or not)
            //{
            characterController.height = m_NormalHeight;
            characterController.center = m_NormalCenter;
            //		}

        }


        /// <summary>
        /// returns the current step offset
        /// </summary>
        protected virtual float OnValue_StepOffset
        {
            get { return characterController.stepOffset; }
        }


        /// <summary>
        /// returns the current slopeLimit
        /// </summary>
        protected virtual float OnValue_SlopeLimit
        {
            get { return characterController.slopeLimit; }
        }


        /// <summary>
        /// moves the controller by 'direction'. the controller will be affected
        /// by gravity, constrained by collisions and slide along colliders
        /// </summary>
        protected virtual void OnMessage_Move(Vector3 direction)
        {
            if (characterController.enabled)
                characterController.Move(direction);
        }


        /// <summary>
        /// returns the current collider radius
        /// </summary>
        protected override float OnValue_Radius
        {
            get { return characterController.radius; }
        }



        /// <summary>
        /// returns the current collider height
        /// </summary>
        protected override float OnValue_Height
        {
            get { return characterController.height; }
        }

    }
}
