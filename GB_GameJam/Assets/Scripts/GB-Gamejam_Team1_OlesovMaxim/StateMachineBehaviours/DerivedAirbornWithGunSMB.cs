using UnityEngine;
using Gamekit2D;

namespace GameJamTeamOne
{
    public class DerivedAirbornWithGunSMB : AirborneWithGunSMB
    {
        public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnSLStateNoTransitionUpdate(animator, stateInfo, layerIndex);

            (m_MonoBehaviour as DerivedPlayerCharacter).CheckForJetpack();
        }
    }
}