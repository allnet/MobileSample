using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Talespin;

namespace Evgo
{
    public class GameMetadata   // can have struct within a struct
    {
        public GameMetadata(eAppState currentGameState, string location, string username, int totalPoints,
                            int currentRound, int numberOfRounds, float elapsedTime, float timeRemaining,
                            List<PlayerScore> playerScores = null)
        {
            CurrentGameState = currentGameState;
            Location = location;
            Username = username;
            CurrentRound = currentRound;
            NumberOfRounds = numberOfRounds;
            ElapsedTime = elapsedTime;
            TotalPoints = totalPoints;
            PlayerScores = playerScores;
        }
        
        #region  Event/Delegate setup
        //public delegate void OnPropertyChangeDelegate();
        public delegate void OnStringPropertyChangeDelegate(string PropertyValue);
        public delegate void OnIntPropertyChangeDelegate(int PropertyValue);
        public delegate void OnFloatPropertyChangeDelegate(float PropertyValue);
        public delegate void OnEnumChangeDelegate(eAppState PropertyValue);
        public delegate void OnPlayerScoresChangeDelegate(List<PlayerScore> PropertyValue);
        
        //Special
        //public delegate void OnArrayChangedDelegate(List<T> PropertyValue);
        public event GameMetadata.OnEnumChangeDelegate OnGameStateSetTo;
        public event GameMetadata.OnStringPropertyChangeDelegate OnLocationSetTo;
        public event GameMetadata.OnStringPropertyChangeDelegate OnUsernameSetTo;
        public event GameMetadata.OnIntPropertyChangeDelegate OnTotalPointsSetTo;
        public event GameMetadata.OnFloatPropertyChangeDelegate OnElapsedTimeSetTo;
        public event GameMetadata.OnIntPropertyChangeDelegate OnCurrentRoundSetTo;
        public event GameMetadata.OnIntPropertyChangeDelegate OnNumberOfRoundsSetTo;
        public event GameMetadata.OnPlayerScoresChangeDelegate OnPlayerScoresSetTo; // commit the score
        #endregion

        private eAppState currentGameState;  
        public eAppState CurrentGameState
        {
            get
            {
                return currentGameState;
            }
            set
            {
                currentGameState = value;
                if (OnGameStateSetTo != null)
                {
                    OnGameStateSetTo(value);      // OnPropertyChanged(nameof(Location)); 
                }
            }
        }

        private string location; 
        public string Location
        {
            get
            {
                return location;
            }
            set
            {
                location = value;
                if (OnLocationSetTo != null)
                {
                    OnLocationSetTo(value);      // OnPropertyChanged(nameof(Location)); 
                }
            }
        }

        private string username;
        public string Username
        {
            get
            {
                return username;
            }
            set
            {
                username = value;
                if (OnUsernameSetTo != null)
                {
                    OnUsernameSetTo(value);      // OnPropertyChanged(nameof(Location)); 
                }
            }
        }

        private int totalPoints;
        public int TotalPoints
        {
            get
            {
                return totalPoints;
            }
            set
            {
                totalPoints = value;
                if (OnTotalPointsSetTo != null)
                {
                    OnTotalPointsSetTo(value);      // OnPropertyChanged(nameof(Location)); 
                }
            } 
        }

        private float elapsedTime;
        public float ElapsedTime
        {
            get
            {
                return elapsedTime;
            }
            set
            {
                elapsedTime = value;
                if (OnElapsedTimeSetTo != null)
                {
                    OnElapsedTimeSetTo(value);      // OnPropertyChanged(nameof(Location)); 
                }
            }
        }

        private int currentRound;
        public int CurrentRound
        {
            get
            {
                return currentRound;
            }
            set
            {
                currentRound = value;
                if (OnCurrentRoundSetTo != null)
                {
                    OnCurrentRoundSetTo(value);      // OnPropertyChanged(nameof(Location)); 
                }
            }
        }


        private int numberOfRounds;
        public int NumberOfRounds
        {
            get
            {
                return numberOfRounds;
            }
            set
            {
                numberOfRounds = value;
                if (OnNumberOfRoundsSetTo != null)
                {
                    OnNumberOfRoundsSetTo(value);      // OnPropertyChanged(nameof(Location)); 
                }
            }
        }

        private List<PlayerScore> playerScores;
        public List<PlayerScore> PlayerScores
        {
            get
            {
                return playerScores;
            }
            set
            {
                playerScores = value;
                if (OnPlayerScoresSetTo != null)
                {
                    OnPlayerScoresSetTo(value);      // OnPropertyChanged(nameof(Location)); 
                }
            }
        }

        public struct PlayerScore : IComparable<PlayerScore>
        {
            public string Username;
            public int Score;

            public PlayerScore(string newName, int newScore)
            {
                Username = newName;
                Score = newScore;
            }

            public int CompareTo(PlayerScore otherScore)
            {
                if (otherScore.Score == 0)
                {
                    return 1;
                }

                return otherScore.Score - Score;
            }
        }
        
    }
}

