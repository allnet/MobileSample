using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class VectorSpritesShapeGroupSelection : EditorWindow {

    //Variables.
    VectorSpritesEditor vectorSpritesEditor;
    VectorSprites.VectorSpritesProperties vectorSpritesProperties;
    Vector2 scrollPosition = Vector2.zero;
    List<int> currentShapeGroups = new List<int>();
    List<int> currentShapes = new List<int>();

    //Initialise.
    public void initialise(VectorSpritesEditor vectorSpritesEditor, VectorSprites.VectorSpritesProperties vectorSpritesProperties) {
        this.vectorSpritesEditor = vectorSpritesEditor;
        this.vectorSpritesProperties = vectorSpritesProperties;
        for (int i = 0; i < vectorSpritesProperties.selectedEntities.Count; i++) {
            currentShapeGroups.Add(vectorSpritesProperties.selectedEntities[i].primaryID);
            currentShapes.Add(vectorSpritesProperties.selectedEntities[i].secondaryID);
        }
    }

    //Update.
    void Update() {

        //Close the window if the selection has changed.
        bool selectionChanged = vectorSpritesProperties.selectedEntity != VectorSprites.SelectableEntity.Shape ||
                vectorSpritesProperties.selectedEntities.Count != currentShapeGroups.Count;
        if (!selectionChanged)
            for (int i = 0; i < vectorSpritesProperties.selectedEntities.Count; i++)
                if (vectorSpritesProperties.selectedEntities[i].primaryID != currentShapeGroups[i] ||
                        vectorSpritesProperties.selectedEntities[i].secondaryID != currentShapes[i]) {
                    selectionChanged = true;
                    break;
                }
        if (selectionChanged)
            Close();
    }

    //Draw the GUI.
    void OnGUI() {

        //Display an instructions label.
        const int labelHeight = 60;
        GUIStyle labelStyleInstructions = new GUIStyle(GUI.skin.label);
        labelStyleInstructions.alignment = TextAnchor.MiddleCenter;
        labelStyleInstructions.fontStyle = FontStyle.Italic;
        labelStyleInstructions.normal.textColor = Color.gray;
        labelStyleInstructions.wordWrap = true;
        EditorGUI.LabelField(new Rect(8, 0, position.width - 16, labelHeight),
                "Click on the name of a shape group, below, to move the selected shape" + (vectorSpritesProperties.selectedEntities.Count == 1 ? "" : "s") +
                " into that group.", labelStyleInstructions);

        //Display a scroll view containing the shape groups, and detect the user clicking on one of them.
        scrollPosition = GUI.BeginScrollView(new Rect(0, labelHeight, position.width, position.height - labelHeight), scrollPosition,
                new Rect(0, 0, position.width - GUI.skin.verticalScrollbar.fixedWidth - 4,
                Mathf.Max((vectorSpritesProperties.shapeGroups.Count - 1) * 20, position.height - labelHeight)), false, true);
        float top = 0;
        for (int i = 0; i < vectorSpritesProperties.shapeGroups.Count; i++) {
            Rect labelRectangle = new Rect(0, top, position.width, 20);
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && labelRectangle.Contains(Event.current.mousePosition)) {
                vectorSpritesEditor.beginUndo("Change Shape Groups");
                for (int j = 0; j < currentShapeGroups.Count; j++) {
                    VectorSprites.Shape shape = vectorSpritesProperties.shapeGroups[currentShapeGroups[j]].shapes[currentShapes[j]];
                    vectorSpritesProperties.shapeGroups[currentShapeGroups[j]].shapes.RemoveAt(currentShapes[j]);
                    vectorSpritesProperties.shapeGroups[i].shapes.Add(shape);
                    vectorSpritesProperties.shapeGroups[currentShapeGroups[j]].expanded = true;
                    vectorSpritesProperties.shapeGroups[i].expanded = true;
                    for (int k = 0; k < vectorSpritesProperties.vectorSprites.Count; k++)
                        for (int l = 0; l < vectorSpritesProperties.vectorSprites[k].shapeGroupIDs.Count; l++) {
                            if (vectorSpritesProperties.vectorSprites[k].shapeGroupIDs[l] == currentShapeGroups[j] &&
                                    vectorSpritesProperties.vectorSprites[k].shapeIDs[l] == currentShapes[j]) {
                                vectorSpritesProperties.vectorSprites[k].shapeGroupIDs[l] = i;
                                vectorSpritesProperties.vectorSprites[k].shapeIDs[l] = vectorSpritesProperties.shapeGroups[i].shapes.Count - 1;
                            }
                            else if (vectorSpritesProperties.vectorSprites[k].shapeGroupIDs[l] == currentShapeGroups[j] &&
                                    vectorSpritesProperties.vectorSprites[k].shapeIDs[l] > currentShapes[j])
                                vectorSpritesProperties.vectorSprites[k].shapeIDs[l]--;
                        }
                    for (int k = 0; k < currentShapeGroups.Count; k++)
                        if (currentShapeGroups[k] == currentShapeGroups[j] && currentShapes[k] > currentShapes[j])
                            currentShapes[k]--;
                    currentShapeGroups[j] = i;
                    currentShapes[j] = vectorSpritesProperties.shapeGroups[i].shapes.Count - 1;
                }
                for (int j = 0; j < currentShapeGroups.Count; j++) {
                    vectorSpritesProperties.selectedEntities[j].primaryID = currentShapeGroups[j];
                    vectorSpritesProperties.selectedEntities[j].secondaryID = currentShapes[j];
                }
                vectorSpritesEditor.endUndo(true);
            }
            EditorGUI.LabelField(labelRectangle, vectorSpritesProperties.shapeGroups[i].name);
            top += 20;
        }
        GUI.EndScrollView();
    }
}