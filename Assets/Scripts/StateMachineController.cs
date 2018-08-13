using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Talespin;
using UnityEngine.EventSystems;

// next activates the state controller
namespace Evgo
{
    public class StateMachineController : MonoBehaviour
    {
        private Animator animator;

        public eAppState requestedState; //prop
        public eAppState currentState;  // prop

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


        [System.Serializable]
        public struct StateToControllerBindings
        {
            public eAppState gameState;
            public StateController StateController; //UI

            public StateToControllerBindings(eAppState gameState = eAppState.State0, StateController stateController = null)
            {
                this.gameState = gameState;
                this.StateController = stateController;
            }
        }
        [Header("Animator State to Controller Bindings")]
        public StateToControllerBindings[] Bindings;

        // ===================================================================]

        void Awake()
        {
            animator = GetComponent<Animator>();
            Reset();
        }

        void Reset()
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

            foreach (eAppState enumVal in Enum.GetValues(typeof(eAppState)))
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