using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AllNetXR
{

    public class SmbEventDispatcher : StateMachineBehaviour  // attach to every state in the animator
    {
        public delegate void StateEventHandler(Animator animator, AnimatorStateInfo stateInfo, int layerIndex);

        public static event StateEventHandler OnStateEntered;
        public static event StateEventHandler OnStateExited;

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            //AnimatorStateInfo baseStateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);  //Might need to activate
            if (OnStateEntered != null)  //does not see listeners
            {
                OnStateEntered(animator, stateInfo, layerIndex);
            }
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);

            if (OnStateExited != null)
            {
                OnStateExited(animator, stateInfo, layerIndex);
            }
        }
    }
}
