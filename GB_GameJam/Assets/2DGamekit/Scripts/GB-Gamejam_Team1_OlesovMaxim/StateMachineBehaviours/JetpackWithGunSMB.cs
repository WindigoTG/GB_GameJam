using UnityEngine;
using Gamekit2D;

namespace GameJamTeamOne
{
    public class JetpackWithGunSMB : SceneLinkedSMB<PlayerCharacter>
    {
        #region Methods

        public override void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var mb = m_MonoBehaviour as ModifiedPlayerCharacter;

            mb.StartJetpacking();
        }

        public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var playerCharacter = m_MonoBehaviour as ModifiedPlayerCharacter;

            playerCharacter.UpdateFacing();

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

        private void ExecuteThrustBehaviour(ModifiedPlayerCharacter playerCharacter)
        {
            playerCharacter.JetpackHorizontalMovement();
            playerCharacter.JetpackVerticalMovement();
        }

        private void ExecuteFreeflightBehaviour(ModifiedPlayerCharacter playerCharacter)
        {
            playerCharacter.JetpackFreeflightMovement();
        }

        public override void OnSLStatePreExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            AnimatorStateInfo nextState = animator.GetNextAnimatorStateInfo(0);
            if (!nextState.IsTag("WithGun"))
                m_MonoBehaviour.ForceNotHoldingGun();

            if (nextState.IsTag("Damaged"))
                (m_MonoBehaviour as ModifiedPlayerCharacter).StopJetpacking();
        }

        #endregion
    }
}