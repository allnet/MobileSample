using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(VectorSpritesRenderer.CreatedFromVectorSpritesInstance))]
public class VectorSpritesRendererPropertyDrawer : PropertyDrawer {

    //Get property height.
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        property.Next(true);
        return property.boolValue ? 0 : (base.GetPropertyHeight(property, label) * 4) + EditorGUIUtility.standardVerticalSpacing;
    }

    //On GUI.
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        property.Next(true);
        if (!property.boolValue)
            EditorGUI.HelpBox(new Rect(position.xMin, position.yMin, position.width, base.GetPropertyHeight(property, label) * 4),
                    "Vector Sprites Renderers are used internally by Vector Sprites to create sprites. To use Vector Sprites, please remove this component " +
                    "and add a \"VectorSprites\" component.", MessageType.Error);
    }
}
