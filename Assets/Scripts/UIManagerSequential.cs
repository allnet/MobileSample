using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Talespin;

namespace AllNetXR
{
    public class UIManagerSequential : MonoBehaviour
    {
        /* 
         * 
        public static UIManagerSequential Instance;
        public bool DebugMode;
        public MainApplication.eApplicationState CurrentApplicationState;
        public int BaseLayerIndex = 0;
        public UIViewControllerSequential[] UIViewControllers;

        private GameManager GameMgr; // shortcuts
        private Animator UIStateMachine;
        private UIViewControllerSequential CurrentVC;
        private int CurrentVCIdx;
        private bool IsFirstPass = true;
        private int HashIsViewActive;
        private int HashShouldWait;
        private int NumberOfRounds = 3;
        private eGameState UpcomingState;

        private void Awake()
        {
            Instance = this;
            //HashStateEnumId = Animator.StringToHash("StateEnumId");    //DH - need to do this for most parameters
            HashIsViewActive = Animator.StringToHash("IsViewActive");
            HashShouldWait = Animator.StringToHash("ShouldWait");

            GameMgr = GameManager.Instance;
            UIStateMachine = GameManager.Instance.UISystemManager.UIStateMachine;

            ResetViewControllers();
            ResetParms();
        }

        public void ResetViewControllers()
        {
            foreach (UIViewControllerSequential UIStateVC in UIViewControllers)
            {
                if (UIStateVC == null) continue;
                UIStateVC.View.SetActive(false);
                UIStateVC.gameObject.SetActive(false);
                UIStateVC.IsReady = false;
            }
            CurrentVC = null;
        }

        private void TurnOffCurrentVC()
        {
            if (CurrentVC == null)
            {
                return;
            }

            CurrentVC.IsReady = false;
            CurrentVC.gameObject.SetActive(false);
            CurrentVC.View.SetActive(false);  // visible UI 
        }

        private void ResetParms()
        {
            UIStateMachine.SetBool(HashIsViewActive, false);
            UIStateMachine.SetBool(HashShouldWait, false);
        }

        public void ChangeStateTo(eGameState aState)  //method to change state
        {
            //if (aState == eApplicationState.running) return; // TODO: Darryl - only process when in wait state
            if (DebugMode) Debug.Log("ChangeStateRequested = " + aState.ToString());


            UpcomingState = aState;
            UIStateMachine.SetBool(HashShouldWait, false);  // continue onto close

            // TempGameManager.Instance.SoundManager.PlayClipWithName("Beep_1");
        }

        private void SetCurrentVCToState(eGameState requestedState)
        {
            if (requestedState == eGameState.None) { return; }

            CurrentVC = UIViewControllers[(int)requestedState];

            CurrentVC.gameObject.SetActive(true);
            CurrentVC.View.SetActive(true);  // visible UI 
            CurrentVC.IsReady = false;
            //Manager.CurrentGameState = requestedState;
            GameManager.Instance.CurrentGameState = requestedState;
            CurrentVCIdx = (int)requestedState;
        }

        #region Mecanim Animation methods (interface)

        public void OnClipStarted(String ClipTitle)  // could be enum
        {

            Debug.Log("Started " + ClipTitle + " for " + GameManager.Instance.CurrentGameState);
   
            switch (ClipTitle)
            {
                case "Open":
                    OnClipOpenStarted(GameManager.Instance.CurrentGameState);
                    break;

                case "Waiting (looping or non)":
                    OnClipWaitStarted(GameManager.Instance.CurrentGameState);
                    break;

                case "Close":
                    break;
            }
        }

        public void OnClipEnded(String ClipTitle)  // could be enum
        {
            Debug.Log("Ended " + ClipTitle + " for last state");
            switch (ClipTitle)
            {
                case "Open":
                    break;

                case "Waiting (looping or non)":

                    break;

                case "Close":
                    OnClipCloseComplete(GameManager.Instance.CurrentGameState);
                    break;
            }
        }

        public void OnClipOpenStarted(eGameState requestedState)  // animation event
        {
            if (DebugMode) Debug.Log("State clip opened - " + requestedState.ToString());

            TurnOffCurrentVC();

            SetCurrentVCToState(UpcomingState);

            UIStateMachine.SetBool(HashIsViewActive, true);
            UIStateMachine.SetBool(HashShouldWait, true); // important  

            //if sound manager exists then invoke it
            if (UISoundManager.Instance != null)
            {
                //GameMgr.SoundManager.PlayMatchingClip(GameMgr.Metadata.CurrentGameState, UISoundManager.eUISoundState.Opening);
            }
        }

        public void OnClipOpenEnded(eGameState requestedState)  // animation event
        {
            if (DebugMode) Debug.Log("State clip open ended - " + requestedState.ToString());
        }

        public void OnClipWaitStarted(eGameState aState)  // interface
        {
            if (aState == eGameState.None)
            {
                return;
            }

            // GameMgr.SoundManager.PlayMatchingClip(GameMgr.Metadata.CurrentGameState, UISoundManager.eUISoundState.Waiting);
            CurrentVC.IsReady = true;
        }

        public void OnClipCloseStarted(eGameState aState)
        {
            //GameMgr.SoundManager.PlayMatchingClip(GameMgr.Metadata.CurrentGameState, UISoundManager.eUISoundState.Closing);
        }

        public void OnClipCloseComplete(eGameState aState)
        {
            if (DebugMode) Debug.Log("OnClip Close complete - " + aState.ToString());

            UIStateMachine.SetBool(HashShouldWait, false);
            UIStateMachine.SetBool(HashIsViewActive, false);

            //UIStateMachine.SetBool(GameMgr.CurrentGameState.ToString(), false);
            //UIStateMachine.SetBool(UpcomingState.ToString(), true);
            String prefix = "On";
            UIStateMachine.SetTrigger(prefix + UpcomingState.ToString());

        }
        #endregion

        #region Next/Previous support
        public void GoToNextState()
        {
            ChangeStateTo(GetNextStateFor(GameMgr.CurrentGameState));
        }

        public void GoToPreviousState()
        {
            ChangeStateTo(GetPreviousStateFor(GameMgr.CurrentGameState));
        }

        public eGameState GetNextStateFor(eGameState state = eGameState.None)
        {
            if (state == eGameState.None)
            {
                state = GameMgr.Metadata.CurrentGameState;
            }

            int idx = (int)state;

            GameMetadata gameData = GameMgr.Metadata;
            bool shouldLoop =
                (GameMgr.CurrentGameState == GameMgr.LoopEndGameState
                && gameData.CurrentRound < gameData.NumberOfRounds);

            int newIdx = (shouldLoop) ? (int)GameMgr.LoopStartGameState : ++idx;

            if (!Enum.IsDefined(typeof(eGameState), newIdx))
            {
                GameMgr.Metadata.CurrentRound = 0;
                return GameMgr.StartGameState;
            }

            eGameState requestedState = (eGameState)newIdx;

            return requestedState;
        }

        public eGameState GetPreviousStateFor(eGameState state)  // starts short game loop again
        {
            int idx = (int)state;
            int newIdx = --idx;   // vs idx++

            if (!Enum.IsDefined(typeof(eGameState), newIdx))
            {
                return GameMgr.LoopStartGameState;
            }

            eGameState requestedState = (eGameState)newIdx;
            return requestedState;
        }

        //protected void DoBetweenRoundsProcessing()
        //{

        //       // GameState = eGamePlayState.Idle;
        //        TempGameManager.Instance.ScoreManager.CalculateScore();
        //        //DOVirtual.DelayedCall(OutroGameSeconds, () => TempGameManager.Instance.GotoFinalBallMode());
        //        //DOVirtual.DelayedCall(OutroGameSeconds, () => TempGameManager.Instance.GotoPostGameMode());               
        //        DOVirtual.DelayedCall(OutroGameSeconds, () => TempGameManager.Instance.ChangeToGameState(eGameState.FinalRound));
        //    }
        //    else
        //    {
        //        TempGameManager.Instance.Metadata.CurrentRound++;
        //        Invoke("Pitch", 2.5f);

        //        eGameState currentState = TempGameManager.Instance.CurrentGameState;
        //        eGameState state = TempGameManager.Instance.UISystemManager.Sequential.GetNextStateFor(currentState);
        //        TempGameManager.Instance.ChangeToGameState(state);
        //    }
        //}

        #endregion
*/
    }
}