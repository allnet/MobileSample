using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Talespin;
using UnityEngine.EventSystems;

// next activates the state controller
namespace AllNetXR
{
    public class StateMachineController : MonoBehaviour
    {
        public static StateMachineController instance;
        public static bool isInitialized;
        private Animator animator;
   
        public eAppState requestedState, activeState, previousState;
        StateController activeController;

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
            public eAppState appState;
            public StateController stateController; //UI

            public StateToControllerBindings(eAppState appState = eAppState.State0, StateController stateController = null)
            {
                this.appState = appState;
                this.stateController = stateController;
            }
        }
        [Header("Animator State to Controller Bindings")]
        public StateToControllerBindings[] bindings;

        // ==================================================================
        void Awake()
        {
            instance = this;
            animator = GetComponent<Animator>();
            friendlyStateInfo = new FriendlyStateInfo(stateName: "", duration: 0, stateIndex: 0);

            Reset();
            ChangeToAppState(requestedState);  // start with requested state
        }

        void ChangeToAppState(eAppState appState)
        {
            animator.SetTrigger("On" + appState);
           // LaunchState(appState);
        } 

        //void LaunchState(eAppState appState)
        //{
        //    StateController controller = bindings[(int)appState].stateController;
        //    //controller.gameObject.SetActive(true);
        //    controller.Begin();
        //}

        void Reset()
        {
            if (StateMachineController.isInitialized) return;

            foreach (StateToControllerBindings binding in bindings)
            {
                binding.stateController.gameObject.SetActive(false);
            }
            StateMachineController.isInitialized = true;
        }

        void OnEnable()
        {
            SmbEventDispatcher.OnStateEntered += HandleStateEnter;
            SmbEventDispatcher.OnStateExited += HandleStateExit;
        }

        private void OnDisable()
        {
            SmbEventDispatcher.OnStateEntered -= HandleStateEnter;
            SmbEventDispatcher.OnStateExited -= HandleStateExit;
        }

        // ======================================================================== State Machine Callbacks
        public void HandleStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            SetFriendlyInfoFrom(stateInfo); Debug.Log("STATE ENTER  =" + friendlyStateInfo.stateName);
          
            animator.SetInteger("AppStateIndex", friendlyStateInfo.stateIndex);
         
            StateController controller = bindings[friendlyStateInfo.stateIndex].stateController;
           //controller.gameObject.SetActive(true);
            controller.Begin();
            activeState =  (eAppState)friendlyStateInfo.stateIndex;
        }

        public void HandleStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {

            Debug.Log("STATE EXIT =" + SetFriendlyInfoFrom(stateInfo).stateName);
            //Reset();  // turn off old state controller

            //controller.gameObject.SetActive(false);
           StateController controller = bindings[friendlyStateInfo.stateIndex].stateController;
           // StateController controller = bindings[(int)previousState].stateController;
            controller.End();
        }

        protected FriendlyStateInfo SetFriendlyInfoFrom(AnimatorStateInfo stateInfo)  // clip name 
        {
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

            return friendlyStateInfo;
        }

        void OnGUI()
        {
            //Output the current Animation name and length to the screen
            GUI.Label(new Rect(0, 0, 200, 20), "Clip Name:" + friendlyStateInfo.stateName);
            GUI.Label(new Rect(0, 30, 200, 20), "Clip Length: " + friendlyStateInfo.duration);
        }
    }
}