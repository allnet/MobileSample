using Talespin;
using UnityEngine;
using System.Collections.Generic;

namespace AllNetXR
{
    public class AppManager : MonoBehaviour  // No update
    {
        public static AppManager Instance;
        //public static eAppState GameState;
        public AppMetadata appMetadata;

        //-------------------- MANAGERS
        //[Header("MANAGERS")]
        //public ScoreManager ScoreManager;
        //public BaseManager BaseManager;
        //public AudioManager AudioManager;
        //public DifficultyManager DifficultyManager;
        //public BaseballTrajectoryCalculator TrajectoryCalculator;
        //public UISystemManager UISystemManager;

        ////-------------------- OBJECTS
        //[Header("OBJECTS")]
        //public Bat Bat;
        //public Pitcher Pitcher;
        //public Transform Strikezone;
        //public ReplayCameraSystem ReplayCamera;



        //-------------------- MODES               

        //[System.Serializable]
        //public struct StringToStateBinding
        //{
        //    public eAppState gameState;
        //    public StateController StateController; //UI

        //    public StringToStateBinding(eAppState gameState = eAppState.State0, StateController stateController = null)
        //    {
        //        this.gameState = gameState;
        //        this.StateController = stateController;
        //    }
        //}
        //[Header("Game Mode to UI Bindings")]
        //public StringToStateBinding[] Bindings;

        //------------------------------------------------------------------------------------- APPLICATION

        protected void Awake()
        {
            Instance = this;

        }

        private void Start()
        {
            appMetadata = new AppMetadata();
        }

        /*
                  protected override void PostStart()
                  {
                      base.PostStart();
                      Fade(Color.black, Color.clear, 2.0f);

                      ChangeToGameState((eAppState)0);  // start app/game/experience
                  }

                  // Darryl TODO:  review this with everyone should probably be on MainApplication possibly 
                  public void ChangeToGameState(eAppState gameState)  // this is simply a monobehavior
                  {
                      // Guard statements
                      if (gameState == CurrentGameState)
                      {
                          return;
                      }

                      // continue            
                      ApplicationMode newMode = Bindings[(int)gameState].GameMode;
                      if (newMode != null)
                      {
                          ChangeMode(newMode);  // previous calls used this
                      }

                      if (UISystemManager.Sequential != null && UISystemManager.Sequential is IUIAnimatableSingle)
                      {
                          UISystemManager.Sequential.ChangeStateTo(gameState);
                      }

                      if (UISystemManager.Scoreboard != null)
                      {
                          UISystemManager.Scoreboard.ChangeStateTo(gameState);
                      }

                      CurrentGameState = gameState;
                      UISystemManager.UIStateMachine.SetInteger("GameState", (int)CurrentGameState);
                    }

                  */

        // On next request
        public void ChangeStateTo(eAppState toState)  // adheres to interface or could subclass
        {
            // send exit to present state controller and wait for callback
            // isBusy = true;
        }

        public void ExitCompleteCallback()  // is ready for 
        {
            // turn off state controller

            // Turn on next state contoller after small delay

            //Set trigger for  next state controller
            // actually change the state by turning on the next controller
        }

        public void EnterCompleteCallback()
        {


        }

    }
}