using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Talespin;

namespace AllNetXR
{
    public class UIManagerSequential : MonoBehaviour  //, IUIAnimatableSingle
    {
        public static UIManagerSequential Instance;
        public int numberOfRounds;
        private eAppState newStartState;
        public eAppState loopStartState, loopEndState; // DH -
        private AppMetadata appMetadata; // helper

        #region Next/Previous support
        public eAppState GetNextStateFor(eAppState state, eAppState theStartState = (eAppState)0  )
        {            
            //int newIdx = DetermineNextIndex(state);
            int newIdx = (int)state + 1;

            if (!Enum.IsDefined(typeof(eAppState), newIdx))
            {
                newIdx = (int)theStartState;
            }

            Debug.Log("***" + state + " < == > " + (eAppState)newIdx);
             
            return (eAppState)newIdx;
        }

        public int DetermineNextIndex(eAppState state)
        {
            int idx = (int)state;

            bool shouldLoop =
                (state == loopEndState && appMetadata.currentRound < numberOfRounds);

            int newIdx = (shouldLoop) ? (int)loopStartState : ++idx;
            appMetadata.currentRound++;

            return newIdx;
        }


        public eAppState GetPreviousStateFor(eAppState state)  // starts short game loop again
        {
            int idx = (int)state;
            int newIdx = --idx;   // vs idx++

            if (!Enum.IsDefined(typeof(eAppState), newIdx))
            {
                return loopStartState;
            }

            eAppState requestedState = (eAppState)newIdx;
            return requestedState;
        }

        //protected void DoBetweenRoundsProcessing()
        //{
        //
        //       // GameState = eGamePlayState.Idle;
        //        TempGameManager.Instance.ScoreManager.CalculateScore();
        //        //DOVirtual.DelayedCall(OutroGameSeconds, () => TempGameManager.Instance.GotoFinalBallMode());
        //        //DOVirtual.DelayedCall(OutroGameSeconds, () => TempGameManager.Instance.GotoPostGameMode());               
        //        DOVirtual.DelayedCall(OutroGameSeconds, () => TempGameManager.Instance.ChangeToGameState(eAppState.FinalRound));
        //    }
        //    else
        //    {
        //        TempGameManager.Instance.Metadata.CurrentRound++;
        //        Invoke("Pitch", 2.5f);

        //        eAppState currentState = TempGameManager.Instance.CurrentGameState;
        //        eAppState state = TempGameManager.Instance.UISystemManager.Sequential.GetNextStateFor(currentState);
        //        TempGameManager.Instance.ChangeToGameState(state);
        //    }
        //}

        #endregion

    }
}