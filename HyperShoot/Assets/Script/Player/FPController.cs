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

        protected override void InitCollider()
        {
            m_NormalHeight = characterController.height;
            characterController.center = m_NormalCenter = (m_NormalHeight * (Vector3.up * 0.5f));
            characterController.radius = m_NormalHeight * DEFAULT_RADIUS_MULTIPLIER;
            m_CrouchHeight = m_NormalHeight * PhysicsCrouchHeightModifier;
            m_CrouchCenter = m_NormalCenter * PhysicsCrouchHeightModifier;
        }

        protected override void RefreshCollider()
        {
            if (Player.Crouch.Active)   // crouching
            {
                characterController.height = m_NormalHeight * PhysicsCrouchHeightModifier;
                characterController.center = m_NormalCenter * PhysicsCrouchHeightModifier;
            }
            else    // standing up 
            {
                characterController.height = m_NormalHeight;
                characterController.center = m_NormalCenter;
            }
        }

        protected virtual float OnValue_StepOffset
        {
            get { return characterController.stepOffset; }
        }

        protected virtual float OnValue_SlopeLimit
        {
            get { return characterController.slopeLimit; }
        }

        protected virtual void OnMessage_Move(Vector3 direction)
        {
            if (characterController.enabled)
                characterController.Move(direction);
        }

        protected override float OnValue_Radius
        {
            get { return characterController.radius; }
        }

        protected override float OnValue_Height
        {
            get { return characterController.height; }
        }
    }
}
