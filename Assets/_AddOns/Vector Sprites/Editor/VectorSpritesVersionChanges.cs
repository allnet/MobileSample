using UnityEditor;
using UnityEngine;

public class VectorSpritesVersionChanges : EditorWindow {

    //Draw the GUI.
    void OnGUI() {

        //Set label styles.
        GUIStyle headerLabel = new GUIStyle(GUI.skin.label);
        headerLabel.fontStyle = FontStyle.Bold;
        headerLabel.alignment = TextAnchor.MiddleCenter;
        GUIStyle boldLabel = new GUIStyle(GUI.skin.label);
        boldLabel.fontStyle = FontStyle.Bold;
        GUIStyle normalLabel = new GUIStyle(GUI.skin.label);
        normalLabel.wordWrap = true;

        //Display the version change text.
        EditorGUILayout.LabelField("Vector Sprites Version Changes", headerLabel);
        EditorGUILayout.GetControlRect(new GUILayoutOption[0]);
        EditorGUILayout.LabelField("Version 1.1.0", boldLabel);
        EditorGUILayout.LabelField("• Sprites can now optionally be associated with sprite sheets, and individual sprites or sprite sheets can be exported. " +
                "This means you are no longer restricted to exporting every sprite in the Vector Sprites component if you want to export a sprite sheet.",
                normalLabel);
        EditorGUILayout.LabelField("• Multiple entities of the same type (shape groups, shapes, sprite sheets or sprites) can now be selected at once. The " +
                "properties section displays a cut-down version of the available functionality when multiple entities are selected, but it does allow, for " +
                "example, multiple shapes to be deleted at once.", normalLabel);
        EditorGUILayout.LabelField("• Added the ability to zoom in on the Vector Sprites editor by up to 50 times to make editing small shapes easier.",
                normalLabel);
        EditorGUILayout.LabelField("• The transform option (for translating, rotating or scaling) can now be selected on a shape group to transform all " +
                "shapes within that group at once. Transform also supports multiple shape/shape group selections.", normalLabel);
        EditorGUILayout.LabelField("• Shapes, as well as shape groups, can now be associated with sprites. Associating a shape group now associates all of " +
                "the shapes within that group, including ones that are added in the future.", normalLabel);
        EditorGUILayout.LabelField("• Improved the grid and guideline colours for the Unity Personal Edition skin - previously they were hardly visible " +
                "against the light grey background.", normalLabel);
        EditorGUILayout.LabelField("• When sprites or sprite sheets are exported, the path within the Assets folder is remembered for each individual sprite " +
                "or sprite sheet so it can quickly be exported to the same place again.", normalLabel);
        EditorGUILayout.GetControlRect(new GUILayoutOption[0]);
        EditorGUILayout.LabelField("Version 1.0.0", boldLabel);
        EditorGUILayout.LabelField("Initial release.", normalLabel);
    }
}
