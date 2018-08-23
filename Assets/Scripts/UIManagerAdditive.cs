using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DoozyUI;

namespace AllNetXR
{
    public enum eNotificationType
    {
        None,
        Busy,
        Info,  // single button
        Warning,
        Choice2Way,
        Choice3Way,
        Count
    }

    public enum eResponseType
    {
        Ok,  // sends global ok and whatever is waiting continues
        Cancel,  // sends global cancel
        Retry  // Resets 
    }

    public class UIManagerAdditive : MonoBehaviour
    {
        public static UIManagerAdditive Instance;
        public static string Category = "Additive";
        public static bool DebugMode = true;

        public delegate void ResponseHandler(eNotificationType notificationType, eResponseType responseType, string input);
        public static event ResponseHandler responseHandler;  // anybody can sign up then relinquish

        private AnimatorStateInfoHelper stateInfoHelper;
        public bool IsBusy; // something is showing that is not dismissable

        // BINDINGS LOGIC
        [System.Serializable]
        public struct NotificationBinding
        {
            public GameObject notification; // uses name to invoke so they must match 
            [HideInInspector] public string title;  //  [HideInInspector]
            public bool isDismissable;
            public bool isShowing;

            public NotificationBinding(GameObject notification, bool isDismissable = false, bool isShowing = false)
            {
                this.notification = notification;
                this.title = notification.name;
                this.isDismissable = isDismissable;
                this.isShowing = isShowing;
            }
        }
        [Header("Notification Bindings")]
        public NotificationBinding[] notificationBindings;
        private List<string> notificationTitles = new List<string>();


        // Use this for initialization
        void Awake()
        {
            Instance = this;

            //Initialize();
            responseHandler += DefaultResponseHandler;
        }

        void Initialize()
        {
            foreach (NotificationBinding nb in notificationBindings)
            {
                nb.notification.SetActive(false);
                notificationTitles.Add(nb.title);
            }
        }        

        public void DefaultResponseHandler(eNotificationType type, eResponseType responseType,  string input)
        {
            Debug.Log("DefaultHandleResponse");

        }


       public void NotificationCallbackOk()
        {
            Debug.Log("OK");
        }


        public void NotificationCallbackRetry()
        {
            Debug.Log("Retry");
        }


        public void NotificationCallbackCancel()
        {
            Debug.Log("Canceling it hard");


        }


        //public void ShowNotification(eNotificationType type, string title, string message)
        //{
        //    //DoozyUI.UIManager.ShowNotification("Busy", -1, false, "Busy", "A test message with informative info");
        //    //DoozyUI.UIManager.ShowNotification("Busy", -1, false, "Busy", "A test message with informative info");
        //    DoozyUI.UIManager.ShowNotification(type.ToString(), -1, false, title, message);


        //    //switch (type)
        //    //{
        //    //    case eNotificationType.Info:
        //    //        DoozyUI.UIManager.ShowNotification(type.ToString(), -1, false, type.ToString(), message );

        //    //        break;

        //    //    case eNotificationType.Busy:
        //    //        break;

        //    //    case eNotificationType.Choice2Way:
        //    //        break;

        //    //    case eNotificationType.Choice3Way:
        //    //        break;

        //    //}

        //}

        // #region  StateMachineApproach
        //void OnEnable()  // Same trigger principal - trigger names start with "On"
        //{
        //    SmbEventDispatcher.OnStateEntered += HandleStateEnter;
        //    SmbEventDispatcher.OnStateExited += HandleStateExit;
        //}

        //private void OnDisable()
        //{
        //    SmbEventDispatcher.OnStateEntered -= HandleStateEnter;
        //    SmbEventDispatcher.OnStateExited -= HandleStateExit;
        //}

        //public void HandleStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        //{
        //    stateInfoHelper = new AnimatorStateInfoHelper(animatorStateInfo, layerIndex, notificationTitles.ToArray());
        //    if (stateInfoHelper.stateName == null) return;

        //   Debug.Log("STATE ENTER  =" + stateInfoHelper.stateName);             
        //    //DoozyUI.UIManager.ShowUiElement("Alert2", Category);
        //    DoozyUI.UIManager.ShowNotification(stateInfoHelper.stateName, 1, false, "Darryl", "Darryl's message");
        //}

        //public void HandleStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        //{
        //    stateInfoHelper = new AnimatorStateInfoHelper(animatorStateInfo, layerIndex, notificationTitles.ToArray());
        //    if (stateInfoHelper.stateName == null) return;

        //    Debug. Log("STATE EXIT =" + stateInfoHelper.stateName);
        //    //HideViewFor(stateInfoHelper.stateName);
        //    //DoozyUI.UIManager.HideNotification("Alert3");
        //   // DoozyUI.UIManager.HideNotification(stateInfoHelper.stateName, 1, false, "Darryl", "Darryl's message");
        //}


        //public bool HideViewFor(string stateName)
        //{
        //    if (stateName == "None")
        //    {
        //        Reset();
        //        return false;
        //    }

        //    // DoozyUI.UIManager.HideUiElement(stateName, Category);
        //    return true;
        //}

        // #endregion

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
