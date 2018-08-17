using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using DoozyUI;

// next activates the state controller
namespace AllNetXR
{
    public enum eAppState // Essential also start of class name
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

        public static String Category = "Example 3 - Buttons";
        public static bool DebugMode;
        public static Stack<eAppState> StateStack;  // LIFO stack
        public static StateMachineController Instance;
        public static bool IsInitialized;

        public Animator animator;
        public LoopSequencer sequencer; //DH - be able to swap in additive or sequential by interface or child class

        private AnimatorStateInfoHelper stateInfoHelper;
        public int activeStateIndex, previousStateIndex;
        public StateController activeController;
        public eAppState startState = eAppState.State0;

        public Dictionary<string, Transform> stateControllers = new Dictionary<string, Transform>();

        // Static - DH - extension possibly        
        public Transform[] GetTopLevelChildren(Transform Parent)
        {
            Transform[] Children = new Transform[Parent.childCount];
            for (int ID = 0; ID < Parent.childCount; ID++)
            {
                Children[ID] = Parent.GetChild(ID);
            }
            return Children;
        }

        public void SetControllersToChildren(Transform parent) //DH
        {
            Transform[] transforms = GetTopLevelChildren(parent);
            foreach (Transform t in transforms)
            {
                stateControllers[t.gameObject.name] = t;
                //AddOrUpdate(stateC)
                t.gameObject.SetActive(false);
            }
        }

        void addOrUpdate(Dictionary<int, int> dic, int key, int newValue)
        {
            int val;
            if (dic.TryGetValue(key, out val))
            {
                // yay, value exists!
                dic[key] = val + newValue;
            }
            else
            {
                // darn, lets add the value
                dic.Add(key, newValue);
            }
        }

        void Awake()
        {
            Instance = this;

            SetControllersToChildren(this.transform); // places in Controllers

            //Reset();
            ChangeToAppState(startState);  // start with requested state
        }
        
        #region Callback Handling
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

        public void HandleStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            stateInfoHelper = new AnimatorStateInfoHelper(animatorStateInfo);
                            // animator.SetInteger("AppStateIndex", stateInfoHelper.stateIndex);  
            Debug.Log("STATE ENTER  =" + stateInfoHelper.stateName);

            //StateController controller = bindings[stateInfoHelper.stateIndex].stateController;           
            //controller.gameObject.SetActive(true);
            //StateController controller = GetComponent(baseVal + stateInfoHelper.stateIndex.ToString()) as StateController;
            Transform t = stateControllers[stateInfoHelper.stateName] as Transform;
          
            activeController = t.gameObject.GetComponent <StateController>();
            //activeController.gameObject.SetActive(true);
            activeController.Begin();
        }

        public void HandleStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            stateInfoHelper = new AnimatorStateInfoHelper(animatorStateInfo);
            Debug.Log("STATE EXIT =" + stateInfoHelper.stateName);
            //StateController controller = bindings[stateInfoHelper.stateIndex].stateController;
            //activeController.gameObject.SetActive(false);
            //.activeController.End();
            Transform t = stateControllers[stateInfoHelper.stateName] as Transform;
            activeController = t.gameObject.GetComponent<StateController>();
            //activeController.gameObject.SetActive(true);
            activeController.End();

        }

        void OnGUI()
        {
            //Output the current Animation name and length to the screen
            GUI.Label(new Rect(0, 0, 200, 20), "Clip Name:" + stateInfoHelper.stateName);
            GUI.Label(new Rect(0, 30, 200, 20), "Clip Length: " + stateInfoHelper.duration);
        }
        #endregion

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

        public void ChangeToAppState(eAppState appState, int direction = 1)  // -1 for reverse
        {
            previousStateIndex = (stateInfoHelper != null) ? stateInfoHelper.stateIndex : (int)eAppState.State0;

            if (appState == eAppState.Count) return;

            string triggerName = cStateTriggerPrefix + appState; Debug.Log(triggerName);

            if (animator != null && animator.isActiveAndEnabled)
            {
                //animator.Play(stateName, 0, percentage);
                PerformUIUpdates(appState, direction);
                animator.SetTrigger(triggerName);
                previousStateIndex = activeStateIndex;
                activeStateIndex = (int)appState;
            }
        }

        public void PerformUIUpdates(eAppState appStateRequested, int direction = 1) // -1 is reverse
        {
            string showElementForState = ((int)appStateRequested).ToString();
            DoozyUI.UIManager.ShowUiElement(showElementForState, Category);

            if (previousStateIndex != (int)appStateRequested)
            {
                string hideElementForState = ((int)previousStateIndex).ToString();
                DoozyUI.UIManager.HideUiElement(hideElementForState, Category);
            }
        }

        public void ChangeToAppStateWith(int stateId)
        {
            ChangeToAppState((eAppState)stateId);
        }

        //void Reset()
        //{
        //    if (!StateMachineController.IsInitialized) return;

        //    foreach (stateControllers con in bindings)
        //    {
        //        StateController ctrl = binding.stateController;  //shortcut
        //        if (ctrl == null) continue;

        //        ctrl.gameObject.SetActive(false);
        //        if (ctrl.view != null)
        //        {
        //            ctrl.view.SetActive(false);
        //        }
        //    }

        //    StateMachineController.IsInitialized = true;
        //}


        // BINDINGS LOGIC

        //[System.Serializable]
        //public struct StateToControllerBindings
        //{
        //    public eAppState appState;
        //    public StateController stateController; //UI

        //    public StateToControllerBindings(eAppState appState = eAppState.State0, StateController stateController = null)
        //    {
        //        this.appState = appState;
        //        this.stateController = stateController;
        //    }
        //}
        //[Header("Animator State to Controller Bindings")]
        //public StateToControllerBindings[] bindings;


        //void Reset()
        //{
        //    if (StateMachineController.IsInitialized) return;

        //    foreach (StateToControllerBindings binding in bindings)
        //    {
        //        StateController ctrl = binding.stateController;  //shortcut
        //        if (ctrl == null) continue;

        //        ctrl.gameObject.SetActive(false);
        //        if (ctrl.view != null)
        //        {
        //            ctrl.view.SetActive(false);
        //        }
        //    }

        //    StateMachineController.IsInitialized = true;
        //}

    }
}