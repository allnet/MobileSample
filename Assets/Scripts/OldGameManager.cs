using Talespin;
using UnityEngine;
using System.Collections.Generic;

namespace Evgo
{
    //-------------------- DIFFICULTY
  /*
   *public enum Difficulty
    {
        Tutorial,
        Easy,
        Medium,
        Hard,
        Final
    }

    public enum eGameState  //Must correspond to state names in animator
    {
        EnterLocation = 0,
        PreGame = 1,  // enter username
        Tutorial = 2,
        GamePlay = 3,
        Finale = 4,
        Scoreboard = 5,
        Count
    }
    */
    public class OldGameManager : MainApplication
    {
        public static OldGameManager Instance;
        public GameMetadata Metadata;
        public int HashStateEnumId;
        public int MaxRounds = 5;
        
        /*
        //-------------------- GAME     
        [Header("Game Objects")]
        public ScoreManager ScoreManager;
        public ClockDisplay TimeClock;
        public Camera ExternalCamera;

        //------------------------------------------------------------------------------------- MONOBEHAVIOUR

        protected override void Awake()
        {
            Instance = this;

            CheckManager<ScoreManager>(ref ScoreManager);          

            base.Awake();
        }

        private void CheckManager<T>(ref T manager) where T : MonoBehaviour
        {
            if (manager == null)
            {
                Debug.LogWarning("Set your " + typeof(T) + " for faster loading");

                manager = FindObjectOfType<T>();
                if (manager == null)
                {
                    gameObject.AddComponent<T>();
                }
            }
        }

        protected override void PostStart()
        {
            //AudioManager.instance.PlayAudienceLoop();
            List<GameMetadata.PlayerScore> scores = new List<GameMetadata.PlayerScore>();
            Metadata = new GameMetadata(currentGameState: eGameState.None, location: "", username: "", totalPoints: 0,
                        currentRound: 0, numberOfRounds: 15, elapsedTime: 0f, timeRemaining: 0f, 
                        playerScores: new List<GameMetadata.PlayerScore>());

            OVRManager.HMDMounted += OnHMDMounted;
            OVRManager.HMDUnmounted += OnHMDUnmounted;

            base.PostStart();
        }
                
        #region Utilities
        public void ToggleExternalDisplay(bool bOn)
        {
            ExternalCamera.enabled = bOn;
        }

        public void OnHMDMounted()
        {
            ToggleExternalDisplay(false);
        }

        public void OnHMDUnmounted()
        {
            ToggleExternalDisplay(true);
        }
        #endregion

        #region Fade
        public void FadeIn()
        {
            Fade(Color.black, Color.clear, 2.0f);
        }

        public void FadeOut()
        {
            Fade(Color.clear, Color.black, 2.0f);
        }
        #endregion Fade
     
        protected override void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q) && Input.GetKey(KeyCode.LeftControl))
            {
                Debug.Log("QUIT");
#if UNITY_EDITOR
                // Application.Quit() does not work in the editor so
                // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                Application.Quit();
            }
            else if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.F12))
            {
                ScoreManager.Reset();
                //JSONSaveLoad.Delete();
            }
            base.Update();
        }

        #region state change response future

        //public void StateChangedTo(eUIState state)
        //{
        //    //Remove all stacked actions

        //    switch (state)
        //    {
        //        case eUIState.EnterLocation:

        //            break;

        //        case eUIState.PreGame:

        //            break;

        //        case eUIState.Welcome:
        //            //GameManager.Instance.OnLocationSetTo += HandleLocationSetTo;  
        //            break;

        //        case eUIState.BetweenRounds:

        //            break;
        //    }
        //}
        #endregion
        */
    }
}