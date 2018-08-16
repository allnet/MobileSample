using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.EventSystems;


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

        public string cStateParmKey = "AppStateIndex";
        public string cStateTriggerPrefix = "On";

        public Animator animator;
        public UIManagerSequential sequencer; //DH - be able to swap in additive or sequential by interface or child class

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

        private int stateHash;
        private eAppState _activeState;
        public eAppState ActiveState { get; set; }           
               //{
            //get
            //{
            //   // return (eAppState)animator.GetInteger(stateParmKey);

            //}
        //}

        [SerializeField]      
        private eAppState requestedState, previousState;

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
            stateHash = Animator.StringToHash(cStateParmKey);
            
            Reset();
            ChangeToAppState(requestedState);  // start with requested state
        }

        public void OnNextAction()
        {
            ChangeToAppState(sequencer.GetNextStateFor(ActiveState));
        }

        public void OnPreviousAction()
        {
            ChangeToAppState(sequencer.GetPreviousStateFor(ActiveState));
        }

        public virtual void ChangeToAppState(eAppState appState)
        {
            string triggerName = cStateTriggerPrefix + appState; Debug.Log(triggerName);
            animator.SetTrigger(triggerName);
            // LaunchState(appState);
            ActiveState = appState;
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