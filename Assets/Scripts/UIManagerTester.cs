using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DoozyUI;
using UnityEngine.Events;

namespace AllNetXR
{
    public class UIManagerTester : MonoBehaviour
    {

        public Sprite notificationSprite;

        private bool IsShowing;

        //private void OnEnable()
        //{
        //    OVRTouchpad.TouchHandler += LocalTouchEventCallback;
        //}

        //private void OnDisable()
        //{
        //    OVRTouchpad.TouchHandler -= LocalTouchEventCallback;
        //}

        private void Start()
        {

            #region PullFromHiearchy
            //if (PullFromHieararchy)
            //{
            //    for (int i = 0; i < gameObject.transform.childCount; i++)
            //    {
            //        GameObject go = gameObject.transform.GetChild(i).gameObject; Debug.Log(go.name);
            //        if (go.activeInHierarchy == true)
            //        {
            //            Manager.RegisterChildrenCheerleaders(go);
            //        }
            //    }
            //}
            // UIManager.ChangeStateTo(eUIState.EnterLocation);
            #endregion

            //GameMgr = TempGameManager.Instance;
        }

        public void Update()
        {
            ProcessAdditiveUIStateRequests();
            //ProcessSequentialUIStateRequests();

        }


        private void ProcessAdditiveUIStateRequests()
        {
            UIManagerAdditive uim = UIManagerAdditive.Instance;

            if (Input.GetKeyDown(KeyCode.Keypad0))  //BUSy  needs rotating sprite
            {
                //AppStateController.Instance.animator.Play("Alert3", 1, 0f);

                //UIManagerAdditive.responseHandler -= UIManagerAdditive.Instance.DefaultResponseHandler; // unload the default
                //UIManagerAdditive.responseHandler += ResponseHandler;
                //UIManagerAdditive.Instance.ShowNotification(eNotificationType.Busy, eNotificationType.Busy.ToString(), "A busy test message");  // needs a callback for each button

                //DoozyUI.UIManager.ShowNotification("Busy", -1, false, "Busy", "A test message with informative info");  // works simple
                DoozyUI.UIManager.ShowNotification("Busy", -1, false, "Busy", "A test message with busy info", UIManagerAdditive.Instance.NotificationCallbackCancel);  //simple cancel callback
            }

            if (Input.GetKeyDown(KeyCode.Keypad0))  //Info
            {
                DoozyUI.UIManager.ShowNotification("Info", -1, false, "Info", "A test message with informative info.", UIManagerAdditive.Instance.NotificationCallbackCancel);
            }


            if (Input.GetKeyDown(KeyCode.Keypad1))  // single button - info
            {

                string[] buttonNames = { "OK" };
                UnityAction[] callbacks = { UIManagerAdditive.Instance.NotificationCallbackOk };//
                DoozyUI.UIManager.ShowNotification("SingleButton", -1, false, "Single Button Info", "A test message with informative info", null, buttonNames, callbacks);

            }

            if (Input.GetKeyDown(KeyCode.Keypad2))
            {
                DoozyUI.UIManager.ShowNotification("Choice2Way", -1, false, "Choice 2-way", "A test message with 2-way info.", UIManagerAdditive.Instance.NotificationCallbackCancel);
            }

            if (Input.GetKeyDown(KeyCode.Keypad3))
            {
                //DoozyUI.UIManager.ShowNotification("Choice3Way", -1, false, "Choice 3-way", "A test message with 3-way info.", UIManagerAdditive.Instance.NotificationCallbackCancel);

                string[] buttonNames = { "OK", "Cancel", "Retry" };  // but 1, 2, 3 respectively
                UnityAction[] callbacks = { uim.NotificationCallbackOk, uim.NotificationCallbackCancel, uim.NotificationCallbackRetry };
                DoozyUI.UIManager.ShowNotification("Choice3Way", -1, false, "Choice 3-Way", "A test message with informative info", notificationSprite, buttonNames, buttonNames, callbacks);
            }

            //ToggleViewVisibility(state);
        }

        void ResponseHandler(eNotificationType type, eResponseType responseType, string input)
        {
            Debug.Log("HandleResponse + " + type.ToString() + " " + responseType.ToString() + "-" + input);

        }

        #region samples
        //private void ToggleViewVisibility(eUIStateAdditive state)
        //{
        //    if (state == eUIStateAdditive.None)
        //    {
        //        return;
        //    }

        //    IsShowing = UIManagerAdditive.IsAdditiveUIShowing[(int)state];
        //    if (IsShowing)
        //    {
        //        UIManagerAdditive.HideViewForState(state);
        //    }
        //    else
        //    {
        //        UIManagerAdditive.ShowViewForState(state);
        //    }
        //}

        //// Touch callbacks 
        //void LocalTouchEventCallback(object sender, EventArgs args)
        //{
        //    var touchArgs = (OVRTouchpad.TouchArgs)args;
        //    OVRTouchpad.TouchEvent touchEvent = touchArgs.TouchType;

        //    switch (touchEvent)
        //    {
        //        case OVRTouchpad.TouchEvent.SingleTap:  // single-click
        //            //Debug.Log("SINGLE CLICK\n");
        //            break;

        //        case OVRTouchpad.TouchEvent.Left:
        //            //Debug.Log("LEFT SWIPE\n");
        //            break;

        //        case OVRTouchpad.TouchEvent.Right:
        //            //Debug.Log("RIGHT SWIPE\n");
        //            break;

        //        case OVRTouchpad.TouchEvent.Up:
        //            //Debug.Log("UP SWIPE\n");
        //            break;

        //        case OVRTouchpad.TouchEvent.Down:
        //            //Debug.Log("DOWN SWIPE\n");
        //            break;
        //    }
        //}

        //private void ProcessSequentialUIStateRequests()
        //{
        //    if (Input.GetKeyDown(KeyCode.A)
        //    {
        //        Debug.Log("Next requested");
        //        //UIManagerSequential.GoToNextState();
        //        eGameState nextState = UIManagerSequential.Instance.GetNextStateFor(TempGameManager.Instance.CurrentGameState);
        //        GameMgr.ChangeToGameState(nextState);
        //    }

        //    if (Input.GetKeyDown(KeyCode.B))
        //    {
        //        Debug.Log("Previous requested");
        //        //UIManagerSequential.GoToPreviousState();
        //        //GameMgr.ChangeToGameState(nextState);
        //        eGameState previousState = UIManagerSequential.Instance.GetPreviousStateFor(TempGameManager.Instance.CurrentGameState);
        //        GameMgr.ChangeToGameState(previousState);
        //    }

        //    if (Input.GetKeyDown(KeyCode.C))  //location or launch
        //    {
        //        Debug.Log("Enter Location requested");
        //        //UIManagerSequential.ChangeStateTo(eGameState.EnterLocation);
        //        //StartCoroutine("ReturnToRandomIdle", 10.0f);
        //        GameMgr.ChangeToGameState(eGameState.EnterLocation);
        //    }

        //    if (Input.GetKeyDown(KeyCode.S))  //start
        //    {
        //        Debug.Log("Enter Username requested");
        //        // UIManagerSequential.ChangeStateTo(eGameState.PreGame);  // enter username
        //        GameMgr.ChangeToGameState(GameMgr.StartGameState);

        //    }

        //    if (Input.GetKeyDown(KeyCode.G))
        //    {
        //        Debug.Log("GamePlay requested");
        //        //StartCoroutine("ReturnToRandomIdle", 1.0f);   // if you want to return from idle after a wait interval
        //        //UIManagerSequential.ChangeStateTo(eGameState.PlayRound); 
        //        GameMgr.ChangeToGameState(GameMgr.LoopStartGameState);
        //        // GameMgr.ChangeToGameState(eGameState.PlayRound);
        //    }
        //}
        #endregion

    }
}

