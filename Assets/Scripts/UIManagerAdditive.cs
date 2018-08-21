using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DoozyUI;

namespace AllNetXR
{
    public enum eUIStateAdditive
    {
        None = -1,
        DirectionIndicator = 0,
        PlayerMarker = 1,
        ErrorMessage = 2,
        Count
    }

    public class UIManagerAdditive : MonoBehaviour
    {
        public static string Category = "Additive";
        public static UIManagerAdditive Instance;
        public static bool DebugMode = true;
        //public Animator stateMachineController;
        private AnimatorStateInfoHelper stateInfoHelper;
        public List<UINotification> uiNotifications;
        public bool IsBusy; // something is showing that is not dismissable

        private Dictionary<string, bool> isShowingDict = new Dictionary<string, bool>();

        // Use this for initialization
        private void Awake()
        {
            Instance = this;
        }

        public void Reset()
        {
           
            foreach (UINotification uiNotify in uiNotifications)
            {
                isShowingDict[uiNotify.gameObject.name] = false;
                uiNotify.gameObject.SetActive(false);
            }
        }

        public void Start()
        {
            Reset();
            //UIStateMachine = StateMachineController.Instance;          
        }

        #region
        void OnEnable()  // Same trigger principal - trigger names start with "On"
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
            //stateInfoHelper = new AnimatorStateInfoHelper(animatorStateInfo, layerIndex, AppStateController.Instance.stateKeys);
            //if (stateInfoHelper.stateName == null) return;

            // Debug.Log("STATE ENTER  =" + stateInfoHelper.stateName);
            //ShowViewFor(stateInfoHelper.stateName);

            //DoozyUI.UIManager.ShowUiElement("Alert2", Category);

          //DoozyUI.UIManager.ShowNotification("Alert3", 1, false, "Darryl", "Darryl's message");
        }

        public void HandleStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            //stateInfoHelper = new AnimatorStateInfoHelper(animatorStateInfo, layerIndex, AppStateController.Instance.stateKeys);
            //if (stateInfoHelper.stateName == null) return;

            //Debug.Log("STATE EXIT =" + stateInfoHelper.stateName);
            //HideViewFor(stateInfoHelper.stateName);
            //DoozyUI.UIManager.HideNotification("Alert3");
        }

        public bool ShowViewFor(string stateName)  // returns error if invalid
        {
            if (stateName == "None")
            {
                Reset();  // clear everything
                return false;
            }

            //if (isShowingDict[stateName] == false) // already on
            //{
            //    DoozyUI.UIManager.ShowUiElement(stateName, Category);

            //}

            //isShowingDict[stateName] = true;

            //return isShowingDict[stateName];
            return true;
        }

        public bool HideViewFor(string stateName)
        {
            if (stateName == "None")
            {
                Reset();
                return false;
            }

           // DoozyUI.UIManager.HideUiElement(stateName, Category);

            return true;
        }

        #endregion

        //private void HandleNoneRequest()
        //{
        //    foreach (eUIStateAdditive aState in System.Enum.GetValues(typeof(eUIStateAdditive)))
        //    {
        //        if (DebugMode) Debug.Log(aState.ToString());
        //        bool result = HideViewForState(aState);
        //    }
        //}

        //public void PerformUIUpdates(string requestedStateName, int direction = 1) // -1 is reverse
        //{
        //    DoozyUI.UIManager.ShowUiElement(requestedStateName, Category);

        //    if (previousStateName != requestedStateName)
        //    {
        //        DoozyUI.UIManager.HideUiElement(previousStateName, Category);
        //    }
        //}

    }
}
