using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using DoozyUI;

// next activates the state controller
namespace AllNetXR
{
    public enum eAppState //DH
    {
        State0,  //splash
        State1,
        State2,
        State3,
        State4,
        Count
    }

    public interface IUINavigationHandler  // required from doozy ui prefab elements
    {
        void OnNextAction();
    }

    public class StateMachineController : MonoBehaviour, IUINavigationHandler
    {
        private static int AppStateHash = Animator.StringToHash("AppStateIndex");
        private static string cStateTriggerPrefix = "On";

        public static bool DebugMode;
        public static Stack<eAppState> StateStack;  // LIFO stack
        public static StateMachineController Instance;
        public static bool IsInitialized;
     
        public Animator animator;
        public LoopSequencer sequencer; //DH - be able to swap in additive or sequential by interface or child class
        public int activeStateIndex;
        public eAppState startState = eAppState.State0;

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
            Instance = this;
         
            Reset();

            ChangeToAppState(startState);  // start with requested state
        }

        public void OnNextAction()
        {
            int nextIndex = sequencer.GetNextIndex(activeStateIndex, (int)eAppState.Count, 0);

            ChangeToAppState((eAppState)nextIndex);
        }

        public void OnPreviousAction()
        {
            int prevIndex = sequencer.GetPreviousIndex(activeStateIndex, (int)eAppState.Count, 0);
            ChangeToAppState((eAppState)prevIndex);
        }

        public void ChangeToAppState(eAppState appState)
        {
            if (appState == eAppState.Count) return;

            string triggerName = cStateTriggerPrefix + appState; Debug.Log(triggerName);

            if (animator != null && animator.isActiveAndEnabled)
            {
                //animator.Play(stateName, 0, percentage);
                animator.SetTrigger(triggerName);
                PerformUIUpdates();
            }

            // LaunchState(appState);
            activeStateIndex = (int)appState;
        }

        public void PerformUIUpdates()
        {
            //DoozyUI.UIManager.ShowUiElement("2"); //if you use the Uncategorized category name
            DoozyUI.UIManager.ShowUiElement("2", "Example 3 - Buttons");
            DoozyUI.UIManager.HideUiElement("1", "Example 3 - Buttons");
        }

        public void ChangeToAppStateWith(int stateId)
        {
            ChangeToAppState((eAppState)stateId);
        }

        void Reset()
        {
            if (StateMachineController.IsInitialized) return;

            foreach (StateToControllerBindings binding in bindings)
            {
                StateController ctrl = binding.stateController;  //shortcut
                if (ctrl == null) continue;

                ctrl.gameObject.SetActive(false);
                if (ctrl.view != null)
                {
                    ctrl.view.SetActive(false);
                }
            }

            StateMachineController.IsInitialized = true;
        }


        #region Callback handling
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
        public void HandleStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            EasyStateInfo easyStateInfo = new StateInfoHelper().GetFriendlyStateInfo(animatorStateInfo);
            //StateInfo stateInfo = stateInfoHelper.GetFriendlyStateInfo(stateInfo); 
            Debug.Log("STATE ENTER  =" + stateInfo.stateName);

            animator.SetInteger("AppStateIndex", stateInfo.stateIndex);
            StateController controller = bindings[stateInfo.stateIndex].stateController;
            //controller.gameObject.SetActive(true);
            controller.Begin();
        }

        public void HandleStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            //Debug.Log("STATE EXIT =" + SetFriendlyInfoFrom(stateInfo).stateName);
            //Reset();  // turn off old state controller

            //controller.gameObject.SetActive(false);
            // StateController controller = bindings[(int)previousState].stateController;
            StateController controller = bindings[friendlyStateInfo.stateIndex].stateController;          
            controller.End();
        }

        void OnGUI()
        {
            //Output the current Animation name and length to the screen
            GUI.Label(new Rect(0, 0, 200, 20), "Clip Name:" + friendlyStateInfo.stateName);
            GUI.Label(new Rect(0, 30, 200, 20), "Clip Length: " + friendlyStateInfo.duration);
        }
        #endregion
    }
}