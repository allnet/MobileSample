using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Talespin.Debugging
{
    [CustomEditor(typeof(LogSettings))]
    public class LogSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            LogSettings t = (target as LogSettings);

            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("UTILS", EditorStyles.boldLabel);

            if (GUILayout.Button("Generate LogTypes")) CreateEnums(t);
        }

        //------------------------------------------------------------------------------------- PATH UTILS

        private void CreateEnums(LogSettings t)
        {
            // check namespace
            if (string.IsNullOrEmpty(t.LogTypesNamespace))
            {
                EditorUtility.DisplayDialog("Enter a namespace!", "You need to enter a namespace.", "OK!");
                return;
            }

            // get enum names
            string[] names = t.GetLogTypes();

            // generate class
            StringBuilder scriptData = new StringBuilder();
            scriptData.AppendLine("namespace " + t.LogTypesNamespace);
            scriptData.AppendLine("{");
            scriptData.AppendLine("    public static class LogTypes");
            scriptData.AppendLine("    {");
            for (int i = 0; i < names.Length; i++)
            {
                scriptData.AppendLine("        public const string " + names[i] + " = \"" + names[i] + "\";");
            }
            scriptData.AppendLine("    }");
            scriptData.AppendLine("}");

            // save file
            string filename = t.LogTypesNamespace + "LogTypes.cs";
            string path = Application.dataPath + @"/Scripts/";
            Directory.CreateDirectory(path);
            File.WriteAllText(path + filename, scriptData.ToString());
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Complete!", "LogTypes created.", "OK!");
        }
    }

    //------------------------------------------------------------------------------------- DRAWER

    [CustomPropertyDrawer(typeof(LogSettings.LogItem))]
    public class LogItemDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            string[] props = { "Type", "Color" };
            float[] weights = { 1, 1 };
            PropertyDrawerHelper.Draw(position, property, label, props, weights);
        }
    }
}