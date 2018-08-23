//using SubjectNerd.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace Talespin.Debugging
{
    [CreateAssetMenu(fileName = "TalespinLogSettings", menuName = "Talespin/LogSettings")]
    public class LogSettings : ScriptableObject
    {
        public static Color HexToRgbColor(String hexValue)
        {
            //string htmlValue = "#FF0000";
            Color newCol;
            if (ColorUtility.TryParseHtmlString(hexValue, out newCol))  //DH
            {
                return newCol;
            }

            return new Color();
        }

        //-------------------- CODE
        [Header("CODE")]
        [SerializeField]
        private string logTypesNamespace;
        public string LogTypesNamespace { get { return logTypesNamespace; } }

        //-------------------- PREFIX     
        [Header("PREFIX")]
        public eTimePrefixType TimePrefix = eTimePrefixType.None;
        public enum eTimePrefixType
        {
            None,
            Frame,
            Time
        }

        public eCallerPrefixType CallerPrefix = eCallerPrefixType.None;
        public enum eCallerPrefixType
        {
            None,
            ClassName,
            MethodName,
            ClassAndMethod
        }

        //-------------------- QUOTES     
        [Header("QUOTES")]
        public Color QuoteColor = HexToRgbColor("#0A89E8");
        public string QuoteMark = "'";

        //-------------------- COLORS     
        [Header("COLORS")]
        public Color WarningColor = HexToRgbColor("#FFFF77");
        public Color ErrorColor = HexToRgbColor("#FF6666");

        //[Reorderable]
        [SerializeField]
        private LogItem[] ItemColors = new LogItem[]
        {
            new LogItem("System", HexToRgbColor("#FF6464")),
            new LogItem("App", HexToRgbColor("#FFBD64")),
            new LogItem("Data", HexToRgbColor("#F6FF64")),
            new LogItem("Input", HexToRgbColor("#96FF64")),
            new LogItem("UI", HexToRgbColor("#64FFE4")),
            new LogItem("Network", HexToRgbColor("#64ADFF")),
            new LogItem("SFX", HexToRgbColor("#646FFF")),
            new LogItem("VFX", HexToRgbColor("#B864FF")),
            new LogItem("Graphics", HexToRgbColor("#FF64FA")),
        };
        private Dictionary<string, Color> Colors;

        [Serializable]
        public struct LogItem
        {
            public string Type;
            public Color Color;
            public LogItem(string type, Color color)
            {
                Type = type;
                Color = color;
            }
        }

        //-------------------------------------------------------------------------------------

        public void Setup()
        {
            Colors = new Dictionary<string, Color>();
            foreach (LogItem item in ItemColors)
            {
                if (!string.IsNullOrEmpty(item.Type))
                {
                    Colors.Add(item.Type.Trim().ToLower(), item.Color);
                }
            }
        }

        public string[] GetLogTypes()
        {
            List<string> names = new List<string>();
            foreach (LogItem item in ItemColors)
            {
                if (!string.IsNullOrEmpty(item.Type))
                {
                    names.Add(item.Type.Trim());
                }
            }
            return names.ToArray();
        }

        public Color? GetColor(string type)
        {
            return Colors.ContainsKey(type.ToLower()) ? Colors[type.ToLower()] : (Color?)null;
        }
       
    }
}