using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class VectorSpritesTransformShape : EditorWindow {

    //Constants.
    const float GUIWidth = 256;
    static readonly string[] originOptions = { "Shape Centre", "Sprite Centre" };
    static readonly int[] originValues = { 0, 1 };
    static readonly string[] transformOptions = { "Translate", "Rotate", "Scale" };
    static readonly int[] transformValues = { 0, 1, 2 };

    //Enumerated types.
    enum EnumeratedTypeNameArrays { TransformType, TransformOrigin };

    //Variables.
    VectorSpritesEditor vectorSpritesEditor;
    VectorSprites.VectorSpritesProperties vectorSpritesProperties;
    VectorSprites.SelectableEntity currentSelectableEntity;
    List<int> currentShapeGroups = new List<int>();
    List<int> currentShapes = new List<int>();
    static string[][] enumeratedTypeNames;
    static int[][] enumeratedTypeValues;

    //Initialise.
    public void initialise(VectorSpritesEditor vectorSpritesEditor, VectorSprites.VectorSpritesProperties vectorSpritesProperties) {
        this.vectorSpritesEditor = vectorSpritesEditor;
        this.vectorSpritesProperties = vectorSpritesProperties;
        currentSelectableEntity = vectorSpritesProperties.selectedEntity;
        for (int i = 0; i < vectorSpritesProperties.selectedEntities.Count; i++) {
            currentShapeGroups.Add(vectorSpritesProperties.selectedEntities[i].primaryID);
            currentShapes.Add(vectorSpritesProperties.selectedEntities[i].secondaryID);
        }

        //Get names and values for the enumerated types.
        enumeratedTypeNames = new string[Enum.GetNames(typeof(EnumeratedTypeNameArrays)).Length][];
        enumeratedTypeValues = new int[enumeratedTypeNames.Length][];
        for (int k = 0; k < enumeratedTypeNames.Length; k++) {
            enumeratedTypeNames[k] = Enum.GetNames(k == 0 ? typeof(VectorSprites.TransformType) : typeof(VectorSprites.TransformOrigin));
            enumeratedTypeValues[k] = new int[enumeratedTypeNames[k].Length];
            for (int i = 0; i < enumeratedTypeNames[k].Length; i++) {
                enumeratedTypeValues[k][i] = i;
                for (int j = 65; j <= 90; j++) {
                    enumeratedTypeNames[k][i] = enumeratedTypeNames[k][i].Replace(((char) j).ToString(), " " + ((char) j).ToString());
                }
                string[] enumeratedTypeNameWords = enumeratedTypeNames[k][i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                enumeratedTypeNames[k][i] = "";
                for (int j = 0; j < enumeratedTypeNameWords.Length; j++)
                    enumeratedTypeNames[k][i] += enumeratedTypeNameWords[j] + (enumeratedTypeNameWords[j].Length > 1 && j < enumeratedTypeNameWords.Length - 1 ?
                            " " : "");
            }
        }
    }

    //Update.
    void Update() {

        //Close the window if the selection has changed.
        bool selectionChanged = vectorSpritesProperties.selectedEntity != currentSelectableEntity ||
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

        //Start the controls at the top of the window and set various styles.
        float top = VectorSpritesEditor.rowGapPixels;
        GUIStyle labelStyleInstructions = new GUIStyle(GUI.skin.label);
        labelStyleInstructions.alignment = TextAnchor.UpperCenter;
        labelStyleInstructions.fontStyle = FontStyle.Italic;
        labelStyleInstructions.normal.textColor = Color.gray;
        labelStyleInstructions.wordWrap = true;
        GUIStyle enumPopupStyle = new GUIStyle(GUI.skin.GetStyle("popup"));
        enumPopupStyle.fixedHeight = VectorSpritesEditor.rowPixels;
        enumPopupStyle.padding.top = 1;
        enumPopupStyle.padding.bottom = 0;
        string shapeSingularOrPlural = "shape" + (currentSelectableEntity == VectorSprites.SelectableEntity.Shape &&
                vectorSpritesProperties.selectedEntities.Count == 1 ? "" : "s");

        //Allow the user to select the transform type.
        EditorGUI.LabelField(getGUIPercentageRectangle(2, 30, top, VectorSpritesEditor.rowPixels), new GUIContent("Transform",
                "Whether to translate, rotate or scale the " + shapeSingularOrPlural + ". Shapes can be transformed by specific values, or dragged in the " +
                "main window while this window is open."));
        vectorSpritesProperties.transformType = (VectorSprites.TransformType) EditorGUI.IntPopup(
                getGUIPercentageRectangle(32.5f, 64.5f, top, VectorSpritesEditor.rowPixels), (int) vectorSpritesProperties.transformType, transformOptions,
                transformValues, enumPopupStyle);
        top += VectorSpritesEditor.rowPixels + VectorSpritesEditor.rowGapPixels;

        //Allow the user to select the origin to rotate/scale from.
        if (vectorSpritesProperties.transformType == VectorSprites.TransformType.Rotate ||
                vectorSpritesProperties.transformType == VectorSprites.TransformType.Scale) {
            EditorGUI.LabelField(getGUIPercentageRectangle(2, 30, top, VectorSpritesEditor.rowPixels), new GUIContent("Origin",
                    "The origin to " + (vectorSpritesProperties.transformType == VectorSprites.TransformType.Rotate ? "rotate" : "scale") +
                    " around - either the centre of the sprite of the centre of the " + shapeSingularOrPlural + "."));
            vectorSpritesProperties.transformOrigin = (VectorSprites.TransformOrigin) EditorGUI.IntPopup(
                    getGUIPercentageRectangle(32.5f, 64.5f, top, VectorSpritesEditor.rowPixels), (int) vectorSpritesProperties.transformOrigin,
                    originOptions, originValues, enumPopupStyle);
            top += VectorSpritesEditor.rowPixels + VectorSpritesEditor.rowGapPixels;
        }

        //Translate the shape.
        if (vectorSpritesProperties.transformType == VectorSprites.TransformType.Translate) {
            vectorSpritesProperties.shapeTranslation = EditorGUI.Vector2Field(getGUIPercentageRectangle(2, 55, top, VectorSpritesEditor.rowPixels), "",
                    vectorSpritesProperties.shapeTranslation);
            if (GUI.Button(getGUIPercentageRectangle(60, 38, top, VectorSpritesEditor.rowPixels), new GUIContent("Translate",
                    "Translate (move) the " + shapeSingularOrPlural + " by the specified amount in the X and Y directions. One unit is the width of a " +
                    "sprite. Positive X values move the " + shapeSingularOrPlural + " to the right and positive Y values move the " + shapeSingularOrPlural +
                    " downwards.")))
                vectorSpritesEditor.translateShape(vectorSpritesProperties.shapeTranslation / 2, true, true);
            top += VectorSpritesEditor.rowPixels + VectorSpritesEditor.rowGapPixels;
            EditorGUI.LabelField(getGUIPercentageRectangle(5, 90, top, position.height),
                    "Select an amount to translate (move) the " + shapeSingularOrPlural + ", or drag the " + shapeSingularOrPlural + " in the main window.",
                    labelStyleInstructions);
        }

        //Rotate the shape.
        else if (vectorSpritesProperties.transformType == VectorSprites.TransformType.Rotate) {
            vectorSpritesProperties.shapeRotation = EditorGUI.FloatField(getGUIPercentageRectangle(2, 35, top, VectorSpritesEditor.rowPixels),
                    vectorSpritesProperties.shapeRotation);
            EditorGUI.LabelField(getGUIPercentageRectangle(38, 22, top, VectorSpritesEditor.rowPixels), "degrees");
            if (GUI.Button(getGUIPercentageRectangle(60, 38, top, VectorSpritesEditor.rowPixels), new GUIContent("Rotate",
                    "Rotate the " + shapeSingularOrPlural + " by the specified angle in degrees. The " + shapeSingularOrPlural + " will be rotated around " +
                    "the selected origin.")))
                vectorSpritesEditor.rotateShape(vectorSpritesProperties.shapeRotation,
                        vectorSpritesProperties.transformOrigin == VectorSprites.TransformOrigin.ShapeCentre, true, true, null);
            top += VectorSpritesEditor.rowPixels + VectorSpritesEditor.rowGapPixels;
            EditorGUI.LabelField(getGUIPercentageRectangle(5, 90, top, position.height),
                    "Select an angle to rotate the " + shapeSingularOrPlural + ", or drag the " + shapeSingularOrPlural + " in the main window.",
                    labelStyleInstructions);
        }

        //Scale the shape.
        else if (vectorSpritesProperties.transformType == VectorSprites.TransformType.Scale) {
            vectorSpritesProperties.shapeScale = EditorGUI.Vector2Field(getGUIPercentageRectangle(2, 55, top, VectorSpritesEditor.rowPixels), "",
                    vectorSpritesProperties.shapeScale);
            if (GUI.Button(getGUIPercentageRectangle(60, 38, top, VectorSpritesEditor.rowPixels), new GUIContent("Scale",
                    "Scale the " + shapeSingularOrPlural + " by the specified factor in the X and Y directions. The " + shapeSingularOrPlural +
                    " will be scaled from the selected origin.")))
                vectorSpritesEditor.scaleShape(vectorSpritesProperties.shapeScale,
                        vectorSpritesProperties.transformOrigin == VectorSprites.TransformOrigin.ShapeCentre, true, true);
            top += VectorSpritesEditor.rowPixels + VectorSpritesEditor.rowGapPixels;
            EditorGUI.LabelField(getGUIPercentageRectangle(5, 90, top, position.height),
                    "Select a scale factor to scale the " + shapeSingularOrPlural + ", or drag the " + shapeSingularOrPlural + " in the main window.",
                    labelStyleInstructions);
        }

        //Set the shape manipulation mode in the editor window.
        vectorSpritesEditor.shapeManipulation = (VectorSpritesEditor.ShapeManipulation) ((int) vectorSpritesProperties.transformType + 1);
        vectorSpritesEditor.transformOriginIsShapeCentre = vectorSpritesProperties.transformOrigin == VectorSprites.TransformOrigin.ShapeCentre;
    }

    //Called when the window is closed.
    void OnDestroy() {
        if (vectorSpritesEditor != null)
            vectorSpritesEditor.shapeManipulation = VectorSpritesEditor.ShapeManipulation.Points;
    }

    //Return a rectangle for adding GUI components to.
    Rect getGUIPercentageRectangle(float left, float width, float top, float height) {
        return new Rect((GUIWidth * (left / 100)), top, GUIWidth * (width / 100), height);
    }
}