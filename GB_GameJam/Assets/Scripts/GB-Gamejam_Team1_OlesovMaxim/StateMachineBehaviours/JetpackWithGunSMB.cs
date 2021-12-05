using UnityEngine;

namespace GameJamTeamOne
{
    public class JetpackWithGunSMB : JetpackSMB
    {
        #region Methods

        public override void OnSLStatePreExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            AnimatorStateInfo nextState = animator.GetNextAnimatorStateInfo(0);
            if (!nextState.IsTag("WithGun"))
                m_MonoBehaviour.ForceNotHoldingGun();
        }

        #endregion
    }
}