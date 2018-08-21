using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DoozyUI;

namespace AllNetXR
{
    public class UIManagerTester : MonoBehaviour
    {
        //public UIManagerSequential UIManagerSequential;
        //public UIManagerAdditive UIManagerAdditive;
        //public TempScoreboardController Scoreboard;
        //private TempGameManager GameMgr;

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

        private void ProcessAdditiveUIStateRequests()
        {
            //eUIStateAdditive state = eUIStateAdditive.None;

            if (Input.GetKeyDown(KeyCode.Keypad0))
            {
                //state = (eUIStateAdditive)0;
               
                DoozyUI.UIManager.ShowNotification("Alert1", -1, false, "Darryl", "Darryl's message");

            }

            if (Input.GetKeyDown(KeyCode.Keypad1))
            {
                //state = (eUIStateAdditive)1;
                DoozyUI.UIManager.ShowNotification("Alert2", -1, false, "Darryl", "Darryl's message");
            }

            if (Input.GetKeyDown(KeyCode.Keypad2))
            {
                //state = (eUIStateAdditive)2;
                DoozyUI.UIManager.ShowNotification("Alert3", -1, false, "Darryl", "Darryl's message");
            }

            //ToggleViewVisibility(state);
        }

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
    }
}
