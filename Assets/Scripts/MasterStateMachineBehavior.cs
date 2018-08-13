using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterStateMachineBehaviour : StateMachineBehaviour {

    //private UIManagerSequential UIMgr;
    public bool DebugMode;
    private int BaseLayerIndex = 0;


    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

        //UIMgr = UIManagerSequential.Instance;
        AnimatorStateInfo baseStateInfo = animator.GetCurrentAnimatorStateInfo(BaseLayerIndex);
        //eGameState uiState = GameManager.Instance.Metadata.CurrentGameState;

        //if (baseStateInfo.IsName("Open"))  // as a result of trigger fire i.e. AddPlayerMarker
        //{
        //    //if (DebugMode) Debug.Log("OnStateEnter - Open = " + uiState.ToString());
        //    // UIMgr.OnClipOpenStarted(uiState);
        //    UIMgr.OnClipStarted("Open");
        //}
        Debug.Log("OnStateEnter +" + baseStateInfo);

        //activate state including UI

    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("OnStateExit");
    }

    // OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

}
