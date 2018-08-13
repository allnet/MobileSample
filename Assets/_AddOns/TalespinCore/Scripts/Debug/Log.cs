using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Talespin.Debugging
{
    public static class Log
    {
        private static LogSettings settings;
        public static LogSettings Settings { get { return GetSettings(); } }

        //------------------------------------------------------------------------------------- SETUP

        private static LogSettings GetSettings()
        {
            if (settings == null)
            {
                settings = Resources.Load<LogSettings>("TalespinLogSettings");

                if (settings == null)
                {
                    settings = ScriptableObject.CreateInstance<LogSettings>();

#if UNITY_EDITOR
                    // Only in the editor should we save it to disk
                    string properPath = Path.Combine(Application.dataPath, "Resources");
                    if (!Directory.Exists(properPath))
                    {
                        UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");
                    }

                    string fullPath = Path.Combine(Path.Combine("Assets", "Resources"), "TalespinLogSettings.asset");
                    UnityEditor.AssetDatabase.CreateAsset(settings, fullPath);
#endif
                }

                settings.Setup();
            }
            return settings;
        }

        //------------------------------------------------------------------------------------- LOG

        public static void LogIt(string message)
        {
            Debug.Log(GetLog(string.Empty, message));
        }

        public static void LogIt(string type, string message)
        {
            Debug.Log(GetLog(type, message));
        }

        public static void LogWarning(string message)
        {
            Debug.Log(GetLog(string.Empty, message, LogType.Warning));
        }

        public static void LogWarning(string type, string message)
        {
            Debug.Log(GetLog(type, message, LogType.Warning));
        }

        public static void LogError(string message)
        {
            Debug.Log(GetLog(string.Empty, message, LogType.Error));
        }

        public static void LogError(string type, string message)
        {
            Debug.Log(GetLog(type, message, LogType.Error));
        }

        public static string ToString(string message, LogType logType = LogType.Log)
        {
            return GetLog(string.Empty, message, logType);
        }

        public static string ToString(string type, string message, LogType logType = LogType.Log)
        {
            return GetLog(type, message, logType);
        }

        //------------------------------------------------------------------------------------- LOG

        private static string GetLog(string type, string message, LogType logType = LogType.Log)
        {
            // Prefix
            Color? prefixColor = Settings.GetColor(type);
            type = string.IsNullOrEmpty(type) ? string.Empty : "[" + type + "] ";
            string timePrefix = GetTimePrefix();
            string callerPrefix = GetCallerPrefix();
            string prefix = string.Format("{0}<b>{1}</b>{2}", timePrefix, type, callerPrefix);
            prefix = SetColor(prefix, prefixColor);

            // Quote: Find at least one quote mark
            if (Regex.IsMatch(message, Settings.QuoteMark))
            {
                // Look through each character in the message
                bool lookingForEndingQuoteMark = true;
                int secondQuoteMark = 0;
                for (int i = message.Length - 1; i > -1; i--)
                {
                    // Find either the start or the head of the quote, make sure one of the chars either side are spaces
                    if (message[i].ToString() == Settings.QuoteMark && FoundEmptyAroundChar(message, i))
                    {
                        // Save the second quote
                        if (lookingForEndingQuoteMark)
                        {
                            secondQuoteMark = i;
                        }
                        else
                        {
                            // Send the quote to the correct color
                            message = SetColorAt(message, i + 1, secondQuoteMark + 1);
                        }
                        lookingForEndingQuoteMark = !lookingForEndingQuoteMark;
                    }
                }
            }

            // Output
            string output = prefix + message;
            if (logType == LogType.Error)
            {
                return SetColor(output, Settings.ErrorColor);
            }
            else if (logType == LogType.Warning)
            {
                return SetColor(output, Settings.WarningColor);
            }
            else
            {
                return output;
            }
        }

        //------------------------------------------------------------------------------------- PREFIX

        private static string GetTimePrefix()
        {
            if (Settings.TimePrefix == LogSettings.eTimePrefixType.None)
            {
                return string.Empty;
            }

            string output = string.Empty;
            switch (Settings.TimePrefix)
            {
                case LogSettings.eTimePrefixType.Frame:
                    output = Time.frameCount.ToString();
                    break;
                case LogSettings.eTimePrefixType.Time:
                    output = Time.time.ToString();
                    break;
            }
            return output + " ";
        }

        private static string GetCallerPrefix()
        {
            if (Settings.CallerPrefix == LogSettings.eCallerPrefixType.None)
            {
                return string.Empty;
            }

            string output = string.Empty;
            string caller = Environment.StackTrace.Split('\n')[4];
            int indexOfPeriod = caller.IndexOf('.');

            switch (Settings.CallerPrefix)
            {
                case LogSettings.eCallerPrefixType.ClassName:
                    output = caller.Substring(6, indexOfPeriod - 6);
                    break;

                case LogSettings.eCallerPrefixType.MethodName:
                    output = caller.Substring(indexOfPeriod + 1, caller.IndexOf('(') - indexOfPeriod - 1);
                    break;

                case LogSettings.eCallerPrefixType.ClassAndMethod:
                    string theClass = caller.Substring(6, caller.IndexOf('.') - 6);
                    string theMethod = caller.Substring(indexOfPeriod + 1, caller.IndexOf('(') - indexOfPeriod - 1);
                    output = theClass + "." + theMethod;
                    break;
            }
            return output + " ";
        }

        //------------------------------------------------------------------------------------- COLOR

        private static string SetColor(string message, Color? color)
        {
            if (color == null)
            {
                return message;
            }
            else
            {
                return string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGB((Color)color), message);
            }
        }

        private static string SetColorAt(string message, int start, int end)
        {
            return string.Format("{0}{1}{2}", message.Substring(0, start), SetColor(message.Substring(start, end - start - 1), Settings.QuoteColor), message.Substring(end - 1));
        }

        //------------------------------------------------------------------------------------- UTILS

        // Check either side of a char in a string for a space, fullstop or commar
        private static bool FoundEmptyAroundChar(string stringToCheck, int index)
        {
            // If the index is at the start or the end of the string return true
            if (index + 1 >= stringToCheck.Length || index == 0)
            {
                return true;
            }
            else
            {
                // If it's surrounded by any of the chars below return true
                string[] valuesToFind = { " ", ",", ".", "-", "=", "<", ">" };
                foreach (string value in valuesToFind)
                {
                    if (stringToCheck[index + 1].ToString() == value || stringToCheck[index - 1].ToString() == value)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}