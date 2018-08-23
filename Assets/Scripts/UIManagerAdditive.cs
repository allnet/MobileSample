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
        public Sprite notificationSprite;

        // Use this for initialization
        void Awake()
        {
            Instance = this;

            responseHandler += DefaultResponseHandler;
            DeactivateUINotifications(this.transform);  // notifications
        }

        public Transform[] DeactivateUINotifications(Transform parent)  // all top level
        {
            Transform[] children = new Transform[parent.childCount];
            for (int ID = 0; ID < parent.childCount; ID++)
            {
                children[ID] = parent.GetChild(ID);
                children[ID].gameObject.SetActive(false);
            }
            return children;
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


    }
}
