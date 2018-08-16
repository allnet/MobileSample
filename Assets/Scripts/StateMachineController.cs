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

    public interface IUINavigationHandler
    {
        void OnNextAction();
    }

    public class StateMachineController : MonoBehaviour, IUINavigationHandler
    {
        public static Stack<eAppState> StateStack;  // LIFO stack
        public static StateMachineController Instance;
        public static bool IsInitialized;

        private string cStateTriggerPrefix = "On";

        public Animator animator;
        public LoopSequencer sequencer; //DH - be able to swap in additive or sequential by interface or child class

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

        private static int AppStateHash = Animator.StringToHash("AppStateIndex");
        //private eAppState _activeState;
        //public eAppState ActiveState { get; set; }         // return (eAppState)animator.GetInteger(stateParmKey);
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
            // animator = GetRequiredComponent<Animator>();  // // must be hooked up in inspector

            friendlyStateInfo = new FriendlyStateInfo(stateName: "", duration: 0, stateIndex: 0);

            Reset();

            ChangeToAppState(startState);  // start with requested state
        }

        public void OnTestAction()
        {
            DoozyUI.UIManager.ShowUiElement("YourElementName"); //if you use the Uncategorized category name
            DoozyUI.UIManager.ShowUiElement("YourElementName", "YourElementCategoryName");
            // UIManager.ShowUiElement("YourElementName", "YourElementCategoryName", instantAction); 
            //instantActions tells the animation to happen in zero seconds (if true) and normally (otherwise)

            DoozyUI.UIManager.HideUiElement("YourElementName"); //if you use the Uncategorized category name
            DoozyUI.UIManager.HideUiElement("YourElementName", "YourElementCategoryName");
            //UIManager.HideUIElement("YourElementName", "YourElementCategoryName", instantAction);                                 
            //instantActions tells the animation to happen in zero seconds (if true) and normally (otherwise)
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