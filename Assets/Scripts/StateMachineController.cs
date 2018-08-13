using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;

// next activates the state controller
namespace Evgo
{
    public class StateMachineController : MonoBehaviour
    {
        private Animator animator;
       //private SmbHandler smbHandler;
        public List<StateController> stateControllers;

        public eGameState requestedState; //prop
        public eGameState currentState;  // prop

        public bool DebugMode;    
        public struct FriendlyStateInfo
        {
            public string stateName;
            public float duration;
            public int stateIndex;

           public FriendlyStateInfo(string stateName, float duration, int stateIndex)
            {
                this.stateName = stateName;
                this.duration = duration;
                this.stateIndex = stateIndex;
            } 
        }
        public FriendlyStateInfo friendlyStateInfo;

        void Awake()
        {
            animator = GetComponent<Animator>();
            // smbHandler = animator.GetBehaviour<SmbHandler>();
           
        }

        private void Reset()
        {
            friendlyStateInfo = new FriendlyStateInfo(stateName: "", duration: 0, stateIndex: 0);
        }

        void OnEnable()
        {
            SmbHandler.OnStateEntered += HandleStateEnter;
            SmbHandler.OnStateExited += HandleStateExit;
        }

        private void OnDisable()
        {
            SmbHandler.OnStateEntered -= HandleStateEnter;
            SmbHandler.OnStateExited -= HandleStateExit;
        }

        // ======================================================================== State Machine Callbacks
        public void HandleStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //UIMgr = UIManagerAdditive.Instance;
            //eUIStateAdditive stateEntered = (eUIStateAdditive)layerIndex - NumberOfLayersToIgnore;
            //AnimatorStateInfo baseStateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);

            Debug.Log("STATE ENTER  =" + GetStateNameFrom(stateInfo));
            // turn on new state controller based on index or dictionary
            animator.SetInteger("AppStateIndex", friendlyStateInfo.stateIndex);
        }

        public void HandleStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            Debug.Log("STATE EXIT =" + GetStateNameFrom(stateInfo));
                      
            //Reset();  // turn off old state controller
        }

        //====================================================     
        protected string GetStateNameFrom(AnimatorStateInfo stateInfo)  // clip name 
        {
            Reset();

            friendlyStateInfo.duration = stateInfo.length;
                  
            foreach (eGameState enumVal in Enum.GetValues(typeof(eGameState)))
            {
                friendlyStateInfo.stateName = (enumVal.ToString() == "Count") ? "< State Mismatch >" : enumVal.ToString();
                //Debug.Log("search val = " + searchVal);
                if (stateInfo.IsName(friendlyStateInfo.stateName))
                {
                    friendlyStateInfo.stateIndex = (int)enumVal;
                    break;
                }
            }         

            return friendlyStateInfo.stateName;
        }

        void OnGUI()
        {
            //Output the current Animation name and length to the screen
            GUI.Label(new Rect(0, 0, 200, 20), "Clip Name:" + friendlyStateInfo.stateName);
            GUI.Label(new Rect(0, 30, 200, 20), "Clip Length: " + friendlyStateInfo.duration);
        }
    }
}