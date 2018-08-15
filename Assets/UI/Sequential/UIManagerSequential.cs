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

        #region Next/Previous support
        public eAppState GetNextStateFor(eAppState state, eAppState newStartState = (eAppState)1  )
        {
            int idx = (int)state;

            AppMetadata gameData = AppManager.Instance.appMetadata;
            bool shouldLoop =
                (state == loopEndState
                && gameData.currentRound < numberOfRounds);

            int newIdx = (shouldLoop) ? (int)loopStartState : ++idx;

            if (!Enum.IsDefined(typeof(eAppState), newIdx))
            {
                gameData.currentRound = 0;
                newIdx = (int)newStartState;
            }

            eAppState requestedState = (eAppState)newIdx;

            return requestedState;
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