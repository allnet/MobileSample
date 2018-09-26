using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(VectorSprites.VectorSpritesProperties))]
public class VectorSpritesPropertyDrawer : PropertyDrawer {

    //Get property height.
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        VectorSprites.VectorSpritesProperties vectorSpritesProperties = (VectorSprites.VectorSpritesProperties) fieldInfo.GetValue(
                property.serializedObject.targetObject);
        return (base.GetPropertyHeight(property, label) * (vectorSpritesProperties.updateVersion() ? 2.5f : 5)) + EditorGUIUtility.standardVerticalSpacing;
    }

    //On GUI.
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

        //Set the row height and initial position.
        float rowHeight = base.GetPropertyHeight(property, label);
        position = new Rect(position.xMin, position.yMin, position.width, rowHeight);
        rowHeight += EditorGUIUtility.standardVerticalSpacing;

        //Allow the user to edit the name of the Vector Sprites instance.
        property.Next(true);
        property.Next(false);
        property.stringValue = EditorGUI.TextField(position, EditorGUI.BeginProperty(position, new GUIContent("Name", "The name of this Vector Sprites " +
                "instance."), property), property.stringValue);
        EditorGUI.EndProperty();
        position = new Rect(position.xMin, position.yMin + rowHeight, position.width, position.height);

        //Display a button to open up the editor window, but only if not in play mode.
        VectorSprites.VectorSpritesProperties vectorSpritesProperties = (VectorSprites.VectorSpritesProperties) fieldInfo.GetValue(
                property.serializedObject.targetObject);
        if (vectorSpritesProperties.updateVersion()) {
            if (GUI.Button(new Rect(position.xMin, position.yMin, position.width, position.height * 1.5f), new GUIContent("Vector Sprites Editor",
                    "Show the Vector Sprites editor window for creating sprites."))) {
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                    EditorUtility.DisplayDialog("Play Mode", "The Vector Sprites Editor window cannot be displayed in play mode.", "OK");
                else {
                    VectorSpritesEditor vectorSpritesEditor = EditorWindow.GetWindow<VectorSpritesEditor>();
                    vectorSpritesEditor.minSize = new Vector3(1124, 596);
                    vectorSpritesEditor.title = "Vector Sprites";
                    vectorSpritesEditor.initialise(property.serializedObject.targetObject, vectorSpritesProperties);
                }
            }
        }
        else
            EditorGUI.HelpBox(new Rect(position.xMin, position.yMin, position.width, position.height * 4),
                    "This Vector Sprites component was last modified with Vector Sprites version " + vectorSpritesProperties.version +
                    ", but you have version " + VectorSprites.VectorSpritesProperties.currentVersion +
                    " installed. Please upgrade Vector Sprites to the latest version to continue using this component.", MessageType.Error);
    }
}
