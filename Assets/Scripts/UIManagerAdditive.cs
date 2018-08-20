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
        public static string Category = "Example 4 - Simple UI";
        public static UIManagerAdditive Instance;
        public static bool DebugMode = true;
        public Animator stateMachineController;
        private AnimatorStateInfoHelper stateInfoHelper;            
        public List<UIElement> uiElements;
        public bool IsBusy; // something is showing that is not dismissable

        private Dictionary<string, bool> isShowingDict = new Dictionary<string, bool>();

        // Use this for initialization
        private void Awake()
        {
            Instance = this;
        }

        public void Reset()
        {
            // IsAdditiveUIShowing = new List<bool>();
            foreach (UIElement uiElement in uiElements)
            {
                isShowingDict[uiElement.gameObject.name] = false;
                uiElement.gameObject.SetActive(false);
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
            stateInfoHelper = new AnimatorStateInfoHelper(animatorStateInfo, layerIndex);
            if (stateInfoHelper.stateName == null) return;         

            Debug.Log("STATE ENTER  =" + stateInfoHelper.stateName);
            ShowViewFor(stateInfoHelper.stateName);
        }

        public void HandleStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            stateInfoHelper = new AnimatorStateInfoHelper(animatorStateInfo, layerIndex);
            if (stateInfoHelper.stateName == null) return;

            Debug.Log("STATE EXIT =" + stateInfoHelper.stateName);
            HideViewFor(stateInfoHelper.stateName);
        }

        public bool ShowViewFor(string stateName)  // returns error if invalid
        {
            if (stateName == "None")
            {
               Reset();  // clear everything
                return false;
            }

            if (isShowingDict[stateName] == false) // already on
            {
                DoozyUI.UIManager.ShowUiElement(stateName, Category);

            }

            isShowingDict[stateName] = true;

            return isShowingDict[stateName];
        }

        public bool HideViewFor(string stateName)
        {
            if (stateName == "None")
                {
                    Reset();
                    return false;
                }

            DoozyUI.UIManager.HideUiElement(stateName, Category);

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
