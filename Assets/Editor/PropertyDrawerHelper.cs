using UnityEditor;
using UnityEngine;

/* Helps in creating custom property drawers. Example use:
using UnityEditor;
using UnityEngine;
[CustomPropertyDrawer(typeof(PlanetResource))]
class PlanetResourceDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        string[] props = { "code", "planet3DPrefab", "thumb" };
        float[] weights = { 1, 4, 4 };
        PropertyDrawerHelper.Draw(position, property, label, props, weights);
    }
}*/
public class PropertyDrawerHelper
{
    public static void Draw(Rect position, SerializedProperty property, GUIContent label, string[] props, float[] weights, int padding = 5)
    {
        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.UpperRight;

        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Don't make child fields be indented
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // calculate weights
        var totalWeight = 0f;
        for (var i = 0; i < weights.Length; i++)
        {
            totalWeight += weights[i];
        }
        for (var i = 0; i < weights.Length; i++)
        {
            weights[i] = weights[i] / totalWeight;
        }

        // Calculate rects & draw fields - passs GUIContent.none to each so they are drawn without labels
        var x = position.x;
        var w = position.width - (padding * (props.Length - 1));
        for (var i = 0; i < props.Length; i++)
        {
            Rect rect = new Rect(x, position.y, w * weights[i], position.height);
            x += w * weights[i] + padding;
            if (props[i].IndexOf("!") == 0)
            {
                EditorGUI.LabelField(rect, props[i].Substring(1), style);
            }
            else
            {
                EditorGUI.PropertyField(rect, property.FindPropertyRelative(props[i]), GUIContent.none);
            }
        }

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}
