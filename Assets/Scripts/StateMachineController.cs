using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using DoozyUI;
using System.Linq;

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
        private string[] stateKeys;
        public string activeStateName, previousStateName;
        public string startState;
        public StateController activeController;

        public Dictionary<string, Transform> stateControllers = new Dictionary<string, Transform>();

        // Static - DH - extension possibly        
        private Transform[] GetTopLevelChildren(Transform Parent)
        {
            Transform[] Children = new Transform[Parent.childCount];
            for (int ID = 0; ID < Parent.childCount; ID++)
            {
                Children[ID] = Parent.GetChild(ID);
            }
            return Children;
        }

        private void SetControllersToChildren(Transform parent) //DH
        {
            Transform[] transforms = GetTopLevelChildren(parent);
            foreach (Transform t in transforms)
            {
                stateControllers[t.gameObject.name] = t;
                //AddOrUpdate(stateC)
                t.gameObject.SetActive(false);
            }
            SetStateControllerSpecifics();
        }

        private void SetStateControllerSpecifics()
        {
            stateKeys = new string[stateControllers.Count];
            stateControllers.Keys.CopyTo(stateKeys, 0);
        }

        void Awake()
        {
            Instance = this;

            SetControllersToChildren(this.transform); // places in Controllers
            startState = stateControllers.Keys.First();  // get first child controller
        }

        void Start()
        {
            ChangeToAppState(startState);
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

        private bool TryToSetActiveController(AnimatorStateInfo animatorStateInfo)
        {
            stateInfoHelper = new AnimatorStateInfoHelper(animatorStateInfo);
            if (!stateControllers.ContainsKey(key: stateInfoHelper.stateName))
            {
                Debug.Log("< INVALID: State and GameObject name mismatch >");
                return false;
            }

            Transform t = stateControllers[stateInfoHelper.stateName] as Transform;
            activeController = t.gameObject.GetComponent<StateController>();

            return true;
        }
        public void HandleStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            if (!TryToSetActiveController(animatorStateInfo)) return;
            
            Debug.Log("STATE ENTER  =" + stateInfoHelper.stateName);
            activeController.Begin();
        }

        public void HandleStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            if (!TryToSetActiveController(animatorStateInfo)) return;

            Debug.Log("STATE EXIT =" + stateInfoHelper.stateName);
            activeController.End();         
        }

        void OnGUI()
        {
            //Output the current Animation name and length to the screen
            if (stateInfoHelper != null)
            {
                GUI.Label(new Rect(0, 0, 200, 20), "Clip Name:" + stateInfoHelper.stateName);
                GUI.Label(new Rect(0, 30, 200, 20), "Clip Length: " + stateInfoHelper.duration);
            }
        }
        #endregion

        public int GetCurrentIndex()
        {
            return Array.FindIndex(stateKeys, s => s == activeStateName);
        }

        public void OnNextAction()
        {
            int nextIndex = sequencer.GetNextIndex(GetCurrentIndex(), stateKeys.Length, 0);
            ChangeToAppState(stateKeys[nextIndex]);
        }

        public void OnPreviousAction()
        {
            int previousIndex = sequencer.GetPreviousIndex(GetCurrentIndex(), stateKeys.Length, 0);
            ChangeToAppState(stateKeys[previousIndex]);
        }

        public void ChangeToAppState(string aRequestedState, int direction = 1)  // -1 for reverse
        {
            if (!stateControllers.ContainsKey(key: aRequestedState))
            {
                Debug.Log("< INVALID: State and GameObject name mismatch >");
                return;
            }

            string triggerName = cStateTriggerPrefix + aRequestedState; Debug.Log(triggerName);
            if (animator != null && animator.isActiveAndEnabled)
            {
                //animator.Play(stateName, 0, percentage);             
                animator.SetTrigger(triggerName);
            }

            previousStateName = activeStateName;
            activeStateName = aRequestedState;

            //NOTE: uncomment this for automatic progression without state controllers
            //PerformUIUpdates(activeStateName, direction); // one approach otherwise state controlelrs
        }

        public void PerformUIUpdates(string requestedStateName, int direction = 1) // -1 is reverse
        {
            DoozyUI.UIManager.ShowUiElement(requestedStateName, Category);

            if (previousStateName != requestedStateName)
            {
                DoozyUI.UIManager.HideUiElement(previousStateName, Category);
            }
        }

        //void addOrUpdate(Dictionary<int, int> dic, int key, int newValue)
        //{
        //    int val;
        //    if (dic.TryGetValue(key, out val))
        //    {
        //        // yay, value exists!
        //        dic[key] = val + newValue;
        //    }
        //    else
        //    {
        //        // darn, lets add the value
        //        dic.Add(key, newValue);
        //    }
        //}
    }
}