using UnityEngine;
using Gamekit2D;

namespace GameJamTeamOne
{
    public class JetpackSMB : SceneLinkedSMB<PlayerCharacter>
    {
        #region Methods

        public override void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var mb = m_MonoBehaviour as DerivedPlayerCharacter;

            mb.StartJetpacking();
        }

        public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var playerCharacter = m_MonoBehaviour as DerivedPlayerCharacter;

            playerCharacter.UpdateFacing();
            playerCharacter.UpdateJetstreamPosition();

            if (playerCharacter.CheckIfJetpackIsInUse())
            {
                if (playerCharacter.JetpackType == JetpackType.UpwardThrust)
                    ExecuteThrustBehaviour(playerCharacter);
                else
                    ExecuteFreeflightBehaviour(playerCharacter);
            }
            else
            {
                playerCharacter.AirborneHorizontalMovement();
                playerCharacter.AirborneVerticalMovement();
            }
            playerCharacter.CheckForGroundedWithJetpack();

            m_MonoBehaviour.CheckForHoldingGun();
            m_MonoBehaviour.CheckAndFireGun();
        }

        private void ExecuteThrustBehaviour(DerivedPlayerCharacter playerCharacter)
        {
            playerCharacter.JetpackHorizontalMovement();
            playerCharacter.JetpackVerticalMovement();
        }

        private void ExecuteFreeflightBehaviour(DerivedPlayerCharacter playerCharacter)
        {
            playerCharacter.JetpackFreeflightMovement();
        }

        public override void OnSLStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            AnimatorStateInfo newtState = animator.GetCurrentAnimatorStateInfo(0);

            if (newtState.IsTag("Damaged"))
                (m_MonoBehaviour as DerivedPlayerCharacter).StopJetpacking();
        }

        #endregion
    }
}