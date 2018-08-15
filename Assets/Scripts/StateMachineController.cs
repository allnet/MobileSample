using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Talespin;
using UnityEngine.EventSystems;


// next activates the state controller
namespace AllNetXR
{

    public enum eAppState //DH
    {
        None = -1,
        State0 = 0,  //splash
        State1 = 1,
        State2 = 2,
        State3 = 3,
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

        [SerializeField]
        protected eAppState activeState;
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

      
       // StateController activeController;

        // ==================================================================
        void Awake()
        {
            Instance = this;
           /// animator = GetRequiredComponent<Animator>();  // // must be hooked up in inspector
                      
            friendlyStateInfo = new FriendlyStateInfo(stateName: "", duration: 0, stateIndex: 0);

            Reset();
            ChangeToAppState(requestedState);  // start with requested state
        }

        private T GetRequiredComponent<T>()
        {
            throw new NotImplementedException();
        }

        public void OnNextAction()
        {
            ChangeToAppState(sequencer.GetNextStateFor(activeState));
        }

        public void OnPreviousAction()
        {
            ChangeToAppState(sequencer.GetPreviousStateFor(activeState));
        }

        public virtual void ChangeToAppState(eAppState appState)
        {
            string triggerName = "On" + appState;  Debug.Log(triggerName);
            animator.SetTrigger(triggerName);
           // LaunchState(appState);
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