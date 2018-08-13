using System;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace Talespin.Debugging
{
    /// <summary>
    /// An on screen logger for logging logs.
    /// </summary>
    public class LogConsole : MonoBehaviour
    {
        private TextMeshPro Text;
        private StringBuilder Data;
        [SerializeField]
        private bool DisplayConsole = false;
        public int MaxLines = 16;

        //------------------------------------------------------------------------------------- MONO BEHAVIORS

        private void Awake()
        {
            Text = GetComponent<TextMeshPro>();
            Text.enabled = DisplayConsole;
            Data = new StringBuilder();
            //Application.logMessageReceivedThreaded += OnLog;
            Application.logMessageReceived += OnLog;
        }

        //------------------------------------------------------------------------------------- EVENTS

        private void OnLog(string logString, string stackTrace, LogType type)
        {
            Log(logString);
        }

        //------------------------------------------------------------------------------------- LOGGING

        /// <summary>
        /// Logs a message directly onto this console only (not the Unity console).
        /// </summary>
        /// <param name="_message">The message to display.</param>
        public void Log(string _message)
        {
            // add message
            _message = Environment.NewLine + _message;
            Data.Append(_message);

            // trim console
            MatchCollection matches = Regex.Matches(Data.ToString(), "(\r\n|\n|\r)");
            if (matches.Count >= MaxLines)
            {
                try
                {
                    Match match = matches[matches.Count - MaxLines - 1];
                    Data.Remove(0, match.Index + match.Length);
                }
                catch { }
            }

            // display message
            Text.text = Data.ToString();
        }

        /// <summary>
        /// Clears this console.
        /// </summary>
        public void Clear()
        {
            Data.Length = 0;
            Text.text = Data.ToString();
        }

        //------------------------------------------------------------------------------------- DISPLAY

        /// <summary>
        /// Toggles the console display.
        /// </summary>
        /// <param name="show"></param>
        public void ToggleDisplay(bool? show = null)
        {
            DisplayConsole = show == null ? !DisplayConsole : (bool)show;
            Text.enabled = DisplayConsole;
        }
    }
}