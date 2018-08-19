using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Talespin;

namespace AllNetXR
{

    public class LoopSequencer : MonoBehaviour  //, IUIAnimatableSingle
    {
        public int numberOfRounds;
        //public static LoopSequencer Instance;
        //private eAppState newStartState;
        //public eAppState loopStartState, loopEndState; // DH -
        //private AppMetadata appMetadata; // helper

        #region Next/Previous support
        public int GetNextIndex(int currentIndex, int startIndex, int maxIndex = 0)  // maxIndex s/b count if using enumerator
        {
            //int newIdx = DetermineNextIndex(state);
            int newIdx = currentIndex + 1;

            if (newIdx >= maxIndex)
            {
                newIdx = startIndex;
            }

            Debug.Log("***" + currentIndex + " < == > " + newIdx);

            return newIdx;
        }
           
        public int GetPreviousIndex(int currentIndex, int startIndex,  int maxIndex = 0)  // count
        {
            int newIdx = currentIndex - 1;   // vs idx++

            if (newIdx < 0)
            {
                newIdx = maxIndex - 1;
            }

            return newIdx;
        }

        /*
        #region Next/Previous support
        
        public int DetermineNextIndex(eAppState state)
        {
            int idx = (int)state;
            bool shouldLoop =
                (state == loopEndState && appMetadata.currentRound < numberOfRounds);

            int newIdx = (shouldLoop) ? (int)loopStartState : ++idx;
            appMetadata.currentRound++;

            return newIdx;
        }
    
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
        */
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