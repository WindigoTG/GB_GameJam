using UnityEngine;
using Gamekit2D;

namespace GameJamTeamOne
{
    public class ModifiedAirborneSMB : AirborneSMB
    {
        #region Methods

        public override void OnSLStateNoTransitionUpdate (Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnSLStateNoTransitionUpdate(animator, stateInfo, layerIndex);

            (m_MonoBehaviour as ModifiedPlayerCharacter).SetJetpacking(m_MonoBehaviour.CheckForJumpInput());
        }

        #endregion
    }
}