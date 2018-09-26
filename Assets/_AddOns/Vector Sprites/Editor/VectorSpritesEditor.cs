using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class VectorSpritesEditor : EditorWindow {

    //Enumerated Types.
    enum MouseOverObject { Point, StartTangent, EndTangent };
    enum EnumeratedTypeNameArrays { FillStyle, FillArea, NoiseType, EdgeShadingType, ShapeStyleType, SpriteRectangleTransform, AlphaBlendMode, Quality };
    enum Materials { Border, Grid, Icons, PillowAndGlowMesh, PillowAndGlowRenderTexture, Point, Shape };
    enum Icons { Expand, Contract, T, L, Translate, Rotate, Scale };
    public enum ShapeManipulation { Points, Translate, Rotate, Scale };

    //Constants.
    public const float rowPixels = 16;
    public const float rowGapPixels = 2;
    const float GUIWidth = 256;
    const float borderPixels = 8;
    const int maxPointsPerShape = 32;
    const float buttonHeightMultiplier = 1.25f;
    const int endPointSize = 16;
    const int tangentPointSize = 12;
    const float maxZoom = 50;

    //Variables.
    Material[] materials;
    string[][] enumeratedTypeNames = null;
    int[][] enumeratedTypeValues = null;
    List<Vector2> newShapePoints = new List<Vector2>();
    bool dragging = false, draggingPreviousFrame = false;
    Vector2 dragStart, nonSnappedDragStart;
    MouseOverObject mouseOverObject;
    int mouseOverPoint = -1;
    float sectionHeaderTop = 0;
    VectorSprites.VectorSpritesProperties vectorSpritesProperties;
    GameObject vectorSpritesRendererGameObject = null;
    VectorSpritesRenderer vectorSpritesRenderer;
    RenderTexture renderTexture = null;
    string newShapeName = "";
    float newShapeRotation = 0;
    UnityEngine.Object gameObject;
    string lastUndoEvent = "";
    int undoGroup = -1;
    SerializedObject serializedObject;
    bool contextClick;
    bool propertyBegun;
    public ShapeManipulation shapeManipulation = ShapeManipulation.Points;
    public bool transformOriginIsShapeCentre = false;
    bool waitForRightClickRelease = false;
    bool suppressNextContextEvent = false;
    Vector2 dragStartZoomCentre;

    //Initialise.
    public void initialise(UnityEngine.Object gameObject, VectorSprites.VectorSpritesProperties vectorSpritesProperties) {

        //Set the Vector Sprite game object and properties.
        this.gameObject = gameObject;
        this.vectorSpritesProperties = vectorSpritesProperties;

        //Load the shader assets and assign materials.
        string[] materialsEnumeratedTypeNames = Enum.GetNames(typeof(Materials));
        materials = new Material[materialsEnumeratedTypeNames.Length];
        for (int i = 0; i < materials.Length; i++) {
            for (int j = 65; j <= 90; j++)
                materialsEnumeratedTypeNames[i] = materialsEnumeratedTypeNames[i].Replace(((char) j).ToString(), " " + ((char) j).ToString());
            string shaderName = "Vector Sprites/" + materialsEnumeratedTypeNames[i].Trim();
            Shader shader = Shader.Find(shaderName);
            if (shader == null) {
                for (int k = 0; k < i; k++)
                    DestroyImmediate(materials[k]);
                Close();
                EditorUtility.DisplayDialog("Shader Not Found", "The shader \"" + shaderName + "\" could not be found. If some of Vector Sprite's resources " +
                        "have gone missing, you may need to re-install the asset.", "OK");
                return;
            }
            materials[i] = new Material(shader);
            materials[i].hideFlags = HideFlags.HideAndDontSave;
        }

        //Initialise the Vector Sprites Renderer game object, which has a camera attached to do the necessary rendering.
        vectorSpritesRendererGameObject = new GameObject("Vector Sprites Renderer");
        vectorSpritesRendererGameObject.hideFlags = HideFlags.HideAndDontSave;
        vectorSpritesRenderer = vectorSpritesRendererGameObject.AddComponent<VectorSpritesRenderer>();
        vectorSpritesRenderer.createdFromVectorSpritesInstance.flag = true;

        //Link up the undo/redo event.
        Undo.undoRedoPerformed += undoRedoPerformed;
    }

    //Repaint the GUI on every update.
    void Update() {

        //Close the editor window if the game object is null - e.g. if the game is run.
        if (gameObject == null || EditorApplication.isPlayingOrWillChangePlaymode)
            Close();
        else
            Repaint();
    }

    //Draw the GUI.
    void OnGUI() {

        //If the game object with the Vector Sprites instance attached has been deleted, don't render anything, and wait for the "Update()" method to close the
        //window.
        if (gameObject == null)
            return;

        //Convert the enumerated types to strings. This saves CPU time and excess garbage each frame that would be generated if an "enum" popup was used.
        if (enumeratedTypeNames == null || enumeratedTypeValues == null) {
            enumeratedTypeNames = new string[Enum.GetNames(typeof(EnumeratedTypeNameArrays)).Length][];
            enumeratedTypeValues = new int[enumeratedTypeNames.Length][];
            for (int k = 0; k < enumeratedTypeNames.Length; k++) {
                enumeratedTypeNames[k] = Enum.GetNames(k == 0 ? typeof(VectorSprites.FillStyle) : (k == 1 ? typeof(VectorSprites.FillArea) :
                        (k == 2 ? typeof(VectorSprites.NoiseType) : (k == 3 ? typeof(VectorSprites.EdgeShadingType) : (k == 4 ?
                        typeof(VectorSprites.ShapeStyleType) : (k == 5 ? typeof(VectorSprites.SpriteRectangleTransform) : (k == 6 ?
                        typeof(VectorSprites.AlphaBlendMode) : typeof(VectorSprites.Quality))))))));
                enumeratedTypeValues[k] = new int[enumeratedTypeNames[k].Length];
                for (int i = 0; i < enumeratedTypeNames[k].Length; i++) {
                    enumeratedTypeValues[k][i] = i;
                    for (int j = 65; j <= 90; j++) {
                        enumeratedTypeNames[k][i] = enumeratedTypeNames[k][i].Replace(((char) j).ToString(), " " + ((char) j).ToString());
                    }
                    string[] enumeratedTypeNameWords = enumeratedTypeNames[k][i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    enumeratedTypeNames[k][i] = "";
                    for (int j = 0; j < enumeratedTypeNameWords.Length; j++)
                        enumeratedTypeNames[k][i] += enumeratedTypeNameWords[j] + (enumeratedTypeNameWords[j].Length > 1 &&
                                j < enumeratedTypeNameWords.Length - 1 ? " " : "");
                }
            }
        }

        //Flag whether this event represents a context menu click. If so, this is most likely a "Revert Value to Prefab" operation, which causes certain meshes
        //and render textures to be refreshed. Need to record the event now because it could become "used" further down.
        contextClick = Event.current.type == EventType.ContextClick;
        if (suppressNextContextEvent && contextClick) {
            contextClick = false;
            suppressNextContextEvent = false;
        }

        //Store a serialized object of the game object for prefab purposes.
        serializedObject = new SerializedObject(gameObject);

        //Set the grid bounds.
        float size = Mathf.Min(position.width - (borderPixels * 4) - (GUIWidth * 2), position.height - (borderPixels * 2));
        Rect grid = new Rect((position.width / 2) - (size / 2), borderPixels, size, size);
        if (renderTexture == null || renderTexture.width != (int) grid.width || renderTexture.height != (int) grid.height) {
            if (renderTexture != null)
                DestroyImmediate(renderTexture);
            renderTexture = new RenderTexture((int) grid.width, (int) grid.height, 16, RenderTextureFormat.ARGB32);
            renderTexture.hideFlags = HideFlags.HideAndDontSave;
        }

        //Get the mouse position and detect whether the mouse is being pressed down on this frame for dragging detection purposes. The method can safely return
        //after a mouse down event - there is nothing else to process at this stage.
        Vector2 mousePosition = Event.current.mousePosition;
        bool mouseOutOfBounds = mousePosition.x < 0 || mousePosition.x >= position.width || mousePosition.y < 0 || mousePosition.y >= position.height;
        if (dragging) {
            if (mousePosition.x < grid.xMin + 10)
                mousePosition.x = grid.xMin + 10;
            else if (mousePosition.x > grid.xMax - 10)
                mousePosition.x = grid.xMax - 10;
            if (mousePosition.y < grid.yMin + 10)
                mousePosition.y = grid.yMin + 10;
            else if (mousePosition.y > grid.yMax - 10)
                mousePosition.y = grid.yMax - 10;
        }
        if (vectorSpritesProperties.snapToGrid)
            snapToGrid(ref mousePosition, grid, true);
        draggingPreviousFrame = dragging;
        if (mouseOutOfBounds) {
            dragging = false;
            draggingPreviousFrame = false;
            mouseOverPoint = -1;
            if (vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.Shape && vectorSpritesProperties.selectedEntities.Count == 1)
                vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[0].primaryID].shapes[vectorSpritesProperties.selectedEntities[0].
                        secondaryID].resetAllMeshes();
        }
        else if (Event.current.type == EventType.MouseDown) {
            if (Event.current.button == 0 && vectorSpritesProperties.selectedEntity != VectorSprites.SelectableEntity.NewShape &&
                    Event.current.mousePosition.x >= grid.xMin && Event.current.mousePosition.x <= grid.xMax &&
                    Event.current.mousePosition.y >= grid.yMin && Event.current.mousePosition.y <= grid.yMax && !waitForRightClickRelease) {
                dragging = true;
                dragStartZoomCentre = vectorSpritesProperties.zoomCentre;
                if (!draggingPreviousFrame) {
                    dragStart = mousePosition;
                    nonSnappedDragStart = Event.current.mousePosition;
                }
                draggingPreviousFrame = true;
            }
            else if (Event.current.button == 1) {
                if (dragging)
                    suppressNextContextEvent = true;
                dragging = false;
                draggingPreviousFrame = false;
                mouseOverPoint = -1;
                waitForRightClickRelease = true;
            }
        }
        else if (Event.current.type == EventType.MouseUp) {
            if (Event.current.button == 0)
                dragging = false;
            else if (Event.current.button == 1)
                waitForRightClickRelease = false;
        }

        //Zoom if the user moves the scroll wheel, or drag the window if the user is dragging the mouse and isn't over anything.
        if (Event.current.type == EventType.ScrollWheel && Event.current.mousePosition.x >= grid.xMin && Event.current.mousePosition.x <= grid.xMax &&
                Event.current.mousePosition.y >= grid.yMin && Event.current.mousePosition.y <= grid.yMax) {
            float oldZoom = vectorSpritesProperties.zoom;
            vectorSpritesProperties.zoom = Mathf.Clamp(vectorSpritesProperties.zoom - (Event.current.delta.y /
                    Mathf.Exp((maxZoom - vectorSpritesProperties.zoom) / (maxZoom / 4))), 1, maxZoom);
            Vector2 mouseGridPosition = transformPointTo01Space(Event.current.mousePosition, grid, false);
            vectorSpritesProperties.zoomCentre.x = (mouseGridPosition.x * (1 / oldZoom)) - (mouseGridPosition.x * (1 / vectorSpritesProperties.zoom)) +
                    vectorSpritesProperties.zoomCentre.x - (0.5f / oldZoom) + (0.5f / vectorSpritesProperties.zoom);
            vectorSpritesProperties.zoomCentre.y = (mouseGridPosition.y * (1 / oldZoom)) - (mouseGridPosition.y * (1 / vectorSpritesProperties.zoom)) +
                    vectorSpritesProperties.zoomCentre.y - (0.5f / oldZoom) + (0.5f / vectorSpritesProperties.zoom);
        }
        if (dragging && mouseOverPoint == -1 && shapeManipulation == ShapeManipulation.Points) {
            vectorSpritesProperties.zoomCentre.x = dragStartZoomCentre.x - (((Event.current.mousePosition.x - nonSnappedDragStart.x) / grid.width) *
                    (1 / vectorSpritesProperties.zoom));
            vectorSpritesProperties.zoomCentre.y = dragStartZoomCentre.y - (((Event.current.mousePosition.y - nonSnappedDragStart.y) / grid.height) *
                    (1 / vectorSpritesProperties.zoom));
        }
        vectorSpritesProperties.zoomCentre = new Vector2(
                Mathf.Clamp(vectorSpritesProperties.zoomCentre.x, 0.5f / vectorSpritesProperties.zoom, 1 - (0.5f / vectorSpritesProperties.zoom)),
                Mathf.Clamp(vectorSpritesProperties.zoomCentre.y, 0.5f / vectorSpritesProperties.zoom, 1 - (0.5f / vectorSpritesProperties.zoom)));

        //Draw the grid behind everything if required.
        if (!vectorSpritesProperties.gridOnTop && Event.current.type == EventType.Repaint)
            drawGrid(grid);

        //If a shape point or tangent is being dragged, flag the entire shape as being dirty so it can be recreated.
        if (vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.Shape && vectorSpritesProperties.selectedEntities.Count == 1 &&
                mouseOverPoint != -1)
            vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[0].primaryID].
                    shapes[vectorSpritesProperties.selectedEntities[0].secondaryID].resetAllMeshes();

        //Draw the meshes.
        if (Event.current.type == EventType.Repaint && (vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.Shape ||
                vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.ShapeGroup ||
                (vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.NewShape &&
                    vectorSpritesProperties.showOtherShapesWhenCreatingNewShape) ||
                vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.Sprite)) {

            //If rendering a specific shape or shape group...
            Vector2 oldPoint = Vector2.zero, centrePoint = Vector2.zero;
            if ((vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.ShapeGroup ||
                    vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.Shape) && vectorSpritesProperties.selectedEntities.Count > 0) {

                //If the end point or one of the tangents on a line in this shape is being dragged, temporarily adjust its position so the meshes can be drawn.
                VectorSprites.Shape selectedShape = vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.Shape ?
                        vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[0].primaryID].
                        shapes[vectorSpritesProperties.selectedEntities[0].secondaryID] : null;
                if (vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.Shape && vectorSpritesProperties.selectedEntities.Count == 1 &&
                        shapeManipulation == ShapeManipulation.Points && mouseOverPoint != -1) {
                    Vector2 dragDistance = new Vector2(((mousePosition.x - dragStart.x) / grid.width) / vectorSpritesProperties.zoom,
                            ((mousePosition.y - dragStart.y) / grid.height) / vectorSpritesProperties.zoom);
                    if (mouseOverObject == MouseOverObject.Point) {
                        oldPoint = selectedShape.shapePoints[mouseOverPoint].endPoint;
                        if (vectorSpritesProperties.snapToGrid)
                            snapToGrid(ref selectedShape.shapePoints[mouseOverPoint].endPoint, false);
                        selectedShape.shapePoints[mouseOverPoint].endPoint += dragDistance;
                    }
                    else if (mouseOverObject == MouseOverObject.StartTangent) {
                        oldPoint = selectedShape.shapePoints[mouseOverPoint].startTangent;
                        if (vectorSpritesProperties.snapToGrid)
                            snapToGrid(ref selectedShape.shapePoints[mouseOverPoint].startTangent, false);
                        selectedShape.shapePoints[mouseOverPoint].startTangent += dragDistance;
                    }
                    else if (mouseOverObject == MouseOverObject.EndTangent) {
                        oldPoint = selectedShape.shapePoints[mouseOverPoint].endTangent;
                        if (vectorSpritesProperties.snapToGrid)
                            snapToGrid(ref selectedShape.shapePoints[mouseOverPoint].endTangent, false);
                        selectedShape.shapePoints[mouseOverPoint].endTangent += dragDistance;
                    }
                }
                else if (shapeManipulation == ShapeManipulation.Translate && dragging) {
                    oldPoint = new Vector2(((mousePosition.x - dragStart.x) / grid.width) / vectorSpritesProperties.zoom,
                            ((mousePosition.y - dragStart.y) / grid.height) / vectorSpritesProperties.zoom);
                    translateShape(oldPoint, false, false);
                }
                else if (shapeManipulation == ShapeManipulation.Rotate && dragging) {
                    oldPoint.x = ((mousePosition.x - dragStart.x) / grid.width) * 360;
                    centrePoint = getCentrePoint(transformOriginIsShapeCentre);
                    rotateShape(oldPoint.x, transformOriginIsShapeCentre, false, false, centrePoint);
                }
                else if (shapeManipulation == ShapeManipulation.Scale && dragging) {
                    oldPoint = new Vector2(Mathf.Max(((mousePosition.x - dragStart.x) / grid.width) + 1, 0.01f),
                            Mathf.Max(((mousePosition.y - dragStart.y) / grid.height) + 1, 0.01f));
                    scaleShape(oldPoint, transformOriginIsShapeCentre, false, false);
                }
            }

            //Draw the shapes.
            vectorSpritesRenderer.render(vectorSpritesProperties, materials[(int) Materials.Shape], materials[(int) Materials.PillowAndGlowMesh],
                    materials[(int) Materials.PillowAndGlowRenderTexture], renderTexture, Vector2.one, true);
            Graphics.DrawTexture(grid, renderTexture);

            //If rendering a specific shape or shape group...
            if ((vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.ShapeGroup ||
                    vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.Shape) && vectorSpritesProperties.selectedEntities.Count > 0) {

                //Get the selected shape.
                VectorSprites.Shape selectedShape = vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.Shape ?
                        vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[0].primaryID].
                        shapes[vectorSpritesProperties.selectedEntities[0].secondaryID] : null;

                //If the end point or one of the tangents on a line in this shape is being dragged, restore its original position now that the meshes have been
                //drawn.
                if (shapeManipulation == ShapeManipulation.Points && vectorSpritesProperties.selectedEntities.Count == 1 && mouseOverPoint != -1) {
                    if (mouseOverObject == MouseOverObject.Point)
                        selectedShape.shapePoints[mouseOverPoint].endPoint = oldPoint;
                    else if (mouseOverObject == MouseOverObject.StartTangent)
                        selectedShape.shapePoints[mouseOverPoint].startTangent = oldPoint;
                    else if (mouseOverObject == MouseOverObject.EndTangent)
                        selectedShape.shapePoints[mouseOverPoint].endTangent = oldPoint;
                }
                else if (shapeManipulation == ShapeManipulation.Translate && dragging)
                    translateShape(-oldPoint, false, false);
                else if (shapeManipulation == ShapeManipulation.Rotate && dragging)
                    rotateShape(-oldPoint.x, transformOriginIsShapeCentre, false, false, centrePoint);
                else if (shapeManipulation == ShapeManipulation.Scale && dragging)
                    scaleShape(new Vector2(1 / oldPoint.x, 1 / oldPoint.y), transformOriginIsShapeCentre, false, false);
            }
        }

        //Draw the shape points. Need to do this on a mouse up event to check for when the user stops dragging a point.
        if ((Event.current.type == EventType.Repaint || Event.current.type == EventType.MouseUp || contextClick) &&
                (vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.Shape ||
                vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.ShapeGroup)) {

            //If manipulating the shape points...
            if (vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.Shape && vectorSpritesProperties.selectedEntities.Count == 1 &&
                    shapeManipulation == ShapeManipulation.Points) {

                //Get the selected shape.
                VectorSprites.Shape selectedShape = vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.Shape ?
                        vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[0].primaryID].
                        shapes[vectorSpritesProperties.selectedEntities[0].secondaryID] : null;

                //For the selected shape, draw handles on the points and the tangents.
                List<Vector3> pointPositions = new List<Vector3>();
                List<Color> pointColours = new List<Color>();
                Vector2 startPoint = zoomPoint(transformPointToGridBounds(selectedShape.shapePoints[selectedShape.shapePoints.Count - 1].endPoint, grid), grid);

                //Start by drawing the tangent points.
                Handles.color = Color.green;
                for (int i = 0; i < selectedShape.shapePoints.Count; i++) {

                    //Calculate the position of the end point of the line, adjusting it if it is being dragged.
                    Vector2 endPoint = zoomPoint(transformPointToGridBounds(selectedShape.shapePoints[i].endPoint, grid), grid);
                    bool mouseOverEndPoint = mouseOverPoint == -1 && Vector2.Distance(Event.current.mousePosition, endPoint) <= (endPointSize / 2);
                    if (dragging && (mouseOverEndPoint || (mouseOverPoint == i && mouseOverObject == MouseOverObject.Point))) {
                        if (vectorSpritesProperties.snapToGrid)
                            snapToGrid(ref endPoint, grid, true);
                        endPoint += mousePosition - dragStart;
                        mouseOverPoint = i;
                        mouseOverObject = MouseOverObject.Point;
                    }
                    else if (mouseOverPoint == i && mouseOverObject == MouseOverObject.Point && draggingPreviousFrame) {
                        if (vectorSpritesProperties.snapToGrid)
                            snapToGrid(ref endPoint, grid, true);
                        endPoint += mousePosition - dragStart;
                        beginUndo("Move Shape Point");
                        selectedShape.shapePoints[i].endPoint = transformPointTo01Space(endPoint, grid, true);
                        endUndo(true);
                        mouseOverPoint = -1;
                        selectedShape.resetAllMeshes();
                    }

                    //Queue up the end point of the line for drawing.
                    if (Event.current.type == EventType.Repaint) {
                        drawPoint(endPoint, endPointSize, mouseOverEndPoint ? Color.red : Color.yellow, grid);
                        pointPositions.Add(endPoint);
                        pointColours.Add(mouseOverEndPoint ? Color.red : Color.yellow);
                    }
                    else if (contextClick && mouseOverEndPoint) {
                        GenericMenu shapePointContextMenu = new GenericMenu();
                        shapePointContextMenu.AddItem(new GUIContent("Delete Point"), false, deletePoint, i);
                        shapePointContextMenu.AddSeparator("");
                        shapePointContextMenu.AddItem(new GUIContent("Insert Point After"), false, insertShapePointAfter, i);
                        shapePointContextMenu.AddItem(new GUIContent("Insert Point Before"), false, insertShapePointBefore, i);
                        shapePointContextMenu.ShowAsContext();
                        Event.current.Use();
                    }

                    //Calculate the position of the start tangent, adjusting it if it is being dragged.
                    Vector2 startTangent = zoomPoint(transformPointToGridBounds(selectedShape.shapePoints[i].startTangent, grid), grid);
                    bool mouseOverStartTangent = mouseOverPoint == -1 && Vector2.Distance(Event.current.mousePosition, startTangent) <= (tangentPointSize / 2);
                    if (dragging && (mouseOverStartTangent || (mouseOverPoint == i && mouseOverObject == MouseOverObject.StartTangent))) {
                        if (vectorSpritesProperties.snapToGrid)
                            snapToGrid(ref startTangent, grid, true);
                        startTangent += mousePosition - dragStart;
                        mouseOverPoint = i;
                        mouseOverObject = MouseOverObject.StartTangent;
                    }
                    else if (mouseOverPoint == i && mouseOverObject == MouseOverObject.StartTangent && draggingPreviousFrame) {
                        if (vectorSpritesProperties.snapToGrid)
                            snapToGrid(ref startTangent, grid, true);
                        startTangent += mousePosition - dragStart;
                        beginUndo("Move Shape Tangent");
                        selectedShape.shapePoints[i].startTangent = transformPointTo01Space(startTangent, grid, true);
                        endUndo(true);
                        mouseOverPoint = -1;
                        selectedShape.resetAllMeshes();
                    }

                    //Calculate the position of the end tangent, adjusting it if it is being dragged.
                    Vector2 endTangent = zoomPoint(transformPointToGridBounds(selectedShape.shapePoints[i].endTangent, grid), grid);
                    bool mouseOverEndTangent = mouseOverPoint == -1 && Vector2.Distance(Event.current.mousePosition, endTangent) <= (tangentPointSize / 2);
                    if (dragging && (mouseOverEndTangent || (mouseOverPoint == i && mouseOverObject == MouseOverObject.EndTangent))) {
                        if (vectorSpritesProperties.snapToGrid)
                            snapToGrid(ref endTangent, grid, true);
                        endTangent += mousePosition - dragStart;
                        mouseOverPoint = i;
                        mouseOverObject = MouseOverObject.EndTangent;
                    }
                    else if (mouseOverPoint == i && mouseOverObject == MouseOverObject.EndTangent && draggingPreviousFrame) {
                        if (vectorSpritesProperties.snapToGrid)
                            snapToGrid(ref endTangent, grid, true);
                        endTangent += mousePosition - dragStart;
                        beginUndo("Move Shape Tangent");
                        selectedShape.shapePoints[i].endTangent = transformPointTo01Space(endTangent, grid, true);
                        endUndo(true);
                        mouseOverPoint = -1;
                        selectedShape.resetAllMeshes();
                    }

                    //Draw the tangent points and the lines connecting them to the bezier curve.
                    if (Event.current.type == EventType.Repaint) {
                        drawLine(startTangent, VectorSpritesRenderer.getPointOnBezier(startPoint, endPoint, startTangent, endTangent, 0.3333f), grid);
                        drawPoint(startTangent, tangentPointSize, mouseOverStartTangent ? Color.red : Color.green, grid);
                        drawLine(endTangent, VectorSpritesRenderer.getPointOnBezier(startPoint, endPoint, startTangent, endTangent, 0.6667f), grid);
                        drawPoint(endTangent, tangentPointSize, mouseOverEndTangent ? Color.red : Color.green, grid);
                    }

                    //The start point for the next line is the end point of the previous one.
                    startPoint = endPoint;
                }
            }

            //...otherwise if transforming the shape, display the relevant icon on the shape itself...
            else if (shapeManipulation != ShapeManipulation.Points) {
                Vector2 min = new Vector2(float.MaxValue, float.MaxValue), max = new Vector2(float.MinValue, float.MinValue);
                for (int k = 0; k < vectorSpritesProperties.selectedEntities.Count; k++)
                    for (int j = 0; j < (vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.Shape ? 1 :
                            vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[k].primaryID].shapes.Count); j++) {
                        VectorSprites.Shape thisShape = vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.Shape ?
                                vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[k].primaryID].shapes[
                                vectorSpritesProperties.selectedEntities[k].secondaryID] :
                                vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[k].primaryID].shapes[j];
                        for (int i = 0; i < thisShape.shapePoints.Count; i++) {
                            if (thisShape.shapePoints[i].endPoint.x > max.x)
                                max.x = thisShape.shapePoints[i].endPoint.x;
                            if (thisShape.shapePoints[i].endPoint.y > max.y)
                                max.y = thisShape.shapePoints[i].endPoint.y;
                            if (thisShape.shapePoints[i].endPoint.x < min.x)
                                min.x = thisShape.shapePoints[i].endPoint.x;
                            if (thisShape.shapePoints[i].endPoint.y < min.y)
                                min.y = thisShape.shapePoints[i].endPoint.y;
                        }
                    }
                Vector2 shapeCentre = zoomPoint(transformPointToGridBounds((min + max) / 2, grid) + (shapeManipulation == ShapeManipulation.Translate &&
                        dragging ? (mousePosition - dragStart) / vectorSpritesProperties.zoom : Vector2.zero), grid);
                shapeCentre.x = Mathf.Clamp(shapeCentre.x, grid.xMin + 18, grid.xMax - 18);
                shapeCentre.y = Mathf.Clamp(shapeCentre.y, grid.yMin + 18, grid.yMax - 18);
                drawIcon(Icons.Translate + (int) shapeManipulation - 1, new Rect(shapeCentre.x - 16, shapeCentre.y - 16, 32, 32), 0, 32);
                if (draggingPreviousFrame && !dragging) {
                    if (shapeManipulation == ShapeManipulation.Translate)
                        translateShape(new Vector2(((mousePosition.x - dragStart.x) / grid.width) / vectorSpritesProperties.zoom,
                                ((mousePosition.y - dragStart.y) / grid.height) / vectorSpritesProperties.zoom), true, true);
                    else if (shapeManipulation == ShapeManipulation.Rotate)
                        rotateShape(((mousePosition.x - dragStart.x) / grid.width) * 360, transformOriginIsShapeCentre, true, true, null);
                    else if (shapeManipulation == ShapeManipulation.Scale)
                        scaleShape(new Vector2(Mathf.Max(((mousePosition.x - dragStart.x) / grid.width) + 1, 0.01f),
                                Mathf.Max(((mousePosition.y - dragStart.y) / grid.height) + 1, 0.01f)), transformOriginIsShapeCentre, true, true);
                }
            }
        }

        //Handle adding new shapes, which only occurs on repaint and mouse down events.
        if (vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.NewShape && (Event.current.type == EventType.Repaint ||
                Event.current.type == EventType.MouseDown)) {

            //Draw shapes that are currently being added. Start by drawing any points that have already been added.
            Vector2 previousDrawingPosition = Vector2.zero, drawingPosition;
            Handles.color = Color.white;
            for (int i = 0; i < newShapePoints.Count; i++) {
                drawingPosition = zoomPoint(transformPointToGridBounds(newShapePoints[i], grid), grid);
                drawPoint(drawingPosition, endPointSize, Color.yellow, grid);
                if (i > 0)
                    drawLine(previousDrawingPosition, drawingPosition, grid);
                previousDrawingPosition = drawingPosition;
            }

            //Draw the next point to be added, underneath the mouse pointer.
            if (Event.current.mousePosition.x >= grid.xMin + 10 && Event.current.mousePosition.x <= grid.xMax - 10 &&
                    Event.current.mousePosition.y >= grid.yMin + 10 && Event.current.mousePosition.y <= grid.yMax - 10) {
                drawPoint(mousePosition, endPointSize, Color.red, grid);
                if (newShapePoints.Count > 0)
                    drawLine(previousDrawingPosition, mousePosition, grid);
                if (Event.current.type == EventType.MouseDown) {
                    if (Event.current.button == 0) {
                        if (newShapePoints.Count >= maxPointsPerShape)
                            EditorUtility.DisplayDialog("New Shape", "Shapes cannot have more than " + maxPointsPerShape.ToString() + " points.", "OK");
                        else
                            newShapePoints.Add(transformPointTo01Space(mousePosition, grid, true));
                    }
                    else if (Event.current.button == 1) {
                        createShape();
                        suppressNextContextEvent = true;
                    }
                }
            }
        }

        //Draw the grid on top of everything if required.
        if (vectorSpritesProperties.gridOnTop && Event.current.type == EventType.Repaint)
            drawGrid(grid);

        //Draw a border around the grid and over all shapes.
        drawBorder(grid, Color.white);

        //Set various styles for the various controls.
        GUIStyle labelStyleInstructions = new GUIStyle(GUI.skin.label);
        labelStyleInstructions.alignment = TextAnchor.MiddleCenter;
        labelStyleInstructions.fontStyle = FontStyle.Italic;
        labelStyleInstructions.normal.textColor = Color.gray;
        labelStyleInstructions.wordWrap = true;
        GUIStyle labelStyleHyperlink = new GUIStyle(GUI.skin.label);
        labelStyleHyperlink.alignment = TextAnchor.MiddleCenter;
        labelStyleHyperlink.normal.textColor = new Color(0, 0.4f, 0.8f);
        labelStyleHyperlink.fontStyle = FontStyle.Bold;
        GUIStyle treeViewNode = new GUIStyle(GUI.skin.label);
        treeViewNode.alignment = TextAnchor.UpperLeft;
        GUIStyle emptyTreeViewNode = new GUIStyle(treeViewNode);
        emptyTreeViewNode.normal.textColor = Color.gray;
        emptyTreeViewNode.fontStyle = FontStyle.Italic;
        GUIStyle treeViewGroup = new GUIStyle(treeViewNode);
        treeViewGroup.fontStyle = FontStyle.Bold;
        GUIStyle treeViewNodeSelected = new GUIStyle(treeViewNode);
        treeViewNodeSelected.normal.textColor = Color.red;
        GUIStyle treeViewGroupSelected = new GUIStyle(treeViewGroup);
        treeViewGroupSelected.normal.textColor = Color.red;
        GUIStyle enumPopupStyle = new GUIStyle(GUI.skin.GetStyle("popup"));
        enumPopupStyle.fixedHeight = rowPixels;
        enumPopupStyle.padding.top = 1;
        enumPopupStyle.padding.bottom = 0;
        GUIStyle zoomAmountStyle = new GUIStyle(GUI.skin.label);
        zoomAmountStyle.alignment = TextAnchor.MiddleRight;

        //Draw the hierarchy of shape groups and shapes on the left.
        sectionHeaderTop = 0;
        int settingsHeight = (int) ((rowPixels * 5) + (rowPixels * buttonHeightMultiplier) + (rowGapPixels * 5) + borderPixels);
        int height = (int) ((position.height - settingsHeight - (borderPixels * 4)) / 2);
        float top = sectionHeader("Shapes Hierarchy", height, true);
        if (GUI.Button(getGUIPercentageRectangle(true, 5, 42.5f, top, rowPixels * buttonHeightMultiplier), new GUIContent("Add Group",
                "Adds a shape group."))) {
            beginUndo("Add Shape Group");
            vectorSpritesProperties.shapeGroups.Add(new VectorSprites.ShapeGroup(getNextFreeShapeGroupName()));
            vectorSpritesProperties.selectedEntity = VectorSprites.SelectableEntity.ShapeGroup;
            vectorSpritesProperties.selectedEntities.Clear();
            vectorSpritesProperties.selectedEntities.Add(new VectorSprites.SelectedEntity(vectorSpritesProperties.shapeGroups.Count - 1));
            endUndo(true);
        }
        EditorGUI.BeginDisabledGroup(vectorSpritesProperties.selectedEntities.Count != 1 ||
                (vectorSpritesProperties.selectedEntity != VectorSprites.SelectableEntity.ShapeGroup &&
                vectorSpritesProperties.selectedEntity != VectorSprites.SelectableEntity.Shape));
        if (GUI.Button(getGUIPercentageRectangle(true, 52.5f, 42.5f, top, rowPixels * buttonHeightMultiplier), new GUIContent("Add Shape",
                "Adds a shape to the currently-selected group."))) {
            vectorSpritesProperties.selectedEntity = VectorSprites.SelectableEntity.NewShape;
            while (vectorSpritesProperties.selectedEntities.Count > 1)
                vectorSpritesProperties.selectedEntities.RemoveAt(0);
            vectorSpritesProperties.selectedEntities[0].secondaryID = -1;
            newShapeName = getNextFreeShapeName();
            newShapePoints.Clear();
            newShapeRotation = 0;
        }
        EditorGUI.EndDisabledGroup();
        top += (rowPixels * buttonHeightMultiplier) + (rowGapPixels * 2);
        float scrollPositionHeight = sectionHeaderTop - top - 2;
        float scrollAreaHeight = 0;
        for (int i = 0; i < vectorSpritesProperties.shapeGroups.Count; i++) {
            scrollAreaHeight += 20;
            if (vectorSpritesProperties.shapeGroups[i].expanded)
                scrollAreaHeight += Math.Max(vectorSpritesProperties.shapeGroups[i].shapes.Count, 1) * 20;
        }
        vectorSpritesProperties.shapesScrollPosition = GUI.BeginScrollView(new Rect(borderPixels + 2, top, GUIWidth - 4, scrollPositionHeight),
                vectorSpritesProperties.shapesScrollPosition, new Rect(0, 0, GUIWidth - GUI.skin.verticalScrollbar.fixedWidth - 4,
                Mathf.Max(scrollAreaHeight, sectionHeaderTop - top - 2)), false, true);
        top = 0;
        Rect labelRectangle;
        for (int i = 0; i < vectorSpritesProperties.shapeGroups.Count; i++) {

            //Draw the expand/contract icon and the shape group name. Detect clicking on the icon to expand/contract the group, and on the group itself to
            //select it.
            Rect textureRectangle = new Rect(5, top - 2, 20, 20);
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && textureRectangle.Contains(Event.current.mousePosition)) {
                beginUndo((vectorSpritesProperties.shapeGroups[i].expanded ? "Contract" : "Expand") + " Group");
                vectorSpritesProperties.shapeGroups[i].expanded = !vectorSpritesProperties.shapeGroups[i].expanded;
                endUndo(true);
            }
            drawIcon(vectorSpritesProperties.shapeGroups[i].expanded ? Icons.Contract : Icons.Expand, textureRectangle,
                    (int) (textureRectangle.yMax - vectorSpritesProperties.shapesScrollPosition.y - scrollPositionHeight),
                    (int) (textureRectangle.yMax - vectorSpritesProperties.shapesScrollPosition.y));
            labelRectangle = new Rect(24, top, GUIWidth, 20);
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && labelRectangle.Contains(Event.current.mousePosition)) {
                beginUndo("Select Shape Group");
                if (!Event.current.control || vectorSpritesProperties.selectedEntity != VectorSprites.SelectableEntity.ShapeGroup)
                    vectorSpritesProperties.selectedEntities.Clear();
                vectorSpritesProperties.selectedEntity = VectorSprites.SelectableEntity.ShapeGroup;
                bool alreadySelected = false;
                for (int j = 0; j < vectorSpritesProperties.selectedEntities.Count; j++)
                    if (vectorSpritesProperties.selectedEntities[j].primaryID == i) {
                        alreadySelected = true;
                        vectorSpritesProperties.selectedEntities.RemoveAt(j);
                        break;
                    }
                if (!alreadySelected)
                    vectorSpritesProperties.selectedEntities.Add(new VectorSprites.SelectedEntity(i));
                endUndo(true);
                GUIUtility.keyboardControl = 0;
            }
            bool isSelected = false;
            if (vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.ShapeGroup)
                for (int j = 0; j < vectorSpritesProperties.selectedEntities.Count; j++)
                    if (vectorSpritesProperties.selectedEntities[j].primaryID == i) {
                        isSelected = true;
                        break;
                    }
            EditorGUI.LabelField(new Rect(24, top, GUIWidth, 20), vectorSpritesProperties.shapeGroups[i].name, isSelected ? treeViewGroupSelected :
                    treeViewGroup);
            top += 20;

            //If this group is expanded, draw the shapes contained within it, and detect selection.
            if (vectorSpritesProperties.shapeGroups[i].expanded) {

                //If there are no shapes in the group, display a message indicating this, and allow clicking on it to select the shape group anyway.
                if (vectorSpritesProperties.shapeGroups[i].shapes.Count == 0) {
                    labelRectangle = new Rect(24, top, GUIWidth, 20);
                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && labelRectangle.Contains(Event.current.mousePosition)) {
                        beginUndo("Select Shape Group");
                        if (!Event.current.control || vectorSpritesProperties.selectedEntity != VectorSprites.SelectableEntity.ShapeGroup)
                            vectorSpritesProperties.selectedEntities.Clear();
                        vectorSpritesProperties.selectedEntity = VectorSprites.SelectableEntity.ShapeGroup;
                        bool alreadySelected = false;
                        for (int j = 0; j < vectorSpritesProperties.selectedEntities.Count; j++)
                            if (vectorSpritesProperties.selectedEntities[j].primaryID == i) {
                                alreadySelected = true;
                                vectorSpritesProperties.selectedEntities.RemoveAt(j);
                                break;
                            }
                        if (!alreadySelected)
                            vectorSpritesProperties.selectedEntities.Add(new VectorSprites.SelectedEntity(i));
                        endUndo(true);
                        GUIUtility.keyboardControl = 0;
                    }
                    EditorGUI.LabelField(labelRectangle, "No shapes in this group!", emptyTreeViewNode);
                    top += 20;
                }

                //Loop over and display the shapes in this group.
                for (int j = 0; j < vectorSpritesProperties.shapeGroups[i].shapes.Count; j++) {
                    textureRectangle = new Rect(25, top - 2, 20, 20);
                    drawIcon(j == vectorSpritesProperties.shapeGroups[i].shapes.Count - 1 ? Icons.L : Icons.T, textureRectangle,
                            (int) (textureRectangle.yMax - vectorSpritesProperties.shapesScrollPosition.y - scrollPositionHeight),
                            (int) (textureRectangle.yMax - vectorSpritesProperties.shapesScrollPosition.y));
                    labelRectangle = new Rect(44, top, GUIWidth, 20);
                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && labelRectangle.Contains(Event.current.mousePosition)) {
                        beginUndo("Select Shape");
                        if (!Event.current.control || vectorSpritesProperties.selectedEntity != VectorSprites.SelectableEntity.Shape)
                            vectorSpritesProperties.selectedEntities.Clear();
                        vectorSpritesProperties.selectedEntity = VectorSprites.SelectableEntity.Shape;
                        bool alreadySelected = false;
                        for (int k = 0; k < vectorSpritesProperties.selectedEntities.Count; k++)
                            if (vectorSpritesProperties.selectedEntities[k].primaryID == i && vectorSpritesProperties.selectedEntities[k].secondaryID == j) {
                                alreadySelected = true;
                                vectorSpritesProperties.selectedEntities.RemoveAt(k);
                                break;
                            }
                        if (!alreadySelected)
                            vectorSpritesProperties.selectedEntities.Add(new VectorSprites.SelectedEntity(i, j));
                        endUndo(true);
                        GUIUtility.keyboardControl = 0;
                    }
                    isSelected = false;
                    if (vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.Shape)
                        for (int k = 0; k < vectorSpritesProperties.selectedEntities.Count; k++)
                            if (vectorSpritesProperties.selectedEntities[k].primaryID == i && vectorSpritesProperties.selectedEntities[k].secondaryID == j) {
                                isSelected = true;
                                break;
                            }
                    EditorGUI.LabelField(labelRectangle, vectorSpritesProperties.shapeGroups[i].shapes[j].name, isSelected ? treeViewNodeSelected :
                            treeViewNode);
                    top += 20;
                }
            }
        }
        GUI.EndScrollView(true);

        //Draw the list of sprites on the left.
        top = sectionHeader("Sprites Hierarchy", height, true);
        if (GUI.Button(getGUIPercentageRectangle(true, 5, 42.5f, top, rowPixels * buttonHeightMultiplier), new GUIContent("Add Sprite Sheet",
                "Adds a sprite sheet."))) {
            beginUndo("Add Sprite Sheet");
            vectorSpritesProperties.spriteSheets.Add(new VectorSprites.SpriteSheet(getNextFreeSpriteSheetName()));
            vectorSpritesProperties.selectedEntity = VectorSprites.SelectableEntity.SpriteSheet;
            vectorSpritesProperties.selectedEntities.Clear();
            vectorSpritesProperties.selectedEntities.Add(new VectorSprites.SelectedEntity(vectorSpritesProperties.spriteSheets.Count - 1));
            endUndo(true);
        }
        if (GUI.Button(getGUIPercentageRectangle(true, 52.5f, 42.5f, top, rowPixels * buttonHeightMultiplier), new GUIContent("Add Sprite",
                "Adds a sprite."))) {
            beginUndo("Add Sprite");
            vectorSpritesProperties.vectorSprites.Add(new VectorSprites.VectorSprite(getNextFreeSpriteName()));
            vectorSpritesProperties.selectedEntity = VectorSprites.SelectableEntity.Sprite;
            vectorSpritesProperties.selectedEntities.Clear();
            vectorSpritesProperties.selectedEntities.Add(new VectorSprites.SelectedEntity(vectorSpritesProperties.vectorSprites.Count - 1));
            endUndo(true);
        }
        top += (rowPixels * buttonHeightMultiplier) + (rowGapPixels * 2);

        //Set up the sprites hierarchy scroll area.
        int spritesHierarchyHeight = 0;
        for (int i = 0; i < vectorSpritesProperties.spriteSheets.Count; i++) {
            spritesHierarchyHeight += 20;
            if (vectorSpritesProperties.spriteSheets[i].expanded) {
                bool atLeastOneSprite = false;
                for (int j = 0; j < vectorSpritesProperties.vectorSprites.Count; j++)
                    if (vectorSpritesProperties.vectorSprites[j].spriteSheet == i) {
                        spritesHierarchyHeight += 20;
                        atLeastOneSprite = true;
                    }
                if (!atLeastOneSprite)
                    spritesHierarchyHeight += 20;
            }
        }
        for (int j = 0; j < vectorSpritesProperties.vectorSprites.Count; j++)
            if (vectorSpritesProperties.vectorSprites[j].spriteSheet == -1)
                spritesHierarchyHeight += 20;
        vectorSpritesProperties.spritesScrollPosition = GUI.BeginScrollView(new Rect(borderPixels + 2, top, GUIWidth - 4, sectionHeaderTop - top - 2),
                vectorSpritesProperties.spritesScrollPosition, new Rect(0, 0, GUIWidth - GUI.skin.verticalScrollbar.fixedWidth - 4,
                Mathf.Max(spritesHierarchyHeight, sectionHeaderTop - top - 2)), false, true);
        top = 0;

        //Loop over the sprite sheets in order to display them and their child sprites.
        for (int k = 0; k <= vectorSpritesProperties.spriteSheets.Count; k++) {
            if (k < vectorSpritesProperties.spriteSheets.Count) {

                //Display the expand/contract icon for the sprite sheet.
                Rect textureRectangle = new Rect(5, top - 2, 20, 20);
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && textureRectangle.Contains(Event.current.mousePosition)) {
                    beginUndo((vectorSpritesProperties.spriteSheets[k].expanded ? "Contract" : "Expand") + " Sprite Sheet");
                    vectorSpritesProperties.spriteSheets[k].expanded = !vectorSpritesProperties.spriteSheets[k].expanded;
                    endUndo(true);
                }
                drawIcon(vectorSpritesProperties.spriteSheets[k].expanded ? Icons.Contract : Icons.Expand, textureRectangle,
                        (int) (textureRectangle.yMax - vectorSpritesProperties.spritesScrollPosition.y - scrollPositionHeight),
                        (int) (textureRectangle.yMax - vectorSpritesProperties.spritesScrollPosition.y));

                //Display the sprite sheet name. Allow clicking on it to select the sprite sheet.
                labelRectangle = new Rect(24, top, GUIWidth, 20);
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && labelRectangle.Contains(Event.current.mousePosition)) {
                    beginUndo("Select Sprite Sheet");
                    if (!Event.current.control || vectorSpritesProperties.selectedEntity != VectorSprites.SelectableEntity.SpriteSheet)
                        vectorSpritesProperties.selectedEntities.Clear();
                    vectorSpritesProperties.selectedEntity = VectorSprites.SelectableEntity.SpriteSheet;
                    bool alreadySelected = false;
                    for (int j = 0; j < vectorSpritesProperties.selectedEntities.Count; j++)
                        if (vectorSpritesProperties.selectedEntities[j].primaryID == k) {
                            alreadySelected = true;
                            vectorSpritesProperties.selectedEntities.RemoveAt(j);
                            break;
                        }
                    if (!alreadySelected)
                        vectorSpritesProperties.selectedEntities.Add(new VectorSprites.SelectedEntity(k));
                    endUndo(true);
                    GUIUtility.keyboardControl = 0;
                }
                bool isSelected = false;
                if (vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.SpriteSheet)
                    for (int j = 0; j < vectorSpritesProperties.selectedEntities.Count; j++)
                        if (vectorSpritesProperties.selectedEntities[j].primaryID == k) {
                            isSelected = true;
                            break;
                        }
                EditorGUI.LabelField(labelRectangle, vectorSpritesProperties.spriteSheets[k].name, isSelected ? treeViewNodeSelected : treeViewNode);
                top += 20;
            }

            //If the sprite sheet is expanded but there are no sprites associated with it, display an appropriate message. Allow clicking on the message to
            //select the sprite sheet anyway.
            int indexOfLastSpriteAssociatedWithThisSpriteSheet = -1;
            for (int i = vectorSpritesProperties.vectorSprites.Count - 1; i >= 0; i--)
                if (vectorSpritesProperties.vectorSprites[i].spriteSheet == k) {
                    indexOfLastSpriteAssociatedWithThisSpriteSheet = i;
                    break;
                }
            if (k < vectorSpritesProperties.spriteSheets.Count && vectorSpritesProperties.spriteSheets[k].expanded) {                
                if (indexOfLastSpriteAssociatedWithThisSpriteSheet == -1) {
                    labelRectangle = new Rect(24, top, GUIWidth, 20);
                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && labelRectangle.Contains(Event.current.mousePosition)) {
                        beginUndo("Select Sprite Sheet");
                        if (!Event.current.control || vectorSpritesProperties.selectedEntity != VectorSprites.SelectableEntity.SpriteSheet)
                            vectorSpritesProperties.selectedEntities.Clear();
                        vectorSpritesProperties.selectedEntity = VectorSprites.SelectableEntity.SpriteSheet;
                        bool alreadySelected = false;
                        for (int j = 0; j < vectorSpritesProperties.selectedEntities.Count; j++)
                            if (vectorSpritesProperties.selectedEntities[j].primaryID == k) {
                                alreadySelected = true;
                                vectorSpritesProperties.selectedEntities.RemoveAt(j);
                                break;
                            }
                        if (!alreadySelected)
                            vectorSpritesProperties.selectedEntities.Add(new VectorSprites.SelectedEntity(k));
                        endUndo(true);
                        GUIUtility.keyboardControl = 0;
                    }
                    EditorGUI.LabelField(labelRectangle, "No sprites in this sprite sheet!", emptyTreeViewNode);
                    top += 20;
                }
            }

            //Loop over the sprites associted with this sprite sheet if it is expanded (or the sprites associated with no sprite sheet for the final iteration).
            if (k == vectorSpritesProperties.spriteSheets.Count || vectorSpritesProperties.spriteSheets[k].expanded)
                for (int i = 0; i < vectorSpritesProperties.vectorSprites.Count; i++) {
                    if (vectorSpritesProperties.vectorSprites[i].spriteSheet != k && (vectorSpritesProperties.vectorSprites[i].spriteSheet != -1 ||
                            k != vectorSpritesProperties.spriteSheets.Count))
                        continue;
                    if (k < vectorSpritesProperties.spriteSheets.Count) {
                        Rect textureRectangle = new Rect(25, top - 2, 20, 20);
                        drawIcon(i == indexOfLastSpriteAssociatedWithThisSpriteSheet ? Icons.L : Icons.T, textureRectangle,
                                (int) (textureRectangle.yMax - vectorSpritesProperties.spritesScrollPosition.y - scrollPositionHeight),
                                (int) (textureRectangle.yMax - vectorSpritesProperties.spritesScrollPosition.y));
                    }
                    labelRectangle = new Rect(k < vectorSpritesProperties.spriteSheets.Count ? 44 : 24, top, GUIWidth, 20);
                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && labelRectangle.Contains(Event.current.mousePosition)) {
                        beginUndo("Select Sprite");
                        if (!Event.current.control || vectorSpritesProperties.selectedEntity != VectorSprites.SelectableEntity.Sprite)
                            vectorSpritesProperties.selectedEntities.Clear();
                        vectorSpritesProperties.selectedEntity = VectorSprites.SelectableEntity.Sprite;
                        bool alreadySelected = false;
                        for (int j = 0; j < vectorSpritesProperties.selectedEntities.Count; j++)
                            if (vectorSpritesProperties.selectedEntities[j].primaryID == i) {
                                alreadySelected = true;
                                vectorSpritesProperties.selectedEntities.RemoveAt(j);
                                break;
                            }
                        if (!alreadySelected)
                            vectorSpritesProperties.selectedEntities.Add(new VectorSprites.SelectedEntity(i));
                        endUndo(true);
                        GUIUtility.keyboardControl = 0;
                    }
                    bool isSelected = false;
                    if (vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.Sprite)
                        for (int j = 0; j < vectorSpritesProperties.selectedEntities.Count; j++)
                            if (vectorSpritesProperties.selectedEntities[j].primaryID == i) {
                                isSelected = true;
                                break;
                            }
                    EditorGUI.LabelField(labelRectangle, vectorSpritesProperties.vectorSprites[i].name, isSelected ? treeViewNodeSelected : treeViewNode);
                    top += 20;
                }
        }
        GUI.EndScrollView(true);

        //Display the settings section, including grid size and an export button.
        top = sectionHeader("Settings", settingsHeight, true);
        EditorGUI.BeginChangeCheck();
        beginProperty(getGUIPercentageRectangle(true, 2, 17.5f, top), "gridWidth",
                new GUIContent("Grid X", "Configure the number of horizontal grid points."));
        int temporaryGridWidth = Math.Max(Math.Min(EditorGUI.IntField(getGUIPercentageRectangle(true, 20, 28, top), vectorSpritesProperties.gridWidth), 128),
                2);
        endProperty();
        beginProperty(getGUIPercentageRectangle(true, 52, 17.5f, top), "gridHeight",
                new GUIContent("Grid Y", "Configure the number of vertical grid points."));
        int temporaryGridHeight = Math.Max(Math.Min(EditorGUI.IntField(getGUIPercentageRectangle(true, 70, 28, top), vectorSpritesProperties.gridHeight), 128),
                2);
        endProperty();
        top += rowPixels + rowGapPixels;
        beginProperty(getGUIPercentageRectangle(true, 2, 15, top), "showGrid", new GUIContent("Show", "Whether to show the grid."));
        bool temporaryShowGrid = EditorGUI.Toggle(getGUIPercentageRectangle(true, 18, 5, top), vectorSpritesProperties.showGrid);
        endProperty();
        beginProperty(getGUIPercentageRectangle(true, 24.5f, 14, top), "snapToGrid", new GUIContent("Snap", "Whether to snap to the grid points."));
        bool temporarySnapToGrid = EditorGUI.Toggle(getGUIPercentageRectangle(true, 38.5f, 5, top), vectorSpritesProperties.snapToGrid);
        endProperty();
        beginProperty(getGUIPercentageRectangle(true, 46, 19, top), "gridOnTop", new GUIContent("On Top",
                "Whether the grid should be shown on top of everything."));
        bool temporaryGridOnTop = EditorGUI.Toggle(getGUIPercentageRectangle(true, 65, 5, top), vectorSpritesProperties.gridOnTop);
        endProperty();
        beginProperty(getGUIPercentageRectangle(true, 72.5f, 19, top), "showGuidelines", new GUIContent("Guides",
                "Whether to show the sprite boundary guidelines."));
        bool temporaryShowGuidelines = EditorGUI.Toggle(getGUIPercentageRectangle(true, 91.5f, 5, top), vectorSpritesProperties.showGuidelines);
        endProperty();
        if (EditorGUI.EndChangeCheck()) {
            beginUndo("Change Grid Properties");
            vectorSpritesProperties.gridWidth = temporaryGridWidth;
            vectorSpritesProperties.gridHeight = temporaryGridHeight;
            vectorSpritesProperties.showGrid = temporaryShowGrid;
            vectorSpritesProperties.snapToGrid = temporarySnapToGrid;
            vectorSpritesProperties.gridOnTop = temporaryGridOnTop;
            vectorSpritesProperties.showGuidelines = temporaryShowGuidelines;
            endUndo(false);
        }
        top += rowPixels + rowGapPixels;
        EditorGUI.BeginChangeCheck();
        beginProperty(getGUIPercentageRectangle(true, 2, 32.5f, top), "editorQuality", new GUIContent("Editor Quality", "The quality of the shapes in the " +
                "editor window. Although shapes will not look as smooth at lower quality, they will take less time to generate, so the editor will run " +
                "quicker. Reduce the quality if you are experiencing slowdown."));
        VectorSprites.Quality temporaryQuality = (VectorSprites.Quality) EditorGUI.IntPopup(getGUIPercentageRectangle(true, 35, 62, top),
                    (int) vectorSpritesProperties.editorQuality, enumeratedTypeNames[(int) EnumeratedTypeNameArrays.Quality],
                    enumeratedTypeValues[(int) EnumeratedTypeNameArrays.Quality], enumPopupStyle);
        endProperty();
        if (EditorGUI.EndChangeCheck()) {
            beginUndo("Change Editor Quality");
            vectorSpritesProperties.editorQuality = temporaryQuality;
            for (int i = 0; i < vectorSpritesProperties.shapeGroups.Count; i++)
                for (int j = 0; j < vectorSpritesProperties.shapeGroups[i].shapes.Count; j++)
                    vectorSpritesProperties.shapeGroups[i].shapes[j].resetAllMeshes();
            endUndo(true);
        }
        top += rowPixels + rowGapPixels;
        EditorGUI.BeginChangeCheck();
        beginProperty(getGUIPercentageRectangle(true, 2, 32.5f, top), "gameQuality", new GUIContent("Game Quality", "The quality of the shapes when they are " +
                "used in a game or exported. It is recommended to leave this at \"Best\" quality because although game sprites will take a little longer to " +
                "generate, the additional delay will only be in the order of milliseconds, and will be a one-off when the scene is started."));
        temporaryQuality = (VectorSprites.Quality) EditorGUI.IntPopup(getGUIPercentageRectangle(true, 35, 62, top),
                    (int) vectorSpritesProperties.gameQuality, enumeratedTypeNames[(int) EnumeratedTypeNameArrays.Quality],
                    enumeratedTypeValues[(int) EnumeratedTypeNameArrays.Quality], enumPopupStyle);
        endProperty();
        if (EditorGUI.EndChangeCheck()) {
            beginUndo("Change Game Quality");
            vectorSpritesProperties.gameQuality = temporaryQuality;
            endUndo(true);
        }
        top += rowPixels + rowGapPixels;
        Rect hyperlinkRectangle = getGUIPercentageRectangle(true, 2, 39, top, rowPixels * buttonHeightMultiplier);
        GUI.Label(hyperlinkRectangle, new GUIContent("Version " + VectorSprites.VectorSpritesProperties.currentVersion, "Click here to see version changes."),
                labelStyleHyperlink);
        Rect zoomRectangle = getGUIPercentageRectangle(true, 43, 34, top, rowPixels * buttonHeightMultiplier);
        GUI.Label(zoomRectangle, new GUIContent("Zoom: " + vectorSpritesProperties.zoom.ToString("0.00") + "x"), zoomAmountStyle);
        zoomRectangle = getGUIPercentageRectangle(true, 79, 19, top, rowPixels * buttonHeightMultiplier);
        if (GUI.Button(zoomRectangle, new GUIContent("Reset", "Reset the zoom level to 1x"))) {
            vectorSpritesProperties.zoom = 1;
            vectorSpritesProperties.zoomCentre = new Vector2(0.5f, 0.5f);
        }
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && hyperlinkRectangle.Contains(Event.current.mousePosition)) {
            VectorSpritesVersionChanges vectorSpritesVersionChanges = GetWindow<VectorSpritesVersionChanges>();
            vectorSpritesVersionChanges.minSize = new Vector2(512, 512);
            vectorSpritesVersionChanges.title = "Vector Sprites - Version Changes";                    
        }

        //Draw the details of the selected item, if there is one, on the right.
        sectionHeaderTop = 0;

        //If nothing is selected...
        if (vectorSpritesProperties.selectedEntities.Count == 0)
            EditorGUI.LabelField(getGUIPercentageRectangle(false, 5, 90, borderPixels, position.height - (borderPixels * 2)),
                    "Select a shape group, shape or sprite on the left to modify its properties.", labelStyleInstructions);

        //If one or more sprite sheets is selected...
        else if (vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.SpriteSheet) {

            //Determine whether a single sprite sheet is selected.
            bool singleSpriteSheet = vectorSpritesProperties.selectedEntities.Count == 1;

            //Display the sprite sheet properties header.
            if (singleSpriteSheet)
                top = sectionHeader("Sprite Sheet Properties", (int) ((rowPixels * 2) + rowGapPixels + borderPixels), false);

            //If only a single sprite sheet is selected, allow the sprite name to be edited.
            if (singleSpriteSheet) {
                EditorGUI.BeginChangeCheck();
                beginProperty(getGUIPercentageRectangle(false, 2, 30, top), "spriteSheets.Array.data[" +
                        vectorSpritesProperties.selectedEntities[0].primaryID + "].name", new GUIContent("Name", "The name of the sprite sheet."));
                string temporarySpriteSheetName = GUI.TextField(getGUIPercentageRectangle(false, 32.5f, 64.5f, top),
                        vectorSpritesProperties.spriteSheets[vectorSpritesProperties.selectedEntities[0].primaryID].name.Replace("\n", "").Replace("\r", ""),
                        100);
                endProperty();
                if (temporarySpriteSheetName.Trim() == "")
                    temporarySpriteSheetName = getNextFreeSpriteSheetName();
                if (EditorGUI.EndChangeCheck()) {
                    beginUndo("Change Sprite Sheet Name");
                    vectorSpritesProperties.spriteSheets[vectorSpritesProperties.selectedEntities[0].primaryID].name = temporarySpriteSheetName;
                    endUndo(false);
                }
                top += rowPixels + rowGapPixels;
            }

            //Display the sprite sheet actions section.
            top = sectionHeader("Sprite Sheet Actions" + (singleSpriteSheet ? "" : " (" + vectorSpritesProperties.selectedEntities.Count + " sprite sheets)"),
                    (int) (rowPixels + (rowPixels * buttonHeightMultiplier * (singleSpriteSheet ? 4 : 1)) + (rowGapPixels * (singleSpriteSheet ? 4 : 1)) +
                    borderPixels), false);

            //Move the sprite sheet up/down (single sprite sheets only).
            if (singleSpriteSheet) {
                EditorGUI.BeginDisabledGroup(vectorSpritesProperties.selectedEntities[0].primaryID == 0);
                if (GUI.Button(getGUIPercentageRectangle(false, 10, 80, top, rowPixels * buttonHeightMultiplier), new GUIContent("Move Sprite Sheet Up",
                        "Swap this sprite sheet with the one above it in the sprites hierarchy."))) {
                    beginUndo("Move Sprite Sheet Up");
                    VectorSprites.SpriteSheet swapSpriteSheet = vectorSpritesProperties.spriteSheets[vectorSpritesProperties.selectedEntities[0].primaryID - 1];
                    vectorSpritesProperties.spriteSheets[vectorSpritesProperties.selectedEntities[0].primaryID - 1] =
                            vectorSpritesProperties.spriteSheets[vectorSpritesProperties.selectedEntities[0].primaryID];
                    vectorSpritesProperties.spriteSheets[vectorSpritesProperties.selectedEntities[0].primaryID] = swapSpriteSheet;
                    for (int i = 0; i < vectorSpritesProperties.vectorSprites.Count; i++)
                        if (vectorSpritesProperties.vectorSprites[i].spriteSheet == vectorSpritesProperties.selectedEntities[0].primaryID - 1)
                            vectorSpritesProperties.vectorSprites[i].spriteSheet = vectorSpritesProperties.selectedEntities[0].primaryID;
                        else if (vectorSpritesProperties.vectorSprites[i].spriteSheet == vectorSpritesProperties.selectedEntities[0].primaryID)
                            vectorSpritesProperties.vectorSprites[i].spriteSheet = vectorSpritesProperties.selectedEntities[0].primaryID - 1;
                    vectorSpritesProperties.selectedEntities[0].primaryID--;
                    endUndo(true);
                }
                EditorGUI.EndDisabledGroup();
                top += (rowPixels * buttonHeightMultiplier) + rowGapPixels;
                EditorGUI.BeginDisabledGroup(vectorSpritesProperties.selectedEntities[0].primaryID == vectorSpritesProperties.spriteSheets.Count - 1);
                if (GUI.Button(getGUIPercentageRectangle(false, 10, 80, top, rowPixels * buttonHeightMultiplier), new GUIContent("Move Sprite Sheet Down",
                        "Swap this sprite sheet with the one below it in the sprites hierarchy."))) {
                    beginUndo("Move Sprite Sheet Down");
                    VectorSprites.SpriteSheet swapSpriteSheet = vectorSpritesProperties.spriteSheets[vectorSpritesProperties.selectedEntities[0].primaryID + 1];
                    vectorSpritesProperties.spriteSheets[vectorSpritesProperties.selectedEntities[0].primaryID + 1] =
                            vectorSpritesProperties.spriteSheets[vectorSpritesProperties.selectedEntities[0].primaryID];
                    vectorSpritesProperties.spriteSheets[vectorSpritesProperties.selectedEntities[0].primaryID] = swapSpriteSheet;
                    for (int i = 0; i < vectorSpritesProperties.vectorSprites.Count; i++)
                        if (vectorSpritesProperties.vectorSprites[i].spriteSheet == vectorSpritesProperties.selectedEntities[0].primaryID + 1)
                            vectorSpritesProperties.vectorSprites[i].spriteSheet = vectorSpritesProperties.selectedEntities[0].primaryID;
                        else if (vectorSpritesProperties.vectorSprites[i].spriteSheet == vectorSpritesProperties.selectedEntities[0].primaryID)
                            vectorSpritesProperties.vectorSprites[i].spriteSheet = vectorSpritesProperties.selectedEntities[0].primaryID + 1;
                    vectorSpritesProperties.selectedEntities[0].primaryID++;
                    endUndo(true);
                }
                EditorGUI.EndDisabledGroup();
                top += (rowPixels * buttonHeightMultiplier) + rowGapPixels;
            }

            //Delete the sprite sheet(s) (multiple sprite sheets supported).
            if (GUI.Button(getGUIPercentageRectangle(false, 10, 80, top, rowPixels * buttonHeightMultiplier), new GUIContent("Delete Sprite Sheet" +
                    (singleSpriteSheet ? "" : "s"), "Delete the sprite sheet" + (singleSpriteSheet ? "" : "s") + ".")) &&
                    EditorUtility.DisplayDialog("Delete Sprite Sheet" + (singleSpriteSheet ? "" : "s"), "Are you sure you want to delete the sprite sheet" +
                    (singleSpriteSheet ? "" : "s") + "?", "Yes", "No")) {
                beginUndo("Delete Sprite Sheet" + (singleSpriteSheet ? "" : "s"));
                while (vectorSpritesProperties.selectedEntities.Count > 0) {
                    for (int i = 1; i < vectorSpritesProperties.selectedEntities.Count; i++)
                        if (vectorSpritesProperties.selectedEntities[i].primaryID > vectorSpritesProperties.selectedEntities[0].primaryID)
                            vectorSpritesProperties.selectedEntities[i].primaryID--;
                    for (int i = 0; i < vectorSpritesProperties.vectorSprites.Count; i++)
                        if (vectorSpritesProperties.vectorSprites[i].spriteSheet > vectorSpritesProperties.selectedEntities[0].primaryID)
                            vectorSpritesProperties.vectorSprites[i].spriteSheet--;
                        else if (vectorSpritesProperties.vectorSprites[i].spriteSheet == vectorSpritesProperties.selectedEntities[0].primaryID)
                            vectorSpritesProperties.vectorSprites[i].spriteSheet = -1;
                    vectorSpritesProperties.spriteSheets.RemoveAt(vectorSpritesProperties.selectedEntities[0].primaryID);
                    vectorSpritesProperties.selectedEntities.RemoveAt(0);
                }
                endUndo(true);
            }
            else {
                top += (rowPixels * buttonHeightMultiplier) + rowGapPixels;

                //Preview/export the sprite sheet (single sprite sheets only).
                bool spriteSheetHasOneSprite = false;
                for (int i = 0; i < vectorSpritesProperties.vectorSprites.Count; i++)
                    if (vectorSpritesProperties.vectorSprites[i].spriteSheet == vectorSpritesProperties.selectedEntities[0].primaryID) {
                        spriteSheetHasOneSprite = true;
                        break;
                    }
                EditorGUI.BeginDisabledGroup(!spriteSheetHasOneSprite);
                if (singleSpriteSheet && GUI.Button(getGUIPercentageRectangle(false, 10, 80, top, rowPixels * buttonHeightMultiplier),
                        new GUIContent("Export Sprite Sheet", "Display a preview of the sprite sheet with the option to export it as an asset.")))
                    exportSprite();
                EditorGUI.EndDisabledGroup();
            }
        }

        //If one or more sprites is selected...
        else if (vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.Sprite) {

            //Determine whether a single sprite is selected.
            bool singleSprite = vectorSpritesProperties.selectedEntities.Count == 1;

            //Display the sprite properties header.
            top = sectionHeader("Sprite Properties" + (singleSprite ? "" : " (" + vectorSpritesProperties.selectedEntities.Count + " sprites)"),
                    (int) ((rowPixels * (singleSprite ? 6 : 4)) + (rowGapPixels * (singleSprite ? 5 : 3)) + borderPixels), false);

            //If only a single sprite is selected, allow the sprite name to be edited.
            if (singleSprite) {
                EditorGUI.BeginChangeCheck();
                beginProperty(getGUIPercentageRectangle(false, 2, 30, top), "vectorSprites.Array.data[" +
                        vectorSpritesProperties.selectedEntities[0].primaryID + "].name", new GUIContent("Name", "The name of the sprite."));
                string temporarySpriteName = GUI.TextField(getGUIPercentageRectangle(false, 32.5f, 64.5f, top),
                        vectorSpritesProperties.vectorSprites[vectorSpritesProperties.selectedEntities[0].primaryID].name.Replace("\n", "").Replace("\r", ""),
                        100);
                endProperty();
                if (temporarySpriteName.Trim() == "")
                    temporarySpriteName = getNextFreeSpriteName();
                if (EditorGUI.EndChangeCheck()) {
                    beginUndo("Change Sprite Name");
                    vectorSpritesProperties.vectorSprites[vectorSpritesProperties.selectedEntities[0].primaryID].name = temporarySpriteName;
                    endUndo(false);
                }
                top += rowPixels + rowGapPixels;
            }

            //Allow the sprite size to be edited (including for multiple sprites).
            EditorGUI.BeginChangeCheck();
            if (singleSprite)
                beginProperty(getGUIPercentageRectangle(false, 2, 30, top), "vectorSprites.Array.data[" +
                        vectorSpritesProperties.selectedEntities[0].primaryID + "].width",
                        new GUIContent("Size X", "The horizontal size of the sprite, in pixels."));
            else
                GUI.Label(getGUIPercentageRectangle(false, 2, 30, top), new GUIContent("Size X", "The horizontal size of the sprites, in pixels."));
            int temporarySpriteWidth = -1;
            for (int i = 0; i < vectorSpritesProperties.selectedEntities.Count; i++)
                if (temporarySpriteWidth == -1 || temporarySpriteWidth ==
                        vectorSpritesProperties.vectorSprites[vectorSpritesProperties.selectedEntities[i].primaryID].width)
                    temporarySpriteWidth = vectorSpritesProperties.vectorSprites[vectorSpritesProperties.selectedEntities[i].primaryID].width;
                else {
                    EditorGUI.showMixedValue = true;
                    break;
                }
            temporarySpriteWidth = Math.Min(Math.Max(EditorGUI.IntField(getGUIPercentageRectangle(false, 32.5f, 19.5f, top),
                    vectorSpritesProperties.vectorSprites[vectorSpritesProperties.selectedEntities[0].primaryID].width), 1), 2048);
            EditorGUI.showMixedValue = false;
            if (singleSprite)
                endProperty();
            if (singleSprite)
                beginProperty(getGUIPercentageRectangle(false, 58, 19, top), "vectorSprites.Array.data[" +
                        vectorSpritesProperties.selectedEntities[0].primaryID + "].height", new GUIContent("Size Y",
                        "The vertical size of the sprite, in pixels."));
            else
                GUI.Label(getGUIPercentageRectangle(false, 58, 19, top), new GUIContent("Size Y", "The vertical size of the sprites, in pixels."));
            int temporarySpriteHeight = -1;
            for (int i = 0; i < vectorSpritesProperties.selectedEntities.Count; i++)
                if (temporarySpriteHeight == -1 || temporarySpriteHeight ==
                        vectorSpritesProperties.vectorSprites[vectorSpritesProperties.selectedEntities[i].primaryID].height)
                    temporarySpriteHeight = vectorSpritesProperties.vectorSprites[vectorSpritesProperties.selectedEntities[i].primaryID].height;
                else {
                    EditorGUI.showMixedValue = true;
                    break;
                }
            temporarySpriteHeight = Math.Min(Math.Max(EditorGUI.IntField(getGUIPercentageRectangle(false, 77.5f, 19.5f, top),
                    vectorSpritesProperties.vectorSprites[vectorSpritesProperties.selectedEntities[0].primaryID].height), 1), 2048);
            EditorGUI.showMixedValue = false;
            if (singleSprite)
                endProperty();
            if (EditorGUI.EndChangeCheck()) {
                beginUndo("Change Sprite Size");
                for (int i = 0; i < vectorSpritesProperties.selectedEntities.Count; i++) {
                    vectorSpritesProperties.vectorSprites[vectorSpritesProperties.selectedEntities[i].primaryID].width = temporarySpriteWidth;
                    vectorSpritesProperties.vectorSprites[vectorSpritesProperties.selectedEntities[i].primaryID].height = temporarySpriteHeight;
                }
                endUndo(false);
            }
            top += rowPixels + rowGapPixels;

            //Select the non-square sprite transform mode (single sprites only).
            if (singleSprite) {
                EditorGUI.BeginDisabledGroup(vectorSpritesProperties.vectorSprites[vectorSpritesProperties.selectedEntities[0].primaryID].width ==
                        vectorSpritesProperties.vectorSprites[vectorSpritesProperties.selectedEntities[0].primaryID].height);
                EditorGUI.BeginChangeCheck();
                beginProperty(getGUIPercentageRectangle(false, 2, 30, top), "vectorSprites.Array.data[" +
                        vectorSpritesProperties.selectedEntities[0].primaryID + "].spriteRectangleTransform", new GUIContent("Transform",
                        "How non-square images should be exported - either cropped or scaled."));
                VectorSprites.SpriteRectangleTransform temporarySpriteRectangleTransform = (VectorSprites.SpriteRectangleTransform)
                        EditorGUI.IntPopup(getGUIPercentageRectangle(false, 32.5f, 64.5f, top),
                        (int) vectorSpritesProperties.vectorSprites[vectorSpritesProperties.selectedEntities[0].primaryID].spriteRectangleTransform,
                        enumeratedTypeNames[(int) EnumeratedTypeNameArrays.SpriteRectangleTransform],
                        enumeratedTypeValues[(int) EnumeratedTypeNameArrays.SpriteRectangleTransform], enumPopupStyle);
                endProperty();
                if (EditorGUI.EndChangeCheck()) {
                    beginUndo("Change Sprite Transform");
                    vectorSpritesProperties.vectorSprites[vectorSpritesProperties.selectedEntities[0].primaryID].spriteRectangleTransform =
                            temporarySpriteRectangleTransform;
                    endUndo(true);
                }
                EditorGUI.EndDisabledGroup();
                top += rowPixels + rowGapPixels;
            }

            //Allow the anti-alias flag to be set (multiple sprites supported).
            EditorGUI.BeginChangeCheck();
            if (singleSprite)
                beginProperty(getGUIPercentageRectangle(false, 2, 30, top), "vectorSprites.Array.data[" +
                        vectorSpritesProperties.selectedEntities[0].primaryID + "].antialias", new GUIContent("Anti-alias",
                        "If checked, renders the sprite at double size then scales it down to smooth out angled edges, making it look less pixelated. Does " +
                        "not affect sprites in the editor window - only when they are exported."));
            else
                GUI.Label(getGUIPercentageRectangle(false, 2, 30, top), new GUIContent("Anti-alias", "If checked, renders the sprites at double size then " +
                        "scales them down to smooth out angled edges, making them look less pixelated. Does not affect sprites in the editor window - only " +
                        "when they are exported."));
            bool temporarySpriteAntialias = false;
            for (int i = 0; i < vectorSpritesProperties.selectedEntities.Count; i++)
                if (i == 0 || temporarySpriteAntialias == vectorSpritesProperties.vectorSprites[vectorSpritesProperties.selectedEntities[i].primaryID].
                        antialias)
                    temporarySpriteAntialias = vectorSpritesProperties.vectorSprites[vectorSpritesProperties.selectedEntities[i].primaryID].antialias;
                else if (i > 0) {
                    EditorGUI.showMixedValue = true;
                    break;
                }
            temporarySpriteAntialias = EditorGUI.Toggle(getGUIPercentageRectangle(false, 32.5f, 64.5f, top),
                    vectorSpritesProperties.vectorSprites[vectorSpritesProperties.selectedEntities[0].primaryID].antialias);
            if (singleSprite)
                endProperty();
            if (EditorGUI.EndChangeCheck()) {
                beginUndo("Toggle Sprite Anti-alias");
                for (int i = 0; i < vectorSpritesProperties.selectedEntities.Count; i++)
                    vectorSpritesProperties.vectorSprites[vectorSpritesProperties.selectedEntities[i].primaryID].antialias = temporarySpriteAntialias;
                endUndo(true);
            }
            top += rowPixels + rowGapPixels;

            //Allow the sprite sheet to be set (multiple sprites supported).
            EditorGUI.BeginChangeCheck();
            if (singleSprite)
                beginProperty(getGUIPercentageRectangle(false, 2, 30, top), "vectorSprites.Array.data[" +
                        vectorSpritesProperties.selectedEntities[0].primaryID + "].spriteSheet", new GUIContent("Sprite Sheet",
                        "The sprite sheet that the sprite is exported onto, if any."));
            else
                GUI.Label(getGUIPercentageRectangle(false, 2, 30, top), new GUIContent("Sprite Sheet",
                        "The sprite sheet that the sprites are exported onto, if any."));
            int temporarySpriteSheet = -1;
            for (int i = 0; i < vectorSpritesProperties.selectedEntities.Count; i++)
                if (i == 0 || temporarySpriteSheet == vectorSpritesProperties.vectorSprites[vectorSpritesProperties.selectedEntities[i].primaryID].
                        spriteSheet)
                    temporarySpriteSheet = vectorSpritesProperties.vectorSprites[vectorSpritesProperties.selectedEntities[i].primaryID].spriteSheet;
                else if (i > 0) {
                    EditorGUI.showMixedValue = true;
                    break;
                }
            int[] spriteSheetIDs = new int[vectorSpritesProperties.spriteSheets.Count + 1];
            spriteSheetIDs[0] = -1;
            string[] spriteSheetNames = new string[vectorSpritesProperties.spriteSheets.Count + 1];
            spriteSheetNames[0] = "--- None ---";
            for (int i = 0; i < vectorSpritesProperties.spriteSheets.Count; i++) {
                spriteSheetIDs[i + 1] = i;
                spriteSheetNames[i + 1] = vectorSpritesProperties.spriteSheets[i].name;
            }
            temporarySpriteSheet = EditorGUI.IntPopup(getGUIPercentageRectangle(false, 32.5f, 64.5f, top),
                    vectorSpritesProperties.vectorSprites[vectorSpritesProperties.selectedEntities[0].primaryID].spriteSheet, spriteSheetNames, spriteSheetIDs);
            if (singleSprite)
                endProperty();
            if (EditorGUI.EndChangeCheck()) {
                beginUndo("Change Sprite's Sprite Sheet");
                for (int i = 0; i < vectorSpritesProperties.selectedEntities.Count; i++)
                    vectorSpritesProperties.vectorSprites[vectorSpritesProperties.selectedEntities[i].primaryID].spriteSheet = temporarySpriteSheet;
                endUndo(true);
            }

            //Display the sprite actions section.
            top = sectionHeader("Sprite Actions" + (singleSprite ? "" : " (" + vectorSpritesProperties.selectedEntities.Count + " sprites)"),
                    (int) (rowPixels + (rowPixels * buttonHeightMultiplier * (singleSprite ? 5 : 1)) + (rowGapPixels * (singleSprite ? 5 : 1)) + borderPixels),
                    false);

            //Move the sprite up/down (single sprites only).
            if (singleSprite) {
                EditorGUI.BeginDisabledGroup(vectorSpritesProperties.selectedEntities[0].primaryID == 0);
                if (GUI.Button(getGUIPercentageRectangle(false, 10, 80, top, rowPixels * buttonHeightMultiplier), new GUIContent("Move Sprite Up",
                        "Swap this sprite with the one above it in the sprites hierarchy."))) {
                    beginUndo("Move Sprite Up");
                    VectorSprites.VectorSprite swapSprite = vectorSpritesProperties.vectorSprites[vectorSpritesProperties.selectedEntities[0].primaryID - 1];
                    vectorSpritesProperties.vectorSprites[vectorSpritesProperties.selectedEntities[0].primaryID - 1] =
                            vectorSpritesProperties.vectorSprites[vectorSpritesProperties.selectedEntities[0].primaryID];
                    vectorSpritesProperties.vectorSprites[vectorSpritesProperties.selectedEntities[0].primaryID] = swapSprite;
                    vectorSpritesProperties.selectedEntities[0].primaryID--;
                    endUndo(true);
                }
                EditorGUI.EndDisabledGroup();
                top += (rowPixels * buttonHeightMultiplier) + rowGapPixels;
                EditorGUI.BeginDisabledGroup(vectorSpritesProperties.selectedEntities[0].primaryID == vectorSpritesProperties.vectorSprites.Count - 1);
                if (GUI.Button(getGUIPercentageRectangle(false, 10, 80, top, rowPixels * buttonHeightMultiplier), new GUIContent("Move Sprite Down",
                        "Swap this sprite with the one below it in the sprites hierarchy."))) {
                    beginUndo("Move Sprite Down");
                    VectorSprites.VectorSprite swapSprite = vectorSpritesProperties.vectorSprites[vectorSpritesProperties.selectedEntities[0].primaryID + 1];
                    vectorSpritesProperties.vectorSprites[vectorSpritesProperties.selectedEntities[0].primaryID + 1] =
                            vectorSpritesProperties.vectorSprites[vectorSpritesProperties.selectedEntities[0].primaryID];
                    vectorSpritesProperties.vectorSprites[vectorSpritesProperties.selectedEntities[0].primaryID] = swapSprite;
                    vectorSpritesProperties.selectedEntities[0].primaryID++;
                    endUndo(true);
                }
                EditorGUI.EndDisabledGroup();
                top += (rowPixels * buttonHeightMultiplier) + rowGapPixels;
            }

            //Delete the sprite(s) (multiple sprites supported).
            if (GUI.Button(getGUIPercentageRectangle(false, 10, 80, top, rowPixels * buttonHeightMultiplier), new GUIContent("Delete Sprite" +
                    (singleSprite ? "" : "s"), "Delete the sprite" + (singleSprite ? "" : "s") + ".")) && EditorUtility.DisplayDialog("Delete Sprite" +
                    (singleSprite ? "" : "s"), "Are you sure you want to delete the sprite" + (singleSprite ? "" : "s") + "?", "Yes", "No")) {
                beginUndo("Delete Sprite" + (singleSprite ? "" : "s"));
                while (vectorSpritesProperties.selectedEntities.Count > 0) {
                    for (int i = 1; i < vectorSpritesProperties.selectedEntities.Count; i++)
                        if (vectorSpritesProperties.selectedEntities[i].primaryID > vectorSpritesProperties.selectedEntities[0].primaryID)
                            vectorSpritesProperties.selectedEntities[i].primaryID--;
                    vectorSpritesProperties.vectorSprites.RemoveAt(vectorSpritesProperties.selectedEntities[0].primaryID);
                    vectorSpritesProperties.selectedEntities.RemoveAt(0);
                }
                endUndo(true);
            }
            else {
                top += (rowPixels * buttonHeightMultiplier) + rowGapPixels;

                //Duplicate the sprite (single sprites only).
                if (singleSprite) {
                    if (GUI.Button(getGUIPercentageRectangle(false, 10, 80, top, rowPixels * buttonHeightMultiplier), new GUIContent("Duplicate Sprite",
                            "Creates a copy of the sprite and select the copy."))) {
                        beginUndo("Duplicate Sprite");
                        vectorSpritesProperties.selectedEntities[0].primaryID = vectorSpritesProperties.copySprite(
                                vectorSpritesProperties.selectedEntities[0].primaryID);
                        endUndo(true);
                    }
                    top += (rowPixels * buttonHeightMultiplier) + rowGapPixels;
                }

                //Preview/export the sprite (single sprites only).
                if (singleSprite && GUI.Button(getGUIPercentageRectangle(false, 10, 80, top, rowPixels * buttonHeightMultiplier),
                        new GUIContent("Export Sprite", "Display a preview of the sprite with the option to export it as an asset.")))
                    exportSprite();

                //Display the list of shapes associated with this sprite (single sprites only). NOTE: begin/end property code has not been included for this
                //section because there are two lists required to create the sprite associations (for shape groups and individual shapes). There doesn't appear
                //to be a way to use properties for multiple fields at once.
                if (singleSprite) {
                    VectorSprites.VectorSprite sprite = vectorSpritesProperties.vectorSprites[vectorSpritesProperties.selectedEntities[0].primaryID];
                    int totalShapesAndGroups = vectorSpritesProperties.shapeGroups.Count;
                    for (int i = 0; i < vectorSpritesProperties.shapeGroups.Count; i++)
                        totalShapesAndGroups += vectorSpritesProperties.shapeGroups[i].shapes.Count;
                    top += (rowPixels * buttonHeightMultiplier) + rowGapPixels;
                    top = sectionHeader("Sprite Shapes", (int) (position.height - top - (borderPixels * 2) - 4), false);
                    sprite.shapeGroupsScrollPosition = GUI.BeginScrollView(
                            new Rect(position.width - GUIWidth - borderPixels, top, GUIWidth - 2, position.height - top - borderPixels - 3),
                            sprite.shapeGroupsScrollPosition, new Rect(0, 0, GUIWidth - GUI.skin.verticalScrollbar.fixedWidth - 4,
                            Mathf.Max(totalShapesAndGroups * 20, position.height - top - borderPixels - 2)), false, true);
                    int spriteShapeTop = 0;
                    for (int i = 0; i < vectorSpritesProperties.shapeGroups.Count; i++) {
                        bool shapeGroupSelected = false;
                        for (int j = 0; j < sprite.shapeGroupIDs.Count; j++)
                            if (sprite.shapeGroupIDs[j] == i && sprite.shapeIDs[j] == -1) {
                                shapeGroupSelected = true;
                                break;
                            }
                        GUI.Label(new Rect(17, spriteShapeTop, GUIWidth, 20), new GUIContent(vectorSpritesProperties.shapeGroups[i].name));
                        bool newAllShapesInGroupSelected = EditorGUI.ToggleLeft(new Rect(3, spriteShapeTop, GUIWidth, 20), "", shapeGroupSelected);
                        spriteShapeTop += 20;
                        if (shapeGroupSelected && !newAllShapesInGroupSelected) {
                            beginUndo("Remove Shape Group From Sprite");
                            for (int j = sprite.shapeGroupIDs.Count - 1; j >= 0; j--)
                                if (sprite.shapeGroupIDs[j] == i) {
                                    sprite.shapeGroupIDs.RemoveAt(j);
                                    sprite.shapeIDs.RemoveAt(j);
                                }
                            for (int j = 0; j < vectorSpritesProperties.shapeGroups[i].shapes.Count; j++) {
                                sprite.shapeGroupIDs.Add(i);
                                sprite.shapeIDs.Add(j);
                            }
                            endUndo(true);
                        }
                        else if (!shapeGroupSelected && newAllShapesInGroupSelected) {
                            beginUndo("Add Shape Group To Sprite");
                            for (int j = sprite.shapeGroupIDs.Count - 1; j >= 0; j--)
                                if (sprite.shapeGroupIDs[j] == i) {
                                    sprite.shapeGroupIDs.RemoveAt(j);
                                    sprite.shapeIDs.RemoveAt(j);
                                }
                            sprite.shapeGroupIDs.Add(i);
                            sprite.shapeIDs.Add(-1);
                            endUndo(true);
                        }
                        for (int j = 0; j < vectorSpritesProperties.shapeGroups[i].shapes.Count; j++) {
                            bool shapeSelected = shapeGroupSelected;
                            if (!shapeSelected)
                                for (int k = 0; k < sprite.shapeGroupIDs.Count; k++)
                                    if (sprite.shapeGroupIDs[k] == i && sprite.shapeIDs[k] == j) {
                                        shapeSelected = true;
                                        break;
                                    }
                            EditorGUI.BeginDisabledGroup(shapeGroupSelected);
                            GUI.Label(new Rect(37, spriteShapeTop, GUIWidth - 20, 20), new GUIContent(vectorSpritesProperties.shapeGroups[i].shapes[j].name));
                            bool newShapeSelected = EditorGUI.ToggleLeft(new Rect(23, spriteShapeTop, GUIWidth - 20, 20), "", shapeSelected);
                            EditorGUI.EndDisabledGroup();
                            spriteShapeTop += 20;
                            if (shapeSelected && !newShapeSelected) {
                                beginUndo("Remove Shape From Sprite");
                                for (int k = sprite.shapeGroupIDs.Count - 1; k >= 0; k--)
                                    if (sprite.shapeGroupIDs[k] == i && sprite.shapeIDs[k] == j) {
                                        sprite.shapeGroupIDs.RemoveAt(k);
                                        sprite.shapeIDs.RemoveAt(k);
                                    }
                                endUndo(true);
                            }
                            else if (!shapeSelected && newShapeSelected) {
                                beginUndo("Add Shape To Sprite");
                                for (int k = sprite.shapeGroupIDs.Count - 1; k >= 0; k--)
                                    if (sprite.shapeGroupIDs[k] == i && sprite.shapeIDs[k] == j) {
                                        sprite.shapeGroupIDs.RemoveAt(k);
                                        sprite.shapeIDs.RemoveAt(k);
                                    }
                                sprite.shapeGroupIDs.Add(i);
                                sprite.shapeIDs.Add(j);
                                endUndo(true);
                            }
                        }
                    }
                    GUI.EndScrollView(true);
                }
            }
        }

        //If a new shape is being created...
        else if (vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.NewShape) {
            top = sectionHeader("New Shape", (int) (position.height - (borderPixels * 2)), false);
            EditorGUI.LabelField(getGUIPercentageRectangle(false, 2, 30, top), new GUIContent("Name", "The name of the new shape."));
            newShapeName = GUI.TextField(getGUIPercentageRectangle(false, 32.5f, 64.5f, top), newShapeName.Replace("\n", "").Replace("\r", ""), 100);
            if (newShapeName.Trim() == "")
                newShapeName = getNextFreeShapeName();
            top += rowPixels + rowGapPixels;
            EditorGUI.LabelField(getGUIPercentageRectangle(false, 5, 90, top, 150), "Click on the grid on the left to place the points for the new shape. " +
                    "Click \"Create Shape\" below (or right click on the grid) when you're done (the last point in the shape will be automatically joined to " +
                    "the first one). Shapes must have at least three and at most " + maxPointsPerShape.ToString() + " points.", labelStyleInstructions);
            top += 150;
            if (GUI.Button(getGUIPercentageRectangle(false, 5, 42.5f, top, rowPixels * buttonHeightMultiplier), new GUIContent("Create Shape",
                    "Create the shape from the selected points.")))
                createShape();
            if (GUI.Button(getGUIPercentageRectangle(false, 52.5f, 42.5f, top, rowPixels * buttonHeightMultiplier), new GUIContent("Cancel",
                    "Cancel the creation of this shape.")))
                vectorSpritesProperties.selectedEntity = VectorSprites.SelectableEntity.ShapeGroup;
            top += (rowPixels * buttonHeightMultiplier) + rowGapPixels;
            EditorGUI.LabelField(getGUIPercentageRectangle(false, 2, 30, top), new GUIContent("Show Group", "Show other shapes in this group while creating " +
                    "a new shape."));
            vectorSpritesProperties.showOtherShapesWhenCreatingNewShape = EditorGUI.Toggle(getGUIPercentageRectangle(false, 32.5f, 5, top),
                    vectorSpritesProperties.showOtherShapesWhenCreatingNewShape);
            top += rowPixels + rowGapPixels;
            newShapeRotation = EditorGUI.FloatField(getGUIPercentageRectangle(false, 2, 35, top), newShapeRotation);
            EditorGUI.LabelField(getGUIPercentageRectangle(false, 38, 22, top), "degrees");
            if (GUI.Button(getGUIPercentageRectangle(false, 60, 38, top, rowPixels * buttonHeightMultiplier), new GUIContent("Rotate",
                    "Rotate the points already added to the shape around the centre of the sprite. This can be repeated to create, for example circular " +
                    "shapes that have a repeating pattern around the outside."))) {
                float sineAngle = Mathf.Sin(newShapeRotation * Mathf.Deg2Rad);
                float cosineAngle = Mathf.Cos(newShapeRotation * Mathf.Deg2Rad);
                bool shapePointOutOfBounds = false;
                for (int i = 0; i < newShapePoints.Count; i++) {
                    Vector2 rotatedPoint = new Vector2(((newShapePoints[i].x - 0.5f) * cosineAngle) - ((newShapePoints[i].y - 0.5f) * sineAngle),
                            ((newShapePoints[i].x - 0.5f) * sineAngle) + ((newShapePoints[i].y - 0.5f) * cosineAngle)) + new Vector2(0.5f, 0.5f);
                    if (rotatedPoint.x <= 0 || rotatedPoint.x >= 1 || rotatedPoint.y <= 0 || rotatedPoint.y >= 1) {
                        EditorUtility.DisplayDialog("Out of Bounds", "The shape cannot be rotated because the rotation angle you have chosen will move at " +
                                "least one shape point of the bounds of the sprite window.", "OK");
                        shapePointOutOfBounds = true;
                        break;
                    }
                }
                if (!shapePointOutOfBounds)
                    for (int i = 0; i < newShapePoints.Count; i++)
                        newShapePoints[i] = new Vector2(((newShapePoints[i].x - 0.5f) * cosineAngle) - ((newShapePoints[i].y - 0.5f) * sineAngle),
                                ((newShapePoints[i].x - 0.5f) * sineAngle) + ((newShapePoints[i].y - 0.5f) * cosineAngle)) + new Vector2(0.5f, 0.5f);
            }
        }

        //If a shape group is selected...
        else if (vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.ShapeGroup) {

            //Determine whether a single shape group is selected.
            bool singleShapeGroup = vectorSpritesProperties.selectedEntities.Count == 1;

            //Create the section header.
            if (singleShapeGroup)
                top = sectionHeader("Shape Group Properties", (int) ((rowPixels * 2) + rowGapPixels + borderPixels), false);

            //Allow the shape group name to be edited (single shape groups only).
            if (singleShapeGroup) {
                EditorGUI.BeginChangeCheck();
                beginProperty(getGUIPercentageRectangle(false, 2, 30, top), "shapeGroups.Array.data[" + vectorSpritesProperties.selectedEntities[0].primaryID +
                        "].name", new GUIContent("Name", "The name of the shape group."));
                string temporaryShapeGroupName = GUI.TextField(getGUIPercentageRectangle(false, 32.5f, 64.5f, top),
                        vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[0].primaryID].name.Replace("\n", "").Replace("\r", ""),
                        100);
                endProperty();
                if (vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[0].primaryID].name.Trim() == "")
                    vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[0].primaryID].name = getNextFreeShapeGroupName();
                if (EditorGUI.EndChangeCheck()) {
                    beginUndo("Change Shape Group Name");
                    vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[0].primaryID].name = temporaryShapeGroupName;
                    endUndo(false);
                }
                top += rowPixels;
            }

            //Display the shape group actions section.
            top = sectionHeader("Shape Group Actions" + (singleShapeGroup ? "" : " (" + vectorSpritesProperties.selectedEntities.Count + " groups)"),
                    (int) (rowPixels + (rowPixels * buttonHeightMultiplier * (singleShapeGroup ? 4 : 2)) + (rowGapPixels * (singleShapeGroup ? 4 : 2)) +
                    borderPixels), false);

            //Move the shape group up/down (single shape groups only).
            if (singleShapeGroup) {
                EditorGUI.BeginDisabledGroup(vectorSpritesProperties.selectedEntities[0].primaryID == 0);
                if (GUI.Button(getGUIPercentageRectangle(false, 10, 80, top, rowPixels * buttonHeightMultiplier), new GUIContent("Move Shape Group Up",
                        "Swap this shape group with the one above it in the shapes hierarchy."))) {
                    beginUndo("Move Shape Group Up");
                    VectorSprites.ShapeGroup swapShapeGroup = vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[0].primaryID - 1];
                    vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[0].primaryID - 1] =
                            vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[0].primaryID];
                    vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[0].primaryID] = swapShapeGroup;
                    for (int i = 0; i < vectorSpritesProperties.vectorSprites.Count; i++)
                        for (int j = 0; j < vectorSpritesProperties.vectorSprites[i].shapeGroupIDs.Count; j++)
                            if (vectorSpritesProperties.vectorSprites[i].shapeGroupIDs[j] == vectorSpritesProperties.selectedEntities[0].primaryID)
                                vectorSpritesProperties.vectorSprites[i].shapeGroupIDs[j]--;
                            else if (vectorSpritesProperties.vectorSprites[i].shapeGroupIDs[j] == vectorSpritesProperties.selectedEntities[0].primaryID - 1)
                                vectorSpritesProperties.vectorSprites[i].shapeGroupIDs[j]++;
                    vectorSpritesProperties.selectedEntities[0].primaryID--;
                    endUndo(true);
                }
                EditorGUI.EndDisabledGroup();
                top += (rowPixels * buttonHeightMultiplier) + rowGapPixels;
                EditorGUI.BeginDisabledGroup(vectorSpritesProperties.selectedEntities[0].primaryID == vectorSpritesProperties.shapeGroups.Count - 1);
                if (GUI.Button(getGUIPercentageRectangle(false, 10, 80, top, rowPixels * buttonHeightMultiplier), new GUIContent("Move Shape Group Down",
                        "Swap this shape group with the one below it in the shapes hierarchy."))) {
                    beginUndo("Move Shape Group Down");
                    VectorSprites.ShapeGroup swapShapeGroup = vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[0].primaryID + 1];
                    vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[0].primaryID + 1] =
                            vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[0].primaryID];
                    vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[0].primaryID] = swapShapeGroup;
                    for (int i = 0; i < vectorSpritesProperties.vectorSprites.Count; i++)
                        for (int j = 0; j < vectorSpritesProperties.vectorSprites[i].shapeGroupIDs.Count; j++)
                            if (vectorSpritesProperties.vectorSprites[i].shapeGroupIDs[j] == vectorSpritesProperties.selectedEntities[0].primaryID)
                                vectorSpritesProperties.vectorSprites[i].shapeGroupIDs[j]++;
                            else if (vectorSpritesProperties.vectorSprites[i].shapeGroupIDs[j] == vectorSpritesProperties.selectedEntities[0].primaryID + 1)
                                vectorSpritesProperties.vectorSprites[i].shapeGroupIDs[j]--;
                    vectorSpritesProperties.selectedEntities[0].primaryID++;
                    endUndo(true);
                }
                EditorGUI.EndDisabledGroup();
                top += (rowPixels * buttonHeightMultiplier) + rowGapPixels;
            }

            //Transform shapes (multiple shape groups allowed).
            bool atLeastOneShapeInSelectedGroups = false;
            for (int i = 0; i < vectorSpritesProperties.selectedEntities.Count; i++)
                if (vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[i].primaryID].shapes.Count > 0) {
                    atLeastOneShapeInSelectedGroups = true;
                    break;
                }
            EditorGUI.BeginDisabledGroup(!atLeastOneShapeInSelectedGroups);
            if (GUI.Button(getGUIPercentageRectangle(false, 10, 80, top, rowPixels * buttonHeightMultiplier), new GUIContent("Transform All Shapes",
                    "Position, rotate or scale the all shapes within " + (singleShapeGroup ? "this group" : "these groups") + "."))) {
                VectorSpritesTransformShape vectorSpritesTransformShape = GetWindow<VectorSpritesTransformShape>();
                vectorSpritesTransformShape.minSize = new Vector2(256, 128);
                vectorSpritesTransformShape.title = "Vector Sprites - Transform Shape Group" + (singleShapeGroup ? "" : "s");
                vectorSpritesTransformShape.initialise(this, vectorSpritesProperties);
            }
            EditorGUI.EndDisabledGroup();
            top += (rowPixels * buttonHeightMultiplier) + rowGapPixels;

            //Delete the shape group (multiple shape groups allowed).
            if (GUI.Button(getGUIPercentageRectangle(false, 10, 80, top, rowPixels * buttonHeightMultiplier), new GUIContent("Delete Shape Group" +
                    (singleShapeGroup ? "" : "s"), "Delete the shape group" + (singleShapeGroup ? "" : "s") + ", and any shapes associated with " +
                    (singleShapeGroup ? "it" : "them") + "."))) {
                bool acceptedWarning = true;
                if (singleShapeGroup && vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[0].primaryID].shapes.Count > 0)
                    acceptedWarning = EditorUtility.DisplayDialog("Delete Shape Group", "This shape group has " +
                            vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[0].primaryID].shapes.Count + " shape" +
                            (vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[0].primaryID].shapes.Count == 1 ? "" : "s") +
                            " associated with it. Deleting the shape group will delete these shapes as well. Are you sure you want to delete the shape group?",
                            "Yes", "No");
                else if (!singleShapeGroup) {
                    bool oneShapeGroupHasShapesInIt = false;
                    for (int i = 0; i < vectorSpritesProperties.selectedEntities.Count; i++)
                        if (vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[i].primaryID].shapes.Count > 0) {
                            oneShapeGroupHasShapesInIt = true;
                            break;
                        }
                    if (oneShapeGroupHasShapesInIt)
                        acceptedWarning = EditorUtility.DisplayDialog("Delete Shape Groups", "At least one of the selected shape groups contains shapes. " +
                            "Deleting these shape groups will delete all of their associated shapes as well. Are you sure you want to delete these shape " +
                            "groups?", "Yes", "No");
                }
                if (acceptedWarning) {
                    beginUndo("Delete Shape Group" + (singleShapeGroup ? "" : "s"));
                    for (int k = vectorSpritesProperties.selectedEntities.Count - 1; k >= 0; k--) {
                        for (int i = 0; i < vectorSpritesProperties.vectorSprites.Count; i++) {
                            for (int j = 0; j < vectorSpritesProperties.vectorSprites[i].shapeGroupIDs.Count; j++)
                                if (vectorSpritesProperties.vectorSprites[i].shapeGroupIDs[j] == vectorSpritesProperties.selectedEntities[k].primaryID)
                                    vectorSpritesProperties.vectorSprites[i].shapeGroupIDs[j] = -1;
                                else if (vectorSpritesProperties.vectorSprites[i].shapeGroupIDs[j] > vectorSpritesProperties.selectedEntities[k].primaryID)
                                    vectorSpritesProperties.vectorSprites[i].shapeGroupIDs[j]--;
                            for (int j = vectorSpritesProperties.vectorSprites[i].shapeGroupIDs.Count - 1; j >= 0; j--)
                                if (vectorSpritesProperties.vectorSprites[i].shapeGroupIDs[j] == -1) {
                                    vectorSpritesProperties.vectorSprites[i].shapeGroupIDs.RemoveAt(j);
                                    vectorSpritesProperties.vectorSprites[i].shapeIDs.RemoveAt(j);
                                }
                        }
                        vectorSpritesProperties.shapeGroups.RemoveAt(vectorSpritesProperties.selectedEntities[k].primaryID);
                        for (int i = 0; i < vectorSpritesProperties.selectedEntities.Count; i++)
                            if (vectorSpritesProperties.selectedEntities[i].primaryID > vectorSpritesProperties.selectedEntities[k].primaryID)
                                vectorSpritesProperties.selectedEntities[i].primaryID--;
                        vectorSpritesProperties.selectedEntities.RemoveAt(k);
                    }
                    endUndo(true);
                }
            }
        }

        //If a shape is selected...
        else if (vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.Shape) {

            //Determine whether a single shape is selected.
            bool singleShape = vectorSpritesProperties.selectedEntities.Count == 1;

            //Store the shape and shape's property path for a single shape.
            VectorSprites.Shape selectedShape = vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[0].primaryID].
                    shapes[vectorSpritesProperties.selectedEntities[0].secondaryID];
            string shapePropertyPath = "shapeGroups.Array.data[" + vectorSpritesProperties.selectedEntities[0].primaryID + "].shapes.Array.data[" +
                    vectorSpritesProperties.selectedEntities[0].secondaryID + "]";

            //Display a "Shape Properties" header (all shape properties are for single shapes only).
            if (singleShape) {
                top = sectionHeader("Shape Properties", (int) ((rowPixels * 7) + (rowGapPixels * 6) + borderPixels), false);

                //Allow the shape name to be edited.
                EditorGUI.BeginChangeCheck();
                beginProperty(getGUIPercentageRectangle(false, 2, 30, top), shapePropertyPath + ".name", new GUIContent("Name", "The name of the shape."));
                string temporaryShapeName = GUI.TextField(getGUIPercentageRectangle(false, 32.5f, 64.5f, top),
                        vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[0].primaryID].shapes[
                        vectorSpritesProperties.selectedEntities[0].secondaryID].name.Replace("\n", "").Replace("\r", ""), 100);
                endProperty();
                if (selectedShape.name.Trim() == "")
                    selectedShape.name = getNextFreeShapeName();
                if (EditorGUI.EndChangeCheck()) {
                    beginUndo("Change Shape Name");
                    selectedShape.name = temporaryShapeName;
                    endUndo(false);
                }
                top += rowPixels + rowGapPixels;

                //Add the wrapping flags (single shapes only).
                EditorGUI.BeginChangeCheck();
                beginProperty(getGUIPercentageRectangle(false, 2, 30, top), shapePropertyPath + ".wrapLeft", new GUIContent("Wrap Left",
                        "Whether to wrap the shape to the left of the original."));
                bool temporaryWrapLeft = EditorGUI.Toggle(getGUIPercentageRectangle(false, 32.5f, 5, top), selectedShape.wrapLeft);
                endProperty();
                if (EditorGUI.EndChangeCheck()) {
                    beginUndo((temporaryWrapLeft ? "Set" : "Clear") + " Wrap Left Flag");
                    selectedShape.wrapLeft = temporaryWrapLeft;
                    endUndo(true);
                }
                EditorGUI.BeginDisabledGroup(!selectedShape.wrapLeft);
                EditorGUI.BeginChangeCheck();
                beginProperty(getGUIPercentageRectangle(false, 55, 22, top), shapePropertyPath + ".wrapLeftOnTop", new GUIContent("On Top",
                        "If the shape is wrapped to the left of the original, determines whether the wrapped shape is on top of the original shape."));
                bool temporaryWrapLeftOnTop = EditorGUI.Toggle(getGUIPercentageRectangle(false, 79, 5, top), selectedShape.wrapLeftOnTop);
                endProperty();
                if (EditorGUI.EndChangeCheck()) {
                    beginUndo((temporaryWrapLeftOnTop ? "Set" : "Clear") + " Wrap Left On Top Flag");
                    selectedShape.wrapLeftOnTop = temporaryWrapLeftOnTop;
                    endUndo(true);
                }
                EditorGUI.EndDisabledGroup();
                top += rowPixels + rowGapPixels;
                EditorGUI.BeginChangeCheck();
                beginProperty(getGUIPercentageRectangle(false, 2, 30, top), shapePropertyPath + ".wrapRight", new GUIContent("Wrap Right",
                        "Whether to wrap the shape to the right of the original."));
                bool temporaryWrapRight = EditorGUI.Toggle(getGUIPercentageRectangle(false, 32.5f, 5, top), selectedShape.wrapRight);
                endProperty();
                if (EditorGUI.EndChangeCheck()) {
                    beginUndo((temporaryWrapRight ? "Set" : "Clear") + " Wrap Right Flag");
                    selectedShape.wrapRight = temporaryWrapRight;
                    endUndo(true);
                }
                EditorGUI.BeginDisabledGroup(!selectedShape.wrapRight);
                EditorGUI.BeginChangeCheck();
                beginProperty(getGUIPercentageRectangle(false, 55, 22, top), shapePropertyPath + ".wrapRightOnTop", new GUIContent("On Top",
                        "If the shape is wrapped to the right of the original, determines whether the wrapped shape is on top of the original shape."));
                bool temporaryWrapRightOnTop = EditorGUI.Toggle(getGUIPercentageRectangle(false, 79, 5, top), selectedShape.wrapRightOnTop);
                endProperty();
                if (EditorGUI.EndChangeCheck()) {
                    beginUndo((temporaryWrapRightOnTop ? "Set" : "Clear") + " Wrap Right On Top Flag");
                    selectedShape.wrapRightOnTop = temporaryWrapRightOnTop;
                    endUndo(true);
                }
                EditorGUI.EndDisabledGroup();
                top += rowPixels + rowGapPixels;
                EditorGUI.BeginChangeCheck();
                beginProperty(getGUIPercentageRectangle(false, 2, 30, top), shapePropertyPath + ".wrapTop", new GUIContent("Wrap Up",
                        "Whether to wrap the shape above the original."));
                bool temporaryWrapTop = EditorGUI.Toggle(getGUIPercentageRectangle(false, 32.5f, 5, top), selectedShape.wrapTop);
                endProperty();
                if (EditorGUI.EndChangeCheck()) {
                    beginUndo((temporaryWrapTop ? "Set" : "Clear") + " Wrap Top Flag");
                    selectedShape.wrapTop = temporaryWrapTop;
                    endUndo(true);
                }
                EditorGUI.BeginDisabledGroup(!selectedShape.wrapTop);
                EditorGUI.BeginChangeCheck();
                beginProperty(getGUIPercentageRectangle(false, 55, 22, top), shapePropertyPath + ".wrapTopOnTop", new GUIContent("On Top",
                        "If the shape is wrapped above the original, determines whether the wrapped shape is on top of the original shape."));
                bool temporaryWrapTopOnTop = EditorGUI.Toggle(getGUIPercentageRectangle(false, 79, 5, top), selectedShape.wrapTopOnTop);
                endProperty();
                if (EditorGUI.EndChangeCheck()) {
                    beginUndo((temporaryWrapTopOnTop ? "Set" : "Clear") + " Wrap Top On Top Flag");
                    selectedShape.wrapTopOnTop = temporaryWrapTopOnTop;
                    endUndo(true);
                }
                EditorGUI.EndDisabledGroup();
                top += rowPixels + rowGapPixels;
                EditorGUI.BeginChangeCheck();
                beginProperty(getGUIPercentageRectangle(false, 2, 30, top), shapePropertyPath + ".wrapBottom", new GUIContent("Wrap Down",
                        "Whether to wrap the shape below the original."));
                bool temporaryWrapBottom = EditorGUI.Toggle(getGUIPercentageRectangle(false, 32.5f, 5, top), selectedShape.wrapBottom);
                endProperty();
                if (EditorGUI.EndChangeCheck()) {
                    beginUndo((temporaryWrapBottom ? "Set" : "Clear") + " Wrap Bottom Flag");
                    selectedShape.wrapBottom = temporaryWrapBottom;
                    endUndo(true);
                }
                EditorGUI.BeginDisabledGroup(!selectedShape.wrapBottom);
                EditorGUI.BeginChangeCheck();
                beginProperty(getGUIPercentageRectangle(false, 55, 22, top), shapePropertyPath + ".wrapBottomOnTop", new GUIContent("On Top",
                        "If the shape is wrapped below the original, determines whether the wrapped shape is on top of the original shape."));
                bool temporaryWrapBottomOnTop = EditorGUI.Toggle(getGUIPercentageRectangle(false, 79, 5, top), selectedShape.wrapBottomOnTop);
                endProperty();
                if (EditorGUI.EndChangeCheck()) {
                    beginUndo((temporaryWrapBottomOnTop ? "Set" : "Clear") + " Wrap Bottom On Top Flag");
                    selectedShape.wrapBottomOnTop = temporaryWrapBottomOnTop;
                    endUndo(true);
                }
                EditorGUI.EndDisabledGroup();
                top += rowPixels + rowGapPixels;

                //Add the alpha blend mode selection (single shapes only).
                EditorGUI.BeginChangeCheck();
                beginProperty(getGUIPercentageRectangle(false, 2, 30, top), shapePropertyPath + ".alphaBlendMode", new GUIContent("Alpha",
                        "How the alpha (transparency) is blended. \"Blend\" will merge the transparency with whatever is below the shape so it appears on top as " +
                        "a partially transparent shape. \"Overwrite\" will replace the transparency to allow, for example, a sprite to be faded at the edge " +
                        "regardless of how many shapes are underneath this one."));
                VectorSprites.AlphaBlendMode temporaryAlphaBlendMode = (VectorSprites.AlphaBlendMode) EditorGUI.IntPopup(
                        getGUIPercentageRectangle(false, 32.5f, 64.5f, top),
                        (int) selectedShape.alphaBlendMode,
                        enumeratedTypeNames[(int) EnumeratedTypeNameArrays.AlphaBlendMode],
                        enumeratedTypeValues[(int) EnumeratedTypeNameArrays.AlphaBlendMode], enumPopupStyle);
                endProperty();
                if (EditorGUI.EndChangeCheck()) {
                    beginUndo("Change Alpha Blend Mode");
                    selectedShape.alphaBlendMode = temporaryAlphaBlendMode;
                    endUndo(true);
                }
                top += rowPixels + rowGapPixels;
            }

            //Display a "Shape Actions" section.
            top = sectionHeader("Shape Actions" + (singleShape ? "" : " (" + vectorSpritesProperties.selectedEntities.Count + " shapes)"),
                    (int) (rowPixels + (rowPixels * buttonHeightMultiplier * (singleShape ? 6 : 3)) + (rowGapPixels * (singleShape ? 6 : 3)) + borderPixels),
                    false);

            //Move the shapes up or down (single shapes only).
            if (singleShape) {
                EditorGUI.BeginDisabledGroup(vectorSpritesProperties.selectedEntities[0].secondaryID == 0);
                if (GUI.Button(getGUIPercentageRectangle(false, 10, 80, top, rowPixels * buttonHeightMultiplier), new GUIContent("Move Shape Up",
                        "Swap this shape group with the one above it in the shape's group. Shapes are drawn from top to bottom, so moving a shape upwards " +
                        "will move it behind the shape it is being swapped with."))) {
                    beginUndo("Move Shape Up");
                    VectorSprites.Shape swapShape = vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[0].primaryID].shapes[
                            vectorSpritesProperties.selectedEntities[0].secondaryID - 1];
                    vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[0].primaryID].shapes[
                            vectorSpritesProperties.selectedEntities[0].secondaryID - 1] = vectorSpritesProperties.shapeGroups[
                            vectorSpritesProperties.selectedEntities[0].primaryID].shapes[vectorSpritesProperties.selectedEntities[0].secondaryID];
                    vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[0].primaryID].shapes[
                            vectorSpritesProperties.selectedEntities[0].secondaryID] = swapShape;
                    for (int i = 0; i < vectorSpritesProperties.vectorSprites.Count; i++)
                        for (int j = 0; j < vectorSpritesProperties.vectorSprites[i].shapeGroupIDs.Count; j++)
                            if (vectorSpritesProperties.vectorSprites[i].shapeGroupIDs[j] == vectorSpritesProperties.selectedEntities[0].primaryID &&
                                    vectorSpritesProperties.vectorSprites[i].shapeIDs[j] == vectorSpritesProperties.selectedEntities[0].secondaryID)
                                vectorSpritesProperties.vectorSprites[i].shapeIDs[j]--;
                            else if (vectorSpritesProperties.vectorSprites[i].shapeGroupIDs[j] == vectorSpritesProperties.selectedEntities[0].primaryID &&
                                    vectorSpritesProperties.vectorSprites[i].shapeIDs[j] == vectorSpritesProperties.selectedEntities[0].secondaryID - 1)
                                vectorSpritesProperties.vectorSprites[i].shapeIDs[j]++;
                    vectorSpritesProperties.selectedEntities[0].secondaryID--;
                    endUndo(true);
                }
                EditorGUI.EndDisabledGroup();
                top += (rowPixels * buttonHeightMultiplier) + rowGapPixels;
                EditorGUI.BeginDisabledGroup(vectorSpritesProperties.selectedEntities[0].secondaryID == vectorSpritesProperties.shapeGroups[
                        vectorSpritesProperties.selectedEntities[0].primaryID].shapes.Count - 1);
                if (GUI.Button(getGUIPercentageRectangle(false, 10, 80, top, rowPixels * buttonHeightMultiplier), new GUIContent("Move Shape Down",
                        "Swap this shape group with the one below it in the shape's group. Shapes are drawn from top to bottom, so moving a shape downwards " +
                        "will move it in front of the shape it is being swapped with."))) {
                    beginUndo("Move Shape Down");
                    VectorSprites.Shape swapShape = vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[0].primaryID].shapes[
                            vectorSpritesProperties.selectedEntities[0].secondaryID + 1];
                    vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[0].primaryID].shapes[
                            vectorSpritesProperties.selectedEntities[0].secondaryID + 1] = vectorSpritesProperties.shapeGroups[
                                vectorSpritesProperties.selectedEntities[0].primaryID].shapes[vectorSpritesProperties.selectedEntities[0].secondaryID];
                    vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[0].primaryID].shapes[
                            vectorSpritesProperties.selectedEntities[0].secondaryID] = swapShape;
                    for (int i = 0; i < vectorSpritesProperties.vectorSprites.Count; i++)
                        for (int j = 0; j < vectorSpritesProperties.vectorSprites[i].shapeGroupIDs.Count; j++)
                            if (vectorSpritesProperties.vectorSprites[i].shapeGroupIDs[j] == vectorSpritesProperties.selectedEntities[0].primaryID &&
                                    vectorSpritesProperties.vectorSprites[i].shapeIDs[j] == vectorSpritesProperties.selectedEntities[0].secondaryID)
                                vectorSpritesProperties.vectorSprites[i].shapeIDs[j]++;
                            else if (vectorSpritesProperties.vectorSprites[i].shapeGroupIDs[j] == vectorSpritesProperties.selectedEntities[0].primaryID &&
                                    vectorSpritesProperties.vectorSprites[i].shapeIDs[j] == vectorSpritesProperties.selectedEntities[0].secondaryID + 1)
                                vectorSpritesProperties.vectorSprites[i].shapeIDs[j]--;
                    vectorSpritesProperties.selectedEntities[0].secondaryID++;
                    endUndo(true);
                }
                EditorGUI.EndDisabledGroup();
                top += (rowPixels * buttonHeightMultiplier) + rowGapPixels;
            }

            //Transform the shape (multiple selected shapes allowed).
            if (GUI.Button(getGUIPercentageRectangle(false, 10, 80, top, rowPixels * buttonHeightMultiplier), new GUIContent("Transform Shape" +
                    (singleShape ? "" : "s"), "Position, rotate or scale the shape" + (singleShape ? "" : "s") + "."))) {
                VectorSpritesTransformShape vectorSpritesTransformShape = GetWindow<VectorSpritesTransformShape>();
                vectorSpritesTransformShape.minSize = new Vector2(256, 128);
                vectorSpritesTransformShape.title = "Vector Sprites - Transform Shape";
                vectorSpritesTransformShape.initialise(this, vectorSpritesProperties);
            }
            top += (rowPixels * buttonHeightMultiplier) + rowGapPixels;

            //Duplicate the shape (single shapes only).
            if (singleShape) {
                if (GUI.Button(getGUIPercentageRectangle(false, 10, 80, top, rowPixels * buttonHeightMultiplier), new GUIContent("Duplicate Shape",
                        "Creates a copy of this shape and selects the copy."))) {
                    beginUndo("Duplicate Shape");
                    vectorSpritesProperties.selectedEntities[0].secondaryID = vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[0].
                            primaryID].copyShape(vectorSpritesProperties.selectedEntities[0].secondaryID);
                    endUndo(true);
                }
                top += (rowPixels * buttonHeightMultiplier) + rowGapPixels;
            }

            //Change the shape's shape group (multiple shapes allowed).
            if (GUI.Button(getGUIPercentageRectangle(false, 10, 80, top, rowPixels * buttonHeightMultiplier), new GUIContent("Change Shape Group",
                    "Moves the shape to a different group."))) {
                if (vectorSpritesProperties.shapeGroups.Count <= 1)
                    EditorUtility.DisplayDialog("Change Shape Group", "There are no other shape groups to move " + (singleShape ? "this shape" :
                            "these shapes") + " into.", "OK");
                else {
                    VectorSpritesShapeGroupSelection vectorSpritesShapeGroupSelection = GetWindow<VectorSpritesShapeGroupSelection>();
                    vectorSpritesShapeGroupSelection.minSize = new Vector2(256, 256);
                    vectorSpritesShapeGroupSelection.title = "Vector Sprites - Select Group";
                    vectorSpritesShapeGroupSelection.initialise(this, vectorSpritesProperties);
                }
            }
            top += (rowPixels * buttonHeightMultiplier) + rowGapPixels;

            //Delete the shape (multiple shapes allowed).
            if (GUI.Button(getGUIPercentageRectangle(false, 10, 80, top, rowPixels * buttonHeightMultiplier), new GUIContent(singleShape ? "Delete Shape" :
                    "Delete Shapes", singleShape ? "Delete this shape." : "Delete these shapes.")) && EditorUtility.DisplayDialog(singleShape ? "Delete Shape" :
                    "Delete Shapes", "Are you sure you want to delete " + (singleShape ? "this shape?" : "these shapes?"), "Yes", "No")) {
                beginUndo(singleShape ? "Delete Shape" : "Delete Shapes");
                for (int k = vectorSpritesProperties.selectedEntities.Count - 1; k >= 0; k--) {
                    for (int i = 0; i < vectorSpritesProperties.vectorSprites.Count; i++) {
                        for (int j = 0; j < vectorSpritesProperties.vectorSprites[i].shapeGroupIDs.Count; j++)
                            if (vectorSpritesProperties.vectorSprites[i].shapeGroupIDs[j] == vectorSpritesProperties.selectedEntities[k].primaryID &&
                                    vectorSpritesProperties.vectorSprites[i].shapeIDs[j] == vectorSpritesProperties.selectedEntities[k].secondaryID)
                                vectorSpritesProperties.vectorSprites[i].shapeGroupIDs[j] = -1;
                            else if (vectorSpritesProperties.vectorSprites[i].shapeGroupIDs[j] == vectorSpritesProperties.selectedEntities[k].primaryID &&
                                    vectorSpritesProperties.vectorSprites[i].shapeIDs[j] > vectorSpritesProperties.selectedEntities[k].secondaryID)
                                vectorSpritesProperties.vectorSprites[i].shapeIDs[j]--;
                        for (int j = vectorSpritesProperties.vectorSprites[i].shapeGroupIDs.Count - 1; j >= 0; j--)
                            if (vectorSpritesProperties.vectorSprites[i].shapeGroupIDs[j] == -1) {
                                vectorSpritesProperties.vectorSprites[i].shapeGroupIDs.RemoveAt(j);
                                vectorSpritesProperties.vectorSprites[i].shapeIDs.RemoveAt(j);
                            }
                    }
                    vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[k].primaryID].shapes.RemoveAt(
                            vectorSpritesProperties.selectedEntities[k].secondaryID);
                    for (int i = 0; i < vectorSpritesProperties.selectedEntities.Count; i++)
                        if (vectorSpritesProperties.selectedEntities[i].primaryID == vectorSpritesProperties.selectedEntities[k].primaryID &&
                                vectorSpritesProperties.selectedEntities[i].secondaryID > vectorSpritesProperties.selectedEntities[k].secondaryID)
                            vectorSpritesProperties.selectedEntities[i].secondaryID--;
                    vectorSpritesProperties.selectedEntities.RemoveAt(k);
                }
                endUndo(true);
                for (int i = 0; i < vectorSpritesProperties.shapeGroups.Count; i++)
                    for (int j = 0; j < vectorSpritesProperties.shapeGroups[i].shapes.Count; j++)
                        vectorSpritesProperties.shapeGroups[i].shapes[j].resetAllMeshes();
            }
            else if (singleShape) {

                //Add a section for the shape style and a dropdown for the type.
                top += (rowPixels * buttonHeightMultiplier) + rowGapPixels;
                top = sectionHeader("Shape Style", (int) (position.height - (borderPixels * 2) - top) - 4, false);
                EditorGUI.LabelField(getGUIPercentageRectangle(false, 2, 30, top), new GUIContent("Type", "The type of shape properties to display. Any " +
                        "combination of these types can be used to make a shape."));
                EditorGUI.BeginChangeCheck();
                VectorSprites.ShapeStyleType temporaryShapeStyle = (VectorSprites.ShapeStyleType) EditorGUI.IntPopup(
                        getGUIPercentageRectangle(false, 32.5f, 64.5f, top),
                        (int) selectedShape.shapeStyleType,
                        enumeratedTypeNames[(int) EnumeratedTypeNameArrays.ShapeStyleType],
                        enumeratedTypeValues[(int) EnumeratedTypeNameArrays.ShapeStyleType], enumPopupStyle);
                if (EditorGUI.EndChangeCheck()) {
                    beginUndo("Change Shape Style");
                    selectedShape.shapeStyleType = temporaryShapeStyle;
                    endUndo(true);
                }
                top += rowPixels + rowGapPixels;

                //Outline.
                if (selectedShape.shapeStyleType == VectorSprites.ShapeStyleType.Outline) {
                    EditorGUI.BeginChangeCheck();
                    beginProperty(getGUIPercentageRectangle(false, 2, 30, top), shapePropertyPath + ".outlineWidth", new GUIContent("Width",
                            "The width of the outline."));
                    float temporaryOutlineWidth = EditorGUI.Slider(getGUIPercentageRectangle(false, 32.5f, 64.5f, top), selectedShape.outlineWidth, 0.01f, 1);
                    endProperty();
                    bool endChangeCheck = EditorGUI.EndChangeCheck();
                    if (endChangeCheck || contextClick) {
                        if (selectedShape.outlineMesh != null)
                            DestroyImmediate(selectedShape.outlineMesh);
                        selectedShape.outlineMesh = null;
                        if (endChangeCheck) {
                            beginUndo("Change Outline Width");
                            selectedShape.outlineWidth = temporaryOutlineWidth;
                            endUndo(false);
                        }
                    }
                    top += rowPixels + rowGapPixels;
                    drawFillProperties(selectedShape.outlines, ref top, ref selectedShape.outlineSelectedLayer, selectedShape.alphaBlendMode,
                            shapePropertyPath + ".outlines.Array.data", false);
                }

                //Fill.
                else if (selectedShape.shapeStyleType == VectorSprites.ShapeStyleType.Fill)
                    drawFillProperties(selectedShape.shapes, ref top, ref selectedShape.fillSelectedLayer, selectedShape.alphaBlendMode, shapePropertyPath +
                            ".shapes.Array.data", selectedShape.shapeMesh != null && selectedShape.shapeMesh.vertexCount == 0);

                //Shadows.
                else if (selectedShape.shapeStyleType == VectorSprites.ShapeStyleType.Shadow)
                    drawShadowProperties(selectedShape.shadows, ref top, ref selectedShape.shadowSelectedLayer, shapePropertyPath + ".shadows.Array.data",
                            selectedShape.shapeMesh != null && selectedShape.shapeMesh.vertexCount == 0);

                //Pillow shading.
                else if (selectedShape.shapeStyleType == VectorSprites.ShapeStyleType.Pillow)
                    drawEdgeProperties(selectedShape, true, ref top, ref selectedShape.pillowSelectedLayer, shapePropertyPath + ".pillows.Array.data");

                //Glow shading.
                else if (selectedShape.shapeStyleType == VectorSprites.ShapeStyleType.Glow)
                    drawEdgeProperties(selectedShape, false, ref top, ref selectedShape.glowSelectedLayer, shapePropertyPath + ".glows.Array.data");
            }
        }
    }

    //Called when the editor window is destroyed.
    void OnDestroy() {
        for (int i = 0; i < vectorSpritesProperties.shapeGroups.Count; i++)
            for (int j = 0; j < vectorSpritesProperties.shapeGroups[i].shapes.Count; j++)
                vectorSpritesProperties.shapeGroups[i].shapes[j].resetAllMeshes();
        DestroyImmediate(vectorSpritesRendererGameObject);
        if (renderTexture != null)
            DestroyImmediate(renderTexture);
        for (int i = 0; i < materials.Length; i++)
            if (materials[i] != null)
                DestroyImmediate(materials[i]);
        GetWindowWithRect<VectorSpritesShapeGroupSelection>(new Rect(-1000, -1000, 500, 500)).Close();
        GetWindowWithRect<VectorSpritesTransformShape>(new Rect(-1000, -1000, 500, 500)).Close();
        GetWindowWithRect<VectorSpritesExport>(new Rect(-1000, -1000, 500, 500)).Close();
        GetWindowWithRect<VectorSpritesVersionChanges>(new Rect(-1000, -1000, 500, 500)).Close();
        Undo.undoRedoPerformed -= undoRedoPerformed;
    }

    //Transforms a Vector2 whose co-ordinates are in the space passed in (in "bounds") to the 0..1 space.
    Vector2 transformPointTo01Space(Vector2 point, Rect bounds, bool takeZoomIntoAccount) {
        point = new Vector2((point.x - bounds.xMin) / bounds.width, (point.y - bounds.yMin) / bounds.height);
        if (takeZoomIntoAccount)
            point = ((point - new Vector2(0.5f, 0.5f)) / vectorSpritesProperties.zoom) + vectorSpritesProperties.zoomCentre;
        return point;
    }

    //Returns a new rectangle whose width is a given percentage of the window and whose height is fixed.
    Rect getGUIPercentageRectangle(bool leftHandSide, float left, float width, float top) {
        return getGUIPercentageRectangle(leftHandSide, left, width, top, rowPixels);
    }
    Rect getGUIPercentageRectangle(bool leftHandSide, float left, float width, float top, float height) {
        return new Rect((leftHandSide ? borderPixels : (position.width - borderPixels - GUIWidth)) + (GUIWidth * (left / 100)), top, GUIWidth * (width / 100),
                height);
    }

    //Display a GUI section header.
    float sectionHeader(string title, int height, bool leftHandSide) {
        sectionHeaderTop += borderPixels;
        Rect headerRectangle = getGUIPercentageRectangle(leftHandSide, 0, 100, sectionHeaderTop, height);
        sectionHeaderTop += headerRectangle.height;
        drawBorder(headerRectangle, Color.white);
        headerRectangle.height = rowPixels;
        GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.alignment = TextAnchor.MiddleCenter;
        headerRectangle.yMin += 3;
        headerRectangle.yMax += 3;
        EditorGUI.LabelField(headerRectangle, title, headerStyle);
        return headerRectangle.yMin + rowPixels + rowGapPixels;
    }

    //Given an array of "Fill" instances - draws the associated properties.
    void drawFillProperties(VectorSprites.Fill[] fills, ref float top, ref int selectedLayer, VectorSprites.AlphaBlendMode alphaBlendMode,
            string propertyPath, bool invalidMesh) {

        //If the mesh is invalid, display a message indicating this.
        if (invalidMesh) {
            EditorGUI.HelpBox(getGUIPercentageRectangle(false, 2, 96, top, rowPixels * 4), "The shape could not be generated because it is too complex or " +
                    "the points overlap each other too much. Please drag the points into a less-complex shape.", MessageType.Error);
            return;
        }

        //Initialise styles.
        GUIStyle enumPopupStyle = new GUIStyle(GUI.skin.GetStyle("popup"));
        enumPopupStyle.fixedHeight = rowPixels;
        enumPopupStyle.padding.top = 1;
        enumPopupStyle.padding.bottom = 0;

        //Draw the layer selection.
        if (fills.Length > 1) {
            EditorGUI.LabelField(getGUIPercentageRectangle(false, 2, 30, top), new GUIContent("Layer", "The current layer to edit."));
            for (int i = 0; i < fills.Length; i++) {
                Rect textureRectangle = getGUIPercentageRectangle(false, (i * 16.125f) + 33.5f, 13.5f, top + 2, GUIWidth * 0.135f);
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && textureRectangle.Contains(Event.current.mousePosition)) {
                    beginUndo("Change Selected Layer");
                    selectedLayer = i;
                    endUndo(true);
                }
                if (Event.current.type == EventType.Repaint) {
                    drawBorder(new Rect(textureRectangle.xMin - 2, textureRectangle.yMin - 2, textureRectangle.width + 4, textureRectangle.height + 4),
                            i == selectedLayer ? Color.white : Color.gray);
                    if (fills[i].style != VectorSprites.FillStyle.None) {
                        Color colour1, colour2;
                        if (alphaBlendMode == VectorSprites.AlphaBlendMode.Blend) {
                            colour1 = fills[i].colour1;
                            colour2 = fills[i].colour2;
                            if (!fillUsesSecondColour(fills[i]))
                                colour2.a = 0;
                            if (colour1.a < 0.0001f && colour2.a < 0.0001f) {
                                colour1.a = 1;
                                colour2.a = 1;
                            }
                            else {
                                colour1.a = colour1.a >= colour2.a ? 1 : (colour1.a / colour2.a);
                                colour2.a = colour2.a >= colour1.a ? 1 : (colour2.a / colour1.a);
                            }
                        }
                        else {
                            colour1 = Color.white;
                            colour2 = Color.black;
                        }
                        materials[(int) Materials.Shape].SetInt("colourSourceBlend", (int) UnityEngine.Rendering.BlendMode.SrcAlpha);
                        materials[(int) Materials.Shape].SetInt("colourDestinationBlend", (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        materials[(int) Materials.Shape].SetInt("alphaSourceBlend", (int) UnityEngine.Rendering.BlendMode.One);
                        materials[(int) Materials.Shape].SetInt("alphaDestinationBlend", (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        materials[(int) Materials.Shape].SetInt("alphaBlendOperation", (int) UnityEngine.Rendering.BlendOp.Add);
                        materials[(int) Materials.Shape].SetInt("fillStyle", (int) fills[i].style);
                        materials[(int) Materials.Shape].SetColor("colour1", colour1);
                        if (fills[i].style != VectorSprites.FillStyle.SolidColour && fills[i].style != VectorSprites.FillStyle.Noise) {
                            materials[(int) Materials.Shape].SetColor("colour2", colour2);
                            if (fills[i].style == VectorSprites.FillStyle.AngledLinear || fills[i].style == VectorSprites.FillStyle.HorizontalLinear ||
                                    fills[i].style == VectorSprites.FillStyle.Radial || fills[i].style == VectorSprites.FillStyle.VerticalLinear ||
                                    fills[i].style == VectorSprites.FillStyle.AngledBilinear || fills[i].style == VectorSprites.FillStyle.HorizontalBilinear ||
                                    fills[i].style == VectorSprites.FillStyle.VerticalBilinear) {
                                materials[(int) Materials.Shape].SetFloat("colourBias", fills[i].colourBias);
                                materials[(int) Materials.Shape].SetInt("colourBands", fills[i].colourBands);
                            }
                        }
                        if (fills[i].style == VectorSprites.FillStyle.HorizontalLinear || fills[i].style == VectorSprites.FillStyle.AngledLinear ||
                                fills[i].style == VectorSprites.FillStyle.VerticalBars || fills[i].style == VectorSprites.FillStyle.AngledBars ||
                                fills[i].style == VectorSprites.FillStyle.Checkerboard || fills[i].style == VectorSprites.FillStyle.Radial ||
                                fills[i].style == VectorSprites.FillStyle.AngledBilinear || fills[i].style == VectorSprites.FillStyle.HorizontalBilinear) {
                            materials[(int) Materials.Shape].SetFloat("areaFromX", textureRectangle.xMin / position.width);
                            materials[(int) Materials.Shape].SetFloat("areaToX", textureRectangle.xMax / position.width);
                        }
                        if (fills[i].style == VectorSprites.FillStyle.VerticalLinear || fills[i].style == VectorSprites.FillStyle.AngledLinear ||
                                fills[i].style == VectorSprites.FillStyle.HorizontalBars || fills[i].style == VectorSprites.FillStyle.AngledBars ||
                                fills[i].style == VectorSprites.FillStyle.Checkerboard || fills[i].style == VectorSprites.FillStyle.Radial ||
                                fills[i].style == VectorSprites.FillStyle.AngledBilinear || fills[i].style == VectorSprites.FillStyle.VerticalBilinear) {
                            materials[(int) Materials.Shape].SetFloat("areaFromY", (textureRectangle.yMin + (Screen.height - position.height)) / Screen.height);
                            materials[(int) Materials.Shape].SetFloat("areaToY", (textureRectangle.yMax + (Screen.height - position.height)) / Screen.height);
                        }
                        if (fills[i].style == VectorSprites.FillStyle.AngledLinear || fills[i].style == VectorSprites.FillStyle.AngledBars ||
                                fills[i].style == VectorSprites.FillStyle.Checkerboard || fills[i].style == VectorSprites.FillStyle.AngledBilinear)
                            materials[(int) Materials.Shape].SetFloat("angle", fills[i].angle * Mathf.Deg2Rad);
                        if (fills[i].style == VectorSprites.FillStyle.HorizontalBars || fills[i].style == VectorSprites.FillStyle.VerticalBars ||
                                fills[i].style == VectorSprites.FillStyle.AngledBars || fills[i].style == VectorSprites.FillStyle.Checkerboard)
                            materials[(int) Materials.Shape].SetInt("bars", fills[i].bars);
                        if (fills[i].style == VectorSprites.FillStyle.Noise) {
                            materials[(int) Materials.Shape].SetInt("noiseType", (int) fills[i].noiseType);
                            materials[(int) Materials.Shape].SetFloat("noiseLevel", fills[i].noiseLevel);
                        }
                        if (fills[i].style == VectorSprites.FillStyle.Radial)
                            materials[(int) Materials.Shape].SetFloat("radialSize", fills[i].radialSize);
                        if (fills[i].style == VectorSprites.FillStyle.AngledBars || fills[i].style == VectorSprites.FillStyle.AngledLinear ||
                                fills[i].style == VectorSprites.FillStyle.Checkerboard || fills[i].style == VectorSprites.FillStyle.HorizontalLinear ||
                                fills[i].style == VectorSprites.FillStyle.Radial || fills[i].style == VectorSprites.FillStyle.VerticalBars ||
                                fills[i].style == VectorSprites.FillStyle.AngledBilinear || fills[i].style == VectorSprites.FillStyle.HorizontalBilinear)
                            materials[(int) Materials.Shape].SetFloat("centreX", fills[i].centre.x);
                        if (fills[i].style == VectorSprites.FillStyle.Checkerboard || fills[i].style == VectorSprites.FillStyle.HorizontalBars ||
                                fills[i].style == VectorSprites.FillStyle.Radial || fills[i].style == VectorSprites.FillStyle.VerticalLinear ||
                                fills[i].style == VectorSprites.FillStyle.VerticalBilinear)
                            materials[(int) Materials.Shape].SetFloat("centreY", fills[i].centre.y);
                        materials[(int) Materials.Shape].SetFloat("zoom", 1);
                        materials[(int) Materials.Shape].SetFloat("zoomCentreX", 0.5f);
                        materials[(int) Materials.Shape].SetFloat("zoomCentreY", 0.5f);
                        materials[(int) Materials.Shape].SetPass(0);
                        EditorGUI.DrawPreviewTexture(textureRectangle, Texture2D.whiteTexture, materials[(int) Materials.Shape]);
                    }
                }
            }
            top += (GUIWidth * 0.135f) + rowGapPixels + 6;
        }

        //Draw the properties for this layer.
        propertyPath += "[" + selectedLayer + "]";
        EditorGUI.BeginChangeCheck();
        beginProperty(getGUIPercentageRectangle(false, 2, 30, top), propertyPath + ".style", new GUIContent("Style", "The style to use for this layer."));
        VectorSprites.FillStyle temporaryFillStyle = (VectorSprites.FillStyle) EditorGUI.IntPopup(getGUIPercentageRectangle(false, 32.5f, 64.5f, top),
                (int) fills[selectedLayer].style, enumeratedTypeNames[(int) EnumeratedTypeNameArrays.FillStyle],
                enumeratedTypeValues[(int) EnumeratedTypeNameArrays.FillStyle], enumPopupStyle);
        endProperty();
        if (EditorGUI.EndChangeCheck()) {
            beginUndo("Change Layer Style");
            fills[selectedLayer].style = temporaryFillStyle;
            endUndo(true);
        }
        top += rowPixels + rowGapPixels;
        if (fills[selectedLayer].style != VectorSprites.FillStyle.None) {
            EditorGUI.BeginChangeCheck();
            beginProperty(getGUIPercentageRectangle(false, 2, 30, top), propertyPath + ".colour1",
                    fillUsesSecondColour(fills[selectedLayer]) ? new GUIContent("Colour 1", "The first colour to apply to the selected style.") :
                    new GUIContent("Colour", "The colour to apply to the selected style."));
            Color temporaryColour = EditorGUI.ColorField(getGUIPercentageRectangle(false, 32.5f, 64.5f, top), fills[selectedLayer].colour1);
            endProperty();
            if (EditorGUI.EndChangeCheck()) {
                beginUndo("Change Colour");
                fills[selectedLayer].colour1 = temporaryColour;
                endUndo(true);
            }
            top += rowPixels + rowGapPixels;
            if (fillUsesSecondColour(fills[selectedLayer])) {
                EditorGUI.BeginChangeCheck();
                beginProperty(getGUIPercentageRectangle(false, 2, 30, top), propertyPath + ".colour2", new GUIContent("Colour 2",
                        "The second colour to apply to the selected style."));
                temporaryColour = EditorGUI.ColorField(getGUIPercentageRectangle(false, 32.5f, 64.5f, top), fills[selectedLayer].colour2);
                endProperty();
                if (EditorGUI.EndChangeCheck()) {
                    beginUndo("Change Colour");
                    fills[selectedLayer].colour2 = temporaryColour;
                    endUndo(true);
                }
                top += rowPixels + rowGapPixels;
                if (fills[selectedLayer].style == VectorSprites.FillStyle.HorizontalLinear ||
                        fills[selectedLayer].style == VectorSprites.FillStyle.VerticalLinear ||
                        fills[selectedLayer].style == VectorSprites.FillStyle.AngledLinear ||
                        fills[selectedLayer].style == VectorSprites.FillStyle.AngledBilinear ||
                        fills[selectedLayer].style == VectorSprites.FillStyle.HorizontalBilinear ||
                        fills[selectedLayer].style == VectorSprites.FillStyle.VerticalBilinear ||
                        fills[selectedLayer].style == VectorSprites.FillStyle.Radial) {
                    EditorGUI.BeginChangeCheck();
                    beginProperty(getGUIPercentageRectangle(false, 2, 30, top), propertyPath + ".colourBias", new GUIContent("Bias",
                            "The bias between the two selected colours."));
                    float temporaryColourBias = EditorGUI.Slider(getGUIPercentageRectangle(false, 32.5f, 64.5f, top), fills[selectedLayer].colourBias, 0, 1);
                    endProperty();
                    if (EditorGUI.EndChangeCheck()) {
                        beginUndo("Change Colour Bias");
                        fills[selectedLayer].colourBias = temporaryColourBias;
                        endUndo(false);
                    }
                    top += rowPixels + rowGapPixels;
                    EditorGUI.BeginChangeCheck();
                    beginProperty(getGUIPercentageRectangle(false, 2, 30, top), propertyPath + ".colourBands", new GUIContent("Bands",
                            "The number of bands to split the colour into. The default is the maximum value of 255, which produces smooth colouring."));
                    int temporaryColourBands = Math.Max(Math.Min(EditorGUI.IntField(getGUIPercentageRectangle(false, 32.5f, 64.5f, top),
                            fills[selectedLayer].colourBands), 255), 2);
                    endProperty();
                    if (EditorGUI.EndChangeCheck()) {
                        beginUndo("Change Colour Bands");
                        fills[selectedLayer].colourBands = temporaryColourBands;
                        endUndo(false);
                    }
                    top += rowPixels + rowGapPixels;
                }
            }
            if (fills[selectedLayer].style == VectorSprites.FillStyle.HorizontalLinear ||
                    fills[selectedLayer].style == VectorSprites.FillStyle.VerticalLinear ||
                    fills[selectedLayer].style == VectorSprites.FillStyle.AngledLinear ||
                    fills[selectedLayer].style == VectorSprites.FillStyle.HorizontalBilinear ||
                    fills[selectedLayer].style == VectorSprites.FillStyle.VerticalBilinear ||
                    fills[selectedLayer].style == VectorSprites.FillStyle.AngledBilinear ||
                    fills[selectedLayer].style == VectorSprites.FillStyle.HorizontalBars ||
                    fills[selectedLayer].style == VectorSprites.FillStyle.VerticalBars ||
                    fills[selectedLayer].style == VectorSprites.FillStyle.AngledBars ||
                    fills[selectedLayer].style == VectorSprites.FillStyle.Checkerboard ||
                    fills[selectedLayer].style == VectorSprites.FillStyle.Radial) {
                EditorGUI.BeginChangeCheck();
                beginProperty(getGUIPercentageRectangle(false, 2, 30, top), propertyPath + ".area", new GUIContent("Area", "The area to apply the style to - " +
                        "either the entire sprite or the shape itself."));
                VectorSprites.FillArea temporaryArea = (VectorSprites.FillArea) EditorGUI.IntPopup(getGUIPercentageRectangle(false, 32.5f, 64.5f, top),
                        (int) fills[selectedLayer].area, enumeratedTypeNames[(int) EnumeratedTypeNameArrays.FillArea],
                        enumeratedTypeValues[(int) EnumeratedTypeNameArrays.FillArea], enumPopupStyle);
                endProperty();
                if (EditorGUI.EndChangeCheck()) {
                    beginUndo("Change Area");
                    fills[selectedLayer].area = temporaryArea;
                    endUndo(true);
                }
                top += rowPixels + rowGapPixels;
            }
            if (fills[selectedLayer].style == VectorSprites.FillStyle.AngledLinear || fills[selectedLayer].style == VectorSprites.FillStyle.AngledBilinear ||
                    fills[selectedLayer].style == VectorSprites.FillStyle.Checkerboard || fills[selectedLayer].style == VectorSprites.FillStyle.AngledBars) {
                EditorGUI.BeginChangeCheck();
                beginProperty(getGUIPercentageRectangle(false, 2, 30, top), propertyPath + ".angle", new GUIContent("Angle",
                        "The rotation applied to the style."));
                float temporaryAngle = EditorGUI.Slider(getGUIPercentageRectangle(false, 32.5f, 64.5f, top), fills[selectedLayer].angle, 0, 360);
                endProperty();
                if (EditorGUI.EndChangeCheck()) {
                    beginUndo("Change Angle");
                    fills[selectedLayer].angle = temporaryAngle;
                    endUndo(false);
                }
                top += rowPixels + rowGapPixels;
            }
            if (fills[selectedLayer].style == VectorSprites.FillStyle.HorizontalBars || fills[selectedLayer].style == VectorSprites.FillStyle.VerticalBars ||
                    fills[selectedLayer].style == VectorSprites.FillStyle.AngledBars || fills[selectedLayer].style == VectorSprites.FillStyle.Checkerboard) {
                EditorGUI.BeginChangeCheck();
                beginProperty(getGUIPercentageRectangle(false, 2, 30, top), propertyPath + ".bars",
                        fills[selectedLayer].style == VectorSprites.FillStyle.Checkerboard ?
                        new GUIContent("Squares", "The number of squares going across/down the checkerboard.") : new GUIContent("Bars", "The number of bars."));
                int temporaryBars = Math.Max(Math.Min(EditorGUI.IntField(getGUIPercentageRectangle(false, 32.5f, 64.5f, top), fills[selectedLayer].bars), 1024),
                        2);
                endProperty();
                if (EditorGUI.EndChangeCheck()) {
                    beginUndo("Change " + (fills[selectedLayer].style == VectorSprites.FillStyle.Checkerboard ? "Squares" : "Bars"));
                    fills[selectedLayer].bars = temporaryBars;
                    endUndo(false);
                }
                top += rowPixels + rowGapPixels;
            }
            if (fills[selectedLayer].style == VectorSprites.FillStyle.Noise) {
                EditorGUI.BeginChangeCheck();
                beginProperty(getGUIPercentageRectangle(false, 2, 30, top), propertyPath + ".noiseType", new GUIContent("Noise Type",
                        "The type of noise to apply to the style."));
                VectorSprites.NoiseType temporaryNoiseType = (VectorSprites.NoiseType) EditorGUI.IntPopup(getGUIPercentageRectangle(false, 32.5f, 64.5f, top),
                        (int) fills[selectedLayer].noiseType, enumeratedTypeNames[(int) EnumeratedTypeNameArrays.NoiseType],
                        enumeratedTypeValues[(int) EnumeratedTypeNameArrays.NoiseType], enumPopupStyle);
                endProperty();
                if (EditorGUI.EndChangeCheck()) {
                    beginUndo("Change Noise Type");
                    fills[selectedLayer].noiseType = temporaryNoiseType;
                    endUndo(true);
                }
                top += rowPixels + rowGapPixels;
                EditorGUI.BeginChangeCheck();
                beginProperty(getGUIPercentageRectangle(false, 2, 30, top), propertyPath + ".noiseLevel", new GUIContent("Noise Level",
                        "The amount of noise to apply."));
                float temporaryNoiseLevel = EditorGUI.Slider(getGUIPercentageRectangle(false, 32.5f, 64.5f, top), fills[selectedLayer].noiseLevel, 0.01f, 1);
                endProperty();
                if (EditorGUI.EndChangeCheck()) {
                    beginUndo("Change Noise Level");
                    fills[selectedLayer].noiseLevel = temporaryNoiseLevel;
                    endUndo(false);
                }
                top += rowPixels + rowGapPixels;
            }
            if (fills[selectedLayer].style == VectorSprites.FillStyle.AngledBars ||
                    fills[selectedLayer].style == VectorSprites.FillStyle.AngledLinear ||
                    fills[selectedLayer].style == VectorSprites.FillStyle.Checkerboard ||
                    fills[selectedLayer].style == VectorSprites.FillStyle.HorizontalLinear ||
                    fills[selectedLayer].style == VectorSprites.FillStyle.AngledBilinear ||
                    fills[selectedLayer].style == VectorSprites.FillStyle.HorizontalBilinear ||
                    fills[selectedLayer].style == VectorSprites.FillStyle.Radial ||
                    fills[selectedLayer].style == VectorSprites.FillStyle.VerticalBars) {
                EditorGUI.BeginChangeCheck();
                beginProperty(getGUIPercentageRectangle(false, 2, 30, top), propertyPath + ".centre.x", new GUIContent("Centre" +
                        (fills[selectedLayer].style == VectorSprites.FillStyle.AngledBars ||
                        fills[selectedLayer].style == VectorSprites.FillStyle.AngledBilinear ||
                        fills[selectedLayer].style == VectorSprites.FillStyle.AngledLinear ? "" : " X"), "The amount to offset the style horizontally."));
                float temporaryCentreX = EditorGUI.Slider(getGUIPercentageRectangle(false, 32.5f, 64.5f, top), fills[selectedLayer].centre.x, 0, 1);
                endProperty();
                if (EditorGUI.EndChangeCheck()) {
                    beginUndo("Change Centre");
                    fills[selectedLayer].centre.x = temporaryCentreX;
                    endUndo(false);
                }
                top += rowPixels + rowGapPixels;
            }
            if (fills[selectedLayer].style == VectorSprites.FillStyle.Checkerboard || fills[selectedLayer].style == VectorSprites.FillStyle.HorizontalBars ||
                    fills[selectedLayer].style == VectorSprites.FillStyle.Radial || fills[selectedLayer].style == VectorSprites.FillStyle.VerticalLinear ||
                    fills[selectedLayer].style == VectorSprites.FillStyle.VerticalBilinear) {
                EditorGUI.BeginChangeCheck();
                beginProperty(getGUIPercentageRectangle(false, 2, 30, top), propertyPath + ".centre.y", new GUIContent("Centre Y",
                        "The amount to offset the style vertically."));
                float temporaryCentreY = EditorGUI.Slider(getGUIPercentageRectangle(false, 32.5f, 64.5f, top), fills[selectedLayer].centre.y, 0, 1);
                endProperty();
                if (EditorGUI.EndChangeCheck()) {
                    beginUndo("Change Centre");
                    fills[selectedLayer].centre.y = temporaryCentreY;
                    endUndo(false);
                }
                top += rowPixels + rowGapPixels;
            }
            if (fills[selectedLayer].style == VectorSprites.FillStyle.Radial) {
                EditorGUI.BeginChangeCheck();
                beginProperty(getGUIPercentageRectangle(false, 2, 30, top), propertyPath + ".radialSize", new GUIContent("Size", "The size of the radial."));
                float temporaryRadialSize = EditorGUI.Slider(getGUIPercentageRectangle(false, 32.5f, 64.5f, top), fills[selectedLayer].radialSize, 0.01f, 1);
                endProperty();
                if (EditorGUI.EndChangeCheck()) {
                    beginUndo("Change Radial Size");
                    fills[selectedLayer].radialSize = temporaryRadialSize;
                    endUndo(false);
                }
                top += rowPixels + rowGapPixels;
            }
        }
    }

    //Draw the shadow properties. Note that only a single shadow is supported at the moment, but provision has been left in for more shadows to be implemented
    //in the future (the "selectedLayer" parameter). Hence the shadows array of length one.
    void drawShadowProperties(VectorSprites.Shadow[] shadows, ref float top, ref int selectedLayer, string propertyPath, bool invalidMesh) {

        //If the mesh is invalid, display a message indicating this.
        if (invalidMesh) {
            EditorGUI.HelpBox(getGUIPercentageRectangle(false, 2, 96, top, rowPixels * 4), "The shape could not be generated because it is too complex or " +
                    "the points overlap each other too much. Please drag the points into a less-complex shape.", MessageType.Error);
            return;
        }

        //Append the selected layer onto the property path. This is always 0 at the moment because only a single shadow layer is supported, but the layer
        //variable has been added for future expansion.
        propertyPath += "[" + selectedLayer + "]";

        //Draw the shadow properties.
        EditorGUI.BeginChangeCheck();
        beginProperty(getGUIPercentageRectangle(false, 2, 30, top), propertyPath + ".visible", new GUIContent("Visible", "Whether to display the shadow."));
        bool temporaryShadowVisible = EditorGUI.Toggle(getGUIPercentageRectangle(false, 32.5f, 64.5f, top), shadows[0].visible);
        endProperty();
        if (EditorGUI.EndChangeCheck()) {
            beginUndo("Change Shadow Visibility");
            shadows[0].visible = temporaryShadowVisible;
            endUndo(true);
        }
        top += rowPixels + rowGapPixels;
        if (shadows[0].visible) {
            EditorGUI.BeginChangeCheck();
            beginProperty(getGUIPercentageRectangle(false, 2, 30, top), propertyPath + ".offset", new GUIContent("Offset",
                    "The amount to offset the shadow from the shape itself."));
            Vector2 temporaryShadowOffset = EditorGUI.Vector2Field(getGUIPercentageRectangle(false, 32.5f, 64.5f, top), "", shadows[0].offset);
            endProperty();
            if (EditorGUI.EndChangeCheck()) {
                beginUndo("Change Shadow Offset");
                shadows[0].offset = temporaryShadowOffset;
                endUndo(false);
            }
            top += rowPixels + rowGapPixels;
            EditorGUI.BeginChangeCheck();
            beginProperty(getGUIPercentageRectangle(false, 2, 30, top), propertyPath + ".colour", new GUIContent("Colour", "The colour of the shadow."));
            Color temporaryShadowColour = EditorGUI.ColorField(getGUIPercentageRectangle(false, 32.5f, 64.5f, top), shadows[0].colour);
            endProperty();
            if (EditorGUI.EndChangeCheck()) {
                beginUndo("Change Shadow Colour");
                shadows[0].colour = temporaryShadowColour;
                endUndo(false);
            }
            top += rowPixels + rowGapPixels;
        }
    }

    //Draw the edge properties (pillow and glow).
    void drawEdgeProperties(VectorSprites.Shape shape, bool isPillow, ref float top, ref int selectedLayer, string propertyPath) {

        //Get the edges.
        VectorSprites.Edge[] edges = isPillow ? shape.pillows : shape.glows;

        //Initialise styles.
        GUIStyle enumPopupStyle = new GUIStyle(GUI.skin.GetStyle("popup"));
        enumPopupStyle.fixedHeight = rowPixels;
        enumPopupStyle.padding.top = 1;
        enumPopupStyle.padding.bottom = 0;

        //Draw the layer selection.
        if (edges.Length > 1) {
            EditorGUI.LabelField(getGUIPercentageRectangle(false, 2, 30, top), "Layer");
            for (int i = 0; i < edges.Length; i++) {
                Rect textureRectangle = getGUIPercentageRectangle(false, (i * 16.125f) + 33.5f, 13.5f, top + 2, GUIWidth * 0.135f);
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && textureRectangle.Contains(Event.current.mousePosition)) {
                    beginUndo("Change Selected Layer");
                    selectedLayer = i;
                    endUndo(true);
                }
                if (Event.current.type == EventType.Repaint) {
                    drawBorder(new Rect(textureRectangle.xMin - 2, textureRectangle.yMin - 2, textureRectangle.width + 4, textureRectangle.height + 4),
                            i == selectedLayer ? Color.white : Color.gray);
                    if (edges[i].visible) {
                        materials[(int) Materials.Shape].SetInt("fillStyle", (int) (edges[i].type == VectorSprites.EdgeShadingType.Circle ?
                                VectorSprites.FillStyle.Radial : VectorSprites.FillStyle.AngledLinear));
                        float colourFromAlpha = edges[i].colourFrom.a;
                        float colourToAlpha = edges[i].colourTo.a;
                        if (edges[i].colourFrom.a < 0.0001f && edges[i].colourTo.a < 0.0001f) {
                            colourFromAlpha = 1;
                            colourToAlpha = 1;
                        }
                        else {
                            colourFromAlpha = colourFromAlpha >= colourToAlpha ? 1 : (colourFromAlpha / colourToAlpha);
                            colourToAlpha = colourToAlpha >= colourFromAlpha ? 1 : (colourToAlpha / colourFromAlpha);
                        }
                        materials[(int) Materials.Shape].SetColor("colour1", new Color(edges[i].colourTo.r, edges[i].colourTo.g,
                                edges[i].colourTo.b, colourToAlpha));
                        materials[(int) Materials.Shape].SetColor("colour2", new Color(edges[i].colourFrom.r, edges[i].colourFrom.g,
                                edges[i].colourFrom.b, colourFromAlpha));
                        materials[(int) Materials.Shape].SetFloat("colourBias", 1 - edges[i].colourBias);
                        materials[(int) Materials.Shape].SetInt("colourBands", edges[i].colourBands);
                        materials[(int) Materials.Shape].SetFloat("areaFromX", textureRectangle.xMin / position.width);
                        materials[(int) Materials.Shape].SetFloat("areaToX", textureRectangle.xMax / position.width);
                        materials[(int) Materials.Shape].SetFloat("areaFromY", (textureRectangle.yMin + (Screen.height - position.height)) / Screen.height);
                        materials[(int) Materials.Shape].SetFloat("areaToY", (textureRectangle.yMax + (Screen.height - position.height)) / Screen.height);
                        materials[(int) Materials.Shape].SetFloat("centreX", 0.5f);
                        materials[(int) Materials.Shape].SetFloat("centreY", 0.5f);
                        materials[(int) Materials.Shape].SetFloat("angle", (edges[i].angle + 90) * Mathf.Deg2Rad);
                        materials[(int) Materials.Shape].SetFloat("radialSize", 0.5f);
                        materials[(int) Materials.Shape].SetFloat("zoom", 1);
                        materials[(int) Materials.Shape].SetFloat("zoomCentreX", 0.5f);
                        materials[(int) Materials.Shape].SetFloat("zoomCentreY", 0.5f);
                        materials[(int) Materials.Shape].SetPass(0);
                        EditorGUI.DrawPreviewTexture(textureRectangle, Texture2D.whiteTexture, materials[(int) Materials.Shape]);
                    }
                }
            }
            top += (GUIWidth * 0.135f) + rowGapPixels + 6;
        }

        //Draw the properties for this layer.
        propertyPath += "[" + selectedLayer + "]";
        EditorGUI.BeginChangeCheck();
        beginProperty(getGUIPercentageRectangle(false, 2, 30, top), propertyPath + ".visible", new GUIContent("Visible", "Whether this layer is visible."));
        bool temporaryVisible = EditorGUI.Toggle(getGUIPercentageRectangle(false, 32.5f, 64.5f, top), edges[selectedLayer].visible);
        endProperty();
        if (EditorGUI.EndChangeCheck()) {
            beginUndo("Toggle Visibility");
            edges[selectedLayer].visible = temporaryVisible;
            endUndo(true);
        }
        top += rowPixels + rowGapPixels;
        if (edges[selectedLayer].visible) {
            bool setMeshDirty = false, setRenderTextureDirty = false;
            EditorGUI.BeginChangeCheck();
            beginProperty(getGUIPercentageRectangle(false, 2, 30, top), propertyPath + ".distance", new GUIContent("Distance",
                    "The distance from the edge of the shape to apply the effect."));
            float temporaryDistance = EditorGUI.Slider(getGUIPercentageRectangle(false, 32.5f, 64.5f, top), edges[selectedLayer].distance, 0.01f, 1);
            endProperty();
            bool endChangeCheck = EditorGUI.EndChangeCheck();
            if (endChangeCheck || contextClick) {
                setMeshDirty = true;
                if (endChangeCheck) {
                    beginUndo("Change Edge Distance");
                    edges[selectedLayer].distance = temporaryDistance;
                    endUndo(false);
                }
            }
            top += rowPixels + rowGapPixels;
            EditorGUI.BeginChangeCheck();
            beginProperty(getGUIPercentageRectangle(false, 2, 30, top), propertyPath + ".colourFrom", new GUIContent("From",
                    "The starting colour (at the edge of the shape)."));
            Color temporaryColourFrom = EditorGUI.ColorField(getGUIPercentageRectangle(false, 32.5f, 64.5f, top), edges[selectedLayer].colourFrom);
            endProperty();
            endChangeCheck = EditorGUI.EndChangeCheck();
            if (endChangeCheck || contextClick) {
                setRenderTextureDirty = true;
                if (endChangeCheck) {
                    beginUndo("Change From Colour");
                    edges[selectedLayer].colourFrom = temporaryColourFrom;
                    endUndo(false);
                }
            }
            top += rowPixels + rowGapPixels;
            EditorGUI.BeginChangeCheck();
            beginProperty(getGUIPercentageRectangle(false, 2, 30, top), propertyPath + ".colourTo", new GUIContent("To",
                    "The ending colour (furthest from the edge of the shape)."));
            Color temporaryColourTo = EditorGUI.ColorField(getGUIPercentageRectangle(false, 32.5f, 64.5f, top), edges[selectedLayer].colourTo);
            endProperty();
            endChangeCheck = EditorGUI.EndChangeCheck();
            if (endChangeCheck || contextClick) {
                setRenderTextureDirty = true;
                if (endChangeCheck) {
                    beginUndo("Change To Colour");
                    edges[selectedLayer].colourTo = temporaryColourTo;
                    endUndo(false);
                }
            }
            top += rowPixels + rowGapPixels;
            EditorGUI.BeginChangeCheck();
            beginProperty(getGUIPercentageRectangle(false, 2, 30, top), propertyPath + ".colourBias", new GUIContent("Bias",
                    "The bias between the two selected colours."));
            float temporaryColourBias = EditorGUI.Slider(getGUIPercentageRectangle(false, 32.5f, 64.5f, top), edges[selectedLayer].colourBias, 0, 1);
            endProperty();
            endChangeCheck = EditorGUI.EndChangeCheck();
            if (endChangeCheck || contextClick) {
                setRenderTextureDirty = true;
                if (endChangeCheck) {
                    beginUndo("Change Colour Bias");
                    edges[selectedLayer].colourBias = temporaryColourBias;
                    endUndo(false);
                }
            }
            top += rowPixels + rowGapPixels;
            EditorGUI.BeginChangeCheck();
            beginProperty(getGUIPercentageRectangle(false, 2, 30, top), propertyPath + ".colourBands", new GUIContent("Bands",
                    "The number of bands to split the colour into. The default is the maximum value of 255, which produces smooth colouring."));
            int temporaryColourBands = Math.Max(Math.Min(EditorGUI.IntField(getGUIPercentageRectangle(false, 32.5f, 64.5f, top),
                    edges[selectedLayer].colourBands), 255), 2);
            endProperty();
            endChangeCheck = EditorGUI.EndChangeCheck();
            if (endChangeCheck || contextClick) {
                setRenderTextureDirty = true;
                if (endChangeCheck) {
                    beginUndo("Change Colour Bands");
                    edges[selectedLayer].colourBands = temporaryColourBands;
                    endUndo(false);
                }
            }
            top += rowPixels + rowGapPixels;
            EditorGUI.BeginChangeCheck();
            beginProperty(getGUIPercentageRectangle(false, 2, 30, top), propertyPath + ".type", new GUIContent(isPillow ? "Pillow Type" : "Glow Type",
                    "Whether to display the effect at a specific angle, or all the way around the shape."));
            VectorSprites.EdgeShadingType temporaryType = (VectorSprites.EdgeShadingType) EditorGUI.IntPopup(
                    getGUIPercentageRectangle(false, 32.5f, 64.5f, top), (int) edges[selectedLayer].type,
                    enumeratedTypeNames[(int) EnumeratedTypeNameArrays.EdgeShadingType], enumeratedTypeValues[(int) EnumeratedTypeNameArrays.EdgeShadingType],
                    enumPopupStyle);
            endProperty();
            endChangeCheck = EditorGUI.EndChangeCheck();
            if (endChangeCheck || contextClick) {
                setMeshDirty = true;
                if (endChangeCheck) {
                    beginUndo("Change Type");
                    edges[selectedLayer].type = temporaryType;
                    endUndo(true);
                }
            }
            top += rowPixels + rowGapPixels;
            if (edges[selectedLayer].type == VectorSprites.EdgeShadingType.Angle) {
                EditorGUI.BeginChangeCheck();
                beginProperty(getGUIPercentageRectangle(false, 2, 30, top), propertyPath + ".angle", new GUIContent("Angle",
                        "The angle at which to display the effect."));
                float temporaryAngle = EditorGUI.Slider(getGUIPercentageRectangle(false, 32.5f, 64.5f, top), edges[selectedLayer].angle, 0, 360);
                endProperty();
                endChangeCheck = EditorGUI.EndChangeCheck();
                if (endChangeCheck || contextClick) {
                    setMeshDirty = true;
                    if (endChangeCheck) {
                        beginUndo("Change Angle");
                        edges[selectedLayer].angle = temporaryAngle;
                        endUndo(false);
                    }
                }
                top += rowPixels + rowGapPixels;
            }
            if (setMeshDirty) {
                if (isPillow) {
                    if (shape.pillowMeshes[selectedLayer] != null)
                        DestroyImmediate(shape.pillowMeshes[selectedLayer]);
                    shape.pillowMeshes[selectedLayer] = null;
                }
                else {
                    if (shape.glowMeshes[selectedLayer] != null)
                        DestroyImmediate(shape.glowMeshes[selectedLayer]);
                    shape.glowMeshes[selectedLayer] = null;
                }
            }
            if (setRenderTextureDirty) {
                if (isPillow) {
                    if (shape.pillowMeshesRenderTextures[selectedLayer] != null)
                        DestroyImmediate(shape.pillowMeshesRenderTextures[selectedLayer]);
                    shape.pillowMeshesRenderTextures[selectedLayer] = null;
                }
                else {
                    if (shape.glowMeshesRenderTextures[selectedLayer] != null)
                        DestroyImmediate(shape.glowMeshesRenderTextures[selectedLayer]);
                    shape.glowMeshesRenderTextures[selectedLayer] = null;
                }
            }
        }
    }

    //Returns whether a fill style uses the second colour.
    bool fillUsesSecondColour(VectorSprites.Fill fill) {
        return fill.style != VectorSprites.FillStyle.None && fill.style != VectorSprites.FillStyle.SolidColour && fill.style != VectorSprites.FillStyle.Noise;
    }

    //Draws a point in the specified colour and size using the point material.
    void drawPoint(Vector2 pointPosition, int size, Color colour, Rect grid) {
        materials[(int) Materials.Point].SetInt("size", size);
        materials[(int) Materials.Point].SetColor("colour", colour);
        materials[(int) Materials.Point].SetFloat("clampXMin", ((grid.xMin / Screen.width) * 2) - 1);
        materials[(int) Materials.Point].SetFloat("clampXMax", ((grid.xMax / Screen.width) * 2) - 1);
        materials[(int) Materials.Point].SetFloat("clampYMin", 1 - (((grid.yMax + (Screen.height - position.height)) / Screen.height) * 2));
        materials[(int) Materials.Point].SetFloat("clampYMax", 1 - (((grid.yMin + (Screen.height - position.height)) / Screen.height) * 2));
        Graphics.DrawTexture(new Rect(pointPosition.x - (size / 2), pointPosition.y - (size / 2), size, size), Texture2D.whiteTexture,
                materials[(int) Materials.Point]);
    }

    //(Un)zoom a point within the window with respect to the grid and the current Vector Sprites instance zoom properties.
    Vector2 zoomPoint(Vector2 pointPosition, Rect grid) {
        pointPosition -= grid.center;
        pointPosition.x /= grid.width;
        pointPosition.y /= grid.height;
        pointPosition.x = (pointPosition.x * vectorSpritesProperties.zoom) - (vectorSpritesProperties.zoomCentre.x * vectorSpritesProperties.zoom) +
                (vectorSpritesProperties.zoom / 2);
        pointPosition.y = (pointPosition.y * vectorSpritesProperties.zoom) - (vectorSpritesProperties.zoomCentre.y * vectorSpritesProperties.zoom) +
                (vectorSpritesProperties.zoom / 2);
        pointPosition.x *= grid.width;
        pointPosition.y *= grid.width;
        pointPosition += grid.center;
        return pointPosition;
    }
    Vector2 unzoomPoint(Vector2 pointPosition, Rect grid) {
        pointPosition -= grid.center;
        pointPosition.x /= grid.width;
        pointPosition.y /= grid.height;
        pointPosition.x = (pointPosition.x + (vectorSpritesProperties.zoomCentre.x * vectorSpritesProperties.zoom) - (vectorSpritesProperties.zoom / 2)) /
                vectorSpritesProperties.zoom;
        pointPosition.y = (pointPosition.y + (vectorSpritesProperties.zoomCentre.y * vectorSpritesProperties.zoom) - (vectorSpritesProperties.zoom / 2)) /
                vectorSpritesProperties.zoom;
        pointPosition.x *= grid.width;
        pointPosition.y *= grid.width;
        pointPosition += grid.center;
        return pointPosition;
    }

    //Draws a border using the border material.
    void drawBorder(Rect rectangle, Color colour) {
        materials[(int) Materials.Border].SetInt("boxWidth", (int) (rectangle.width + 0.01f));
        materials[(int) Materials.Border].SetInt("boxHeight", (int) (rectangle.height + 0.01f));
        materials[(int) Materials.Border].SetColor("colour", colour);
        Graphics.DrawTexture(rectangle, Texture2D.whiteTexture, materials[(int) Materials.Border]);
    }

    //Transforms a point whose co-ordinates are in the range 0..1 to be within specified bounds (the grid).
    Vector2 transformPointToGridBounds(Vector2 point, Rect bounds) {
        return new Vector2((point.x * bounds.width) + bounds.xMin, (point.y * bounds.height) + bounds.yMin);
    }

    //Returns the name of the next free shape name ("Shape" followed by the lowest possible integer >= 1).
    string getNextFreeShapeName() {
        int nextFreeShapeNumber = 1;
        while (true) {
            string shapeName = "Shape " + nextFreeShapeNumber.ToString();
            bool found = false;
            for (int i = 0; i < vectorSpritesProperties.shapeGroups.Count; i++)
                for (int j = 0; j < vectorSpritesProperties.shapeGroups[i].shapes.Count; j++)
                    if (vectorSpritesProperties.shapeGroups[i].shapes[j].name == shapeName) {
                        found = true;
                        break;
                    }
            if (found)
                nextFreeShapeNumber++;
            else
                return shapeName;
        }
    }

    //Returns the name of the next free shape group name ("Shape Group" followed by the lowest possible integer >= 1).
    string getNextFreeShapeGroupName() {
        int nextFreeShapeGroupNumber = 1;
        while (true) {
            string shapeName = "Shape Group " + nextFreeShapeGroupNumber.ToString();
            bool found = false;
            for (int i = 0; i < vectorSpritesProperties.shapeGroups.Count; i++)
                if (vectorSpritesProperties.shapeGroups[i].name == shapeName) {
                    found = true;
                    break;
                }
            if (found)
                nextFreeShapeGroupNumber++;
            else
                return shapeName;
        }
    }

    //Returns the name of the next free sprite ("Sprite" followed by the lowest possible integer >= 1).
    string getNextFreeSpriteName() {
        int nextFreeSpriteNumber = 1;
        while (true) {
            string spriteName = "Sprite " + nextFreeSpriteNumber.ToString();
            bool found = false;
            for (int i = 0; i < vectorSpritesProperties.vectorSprites.Count; i++)
                if (vectorSpritesProperties.vectorSprites[i].name == spriteName) {
                    found = true;
                    break;
                }
            if (found)
                nextFreeSpriteNumber++;
            else
                return spriteName;
        }
    }

    //Returns the name of the next free sprite sheet ("Sprite Sheet" followed by the lowest possible integer >= 1).
    string getNextFreeSpriteSheetName() {
        int nextFreeSpriteSheetNumber = 1;
        while (true) {
            string spriteSheetName = "Sprite Sheet " + nextFreeSpriteSheetNumber.ToString();
            bool found = false;
            for (int i = 0; i < vectorSpritesProperties.spriteSheets.Count; i++)
                if (vectorSpritesProperties.spriteSheets[i].name == spriteSheetName) {
                    found = true;
                    break;
                }
            if (found)
                nextFreeSpriteSheetNumber++;
            else
                return spriteSheetName;
        }
    }

    //Export one/all sprite(s).
    void exportSprite() {
        VectorSpritesExport vectorSpritesExport = GetWindow<VectorSpritesExport>();
        vectorSpritesExport.minSize = new Vector2(512, 512);
        vectorSpritesExport.title = "Vector Sprites - Export";
        vectorSpritesExport.initialise(vectorSpritesProperties, materials[(int) Materials.Shape], materials[(int) Materials.PillowAndGlowMesh],
                materials[(int) Materials.PillowAndGlowRenderTexture], vectorSpritesRenderer);
    }

    //Draw the grid.
    void drawGrid(Rect grid) {
        materials[(int) Materials.Grid].SetInt("gridWidth", (int) (grid.width + 0.01));
        materials[(int) Materials.Grid].SetInt("gridHeight", (int) (grid.height + 0.01));
        materials[(int) Materials.Grid].SetInt("gridDivisionsX", vectorSpritesProperties.showGrid ? vectorSpritesProperties.gridWidth : 0);
        materials[(int) Materials.Grid].SetInt("gridDivisionsY", vectorSpritesProperties.showGrid ? vectorSpritesProperties.gridHeight : 0);
        materials[(int) Materials.Grid].SetInt("drawGuides", vectorSpritesProperties.showGuidelines ? 1 : 0);
        if (vectorSpritesProperties.selectedEntities.Count == 1 && vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.Sprite &&
                vectorSpritesProperties.vectorSprites[vectorSpritesProperties.selectedEntities[0].primaryID].spriteRectangleTransform ==
                VectorSprites.SpriteRectangleTransform.Crop &&
                vectorSpritesProperties.vectorSprites[vectorSpritesProperties.selectedEntities[0].primaryID].width !=
                vectorSpritesProperties.vectorSprites[vectorSpritesProperties.selectedEntities[0].primaryID].height) {
            materials[(int) Materials.Grid].SetInt("drawSpriteCropLines", 1);
            materials[(int) Materials.Grid].SetFloat("spriteAspectRatio",
                    (float) vectorSpritesProperties.vectorSprites[vectorSpritesProperties.selectedEntities[0].primaryID].width /
                    vectorSpritesProperties.vectorSprites[vectorSpritesProperties.selectedEntities[0].primaryID].height);
        }
        else
            materials[(int) Materials.Grid].SetInt("drawSpriteCropLines", 0);
        materials[(int) Materials.Grid].SetInt("isProfessionalSkin", EditorGUIUtility.isProSkin ? 1 : 0);
        materials[(int) Materials.Grid].SetFloat("zoom", vectorSpritesProperties.zoom);
        materials[(int) Materials.Grid].SetFloat("zoomCentreX", vectorSpritesProperties.zoomCentre.x);
        materials[(int) Materials.Grid].SetFloat("zoomCentreY", 1 - vectorSpritesProperties.zoomCentre.y);
        Graphics.DrawTexture(grid, Texture2D.whiteTexture, materials[(int) Materials.Grid]);
    }

    //Snaps a point to a grid, using the grid width and height in the Vector Sprite properties.
    void snapToGrid(ref Vector2 point, bool applyZooming) {
        snapToGrid(ref point, new Rect(0, 0, 1, 1), applyZooming);
    }
    void snapToGrid(ref Vector2 point, Rect grid, bool applyZooming) {
        if (applyZooming)
            point = unzoomPoint(point, grid);
        point.x = ((Mathf.Floor(Mathf.Clamp(((point.x - grid.xMin) / grid.width) * vectorSpritesProperties.gridWidth, 0.75f,
                vectorSpritesProperties.gridWidth - 0.75f) + 0.5f) / vectorSpritesProperties.gridWidth) * grid.width) + grid.xMin;
        point.y = ((Mathf.Floor(Mathf.Clamp(((point.y - grid.yMin) / grid.height) * vectorSpritesProperties.gridHeight, 0.75f,
                vectorSpritesProperties.gridHeight - 0.75f) + 0.5f) / vectorSpritesProperties.gridHeight) * grid.height) + grid.yMin;
        if (applyZooming)
            point = zoomPoint(point, grid);
    }

    //Creates a shape, if the new shape is valid.
    void createShape() {
        if (newShapePoints.Count < 3)
            EditorUtility.DisplayDialog("New Shape", "Shapes must have at least three points.", "OK");
        else {

            //Add the last point of the shape (same as the first point so it joins up).
            newShapePoints.Add(newShapePoints[0]);

            //Detect if the shape is clockwise or anti-clockwise. If it is clockwise, reverse the points because the shape must be anti-clockwise for the
            //triangles to face the front.
            float clockwiseOrAntiClockwise = 0;
            for (int i = 0; i < newShapePoints.Count; i++)
                clockwiseOrAntiClockwise += (newShapePoints[(i + 1) % newShapePoints.Count].x - newShapePoints[i].x) *
                        (newShapePoints[(i + 1) % newShapePoints.Count].y + newShapePoints[i].y);
            if (clockwiseOrAntiClockwise < 0)
                newShapePoints.Reverse();

            //Create a new shape instance.
            VectorSprites.Shape shape = new VectorSprites.Shape(newShapeName);
            vectorSpritesProperties.selectedEntity = VectorSprites.SelectableEntity.Shape;
            vectorSpritesProperties.selectedEntities[0].secondaryID =
                    vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[0].primaryID].shapes.Count;
            beginUndo("Create Shape");
            for (int i = 1; i < newShapePoints.Count; i++)
                shape.shapePoints.Add(new VectorSprites.ShapePoint(newShapePoints[i - 1], newShapePoints[i]));
            vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[0].primaryID].expanded = true;
            vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[0].primaryID].shapes.Add(shape);
            endUndo(true);
        }
    }

    //Called when an undo/redo is performed. Set all meshes as dirty so they can be recreated.
    void undoRedoPerformed() {

        //Flag all meshes as dirty after an undo/redo. Don't know what has changed so have to be on the safe side.
        for (int i = 0; i < vectorSpritesProperties.shapeGroups.Count; i++)
            for (int j = 0; j < vectorSpritesProperties.shapeGroups[i].shapes.Count; j++)
                vectorSpritesProperties.shapeGroups[i].shapes[j].resetAllMeshes();

        //Ensure the selection is still valid after an undo, otherwise change it.
        for (int i = 0; i < vectorSpritesProperties.selectedEntities.Count; i++) {
            bool clearSelection = false;
            if (vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.ShapeGroup &&
                    vectorSpritesProperties.selectedEntities[i].primaryID >= vectorSpritesProperties.shapeGroups.Count)
                clearSelection = true;
            else if (vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.Sprite &&
                    vectorSpritesProperties.selectedEntities[i].primaryID >= vectorSpritesProperties.vectorSprites.Count)
                clearSelection = true;
            else if (vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.Shape) {
                if (vectorSpritesProperties.selectedEntities[i].primaryID >= vectorSpritesProperties.shapeGroups.Count)
                    clearSelection = true;
                else if (vectorSpritesProperties.selectedEntities[i].secondaryID >=
                        vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[i].primaryID].shapes.Count)
                    clearSelection = true;
            }
            if (clearSelection) {
                vectorSpritesProperties.selectedEntities.Clear();
                break;
            }
        }
    }

    //Begin/end an undo object, grouping events with the same name.
    public void beginUndo(string eventName) {
        Undo.RecordObject(gameObject, eventName);
        if (eventName != lastUndoEvent) {
            undoGroup = Undo.GetCurrentGroup();
            lastUndoEvent = eventName;
        }
    }
    public void endUndo(bool forceNewGroup) {
        EditorUtility.SetDirty(gameObject);
        if (forceNewGroup)
            lastUndoEvent = "";
        else
            Undo.CollapseUndoOperations(undoGroup);
    }

    //Begin/end a property (for compatibility with prefabs).
    public void beginProperty(Rect rectangle, string propertyPath, GUIContent label) {
        SerializedProperty property = serializedObject.FindProperty("vectorSpritesProperties." + propertyPath);
        EditorGUI.LabelField(rectangle, property == null ? label : EditorGUI.BeginProperty(rectangle, label, property));
        propertyBegun = property != null;
    }
    public void endProperty() {
        if (propertyBegun)
            EditorGUI.EndProperty();
    }

    //Translate the shape or all shapes within a shape group.
    public void translateShape(Vector2 translation, bool registerUndo, bool cancelIfOutOfBounds) {
        bool isShape = vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.Shape;
        bool singleShape = isShape && vectorSpritesProperties.selectedEntities.Count == 1;
        if (cancelIfOutOfBounds)
            for (int k = 0; k < vectorSpritesProperties.selectedEntities.Count; k++) {
                for (int j = 0; j < (isShape ? 1 : vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[k].primaryID].shapes.Count);
                        j++) {
                    VectorSprites.Shape shape = isShape ? vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[k].primaryID].shapes[
                            vectorSpritesProperties.selectedEntities[k].secondaryID] :
                            vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[k].primaryID].shapes[j];
                    for (int i = 0; i < shape.shapePoints.Count; i++)
                        if (shape.shapePoints[i].endPoint.x + translation.x <= 0 || shape.shapePoints[i].endPoint.x + translation.x >= 1 ||
                                shape.shapePoints[i].endPoint.y + translation.y <= 0 || shape.shapePoints[i].endPoint.y + translation.y >= 1 ||
                                shape.shapePoints[i].startTangent.x + translation.x <= 0 || shape.shapePoints[i].startTangent.x + translation.x >= 1 ||
                                shape.shapePoints[i].startTangent.y + translation.y <= 0 || shape.shapePoints[i].startTangent.y + translation.y >= 1 ||
                                shape.shapePoints[i].endTangent.x + translation.x <= 0 || shape.shapePoints[i].endTangent.x + translation.x >= 1 ||
                                shape.shapePoints[i].endTangent.y + translation.y <= 0 || shape.shapePoints[i].endTangent.y + translation.y >= 1) {
                            EditorUtility.DisplayDialog("Out of Bounds", "The shape" + (singleShape ? "" : "s") + " cannot be translated because the " +
                                    "translation you have chosen will move at least one shape point or tangent out of the bounds of the sprite window.", "OK");
                            return;
                        }
                }
            }
        if (registerUndo)
            beginUndo("Translate Shape" + (singleShape ? "" : "s"));
        for (int k = 0; k < vectorSpritesProperties.selectedEntities.Count; k++) {
            for (int j = 0; j < (isShape ? 1 : vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[k].primaryID].shapes.Count); j++) {
                VectorSprites.Shape shape = isShape ? vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[k].primaryID].shapes[
                        vectorSpritesProperties.selectedEntities[k].secondaryID] :
                        vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[k].primaryID].shapes[j];
                for (int i = 0; i < shape.shapePoints.Count; i++) {
                    shape.shapePoints[i].endPoint += translation;
                    shape.shapePoints[i].startTangent += translation;
                    shape.shapePoints[i].endTangent += translation;
                }
                shape.resetAllMeshes();
            }
        }
        if (registerUndo)
            endUndo(true);
    }

    //Rotate the shape, or all shapes within a shape group.
    public void rotateShape(float rotation, bool originIsShapeCentre, bool registerUndo, bool cancelIfOutOfBounds, Vector2? centrePoint) {
        bool isShape = vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.Shape;
        bool singleShape = isShape && vectorSpritesProperties.selectedEntities.Count == 1;
        if (centrePoint == null)
            centrePoint = getCentrePoint(originIsShapeCentre);
        float sineAngle = Mathf.Sin(rotation * Mathf.Deg2Rad);
        float cosineAngle = Mathf.Cos(rotation * Mathf.Deg2Rad);
        if (cancelIfOutOfBounds)
            for (int l = 0; l < vectorSpritesProperties.selectedEntities.Count; l++)
                for (int k = 0; k < (isShape ? 1 : vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[l].primaryID].shapes.Count);
                        k++) {
                    VectorSprites.Shape shape = isShape ? vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[l].primaryID].shapes[
                            vectorSpritesProperties.selectedEntities[l].secondaryID] :
                            vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[l].primaryID].shapes[k];
                    for (int i = 0; i < shape.shapePoints.Count; i++)
                        for (int j = 0; j < 3; j++) {
                            Vector2 rotatedPoint = (j == 0 ? shape.shapePoints[i].endPoint : (j == 1 ? shape.shapePoints[i].startTangent :
                                    shape.shapePoints[i].endTangent)) - (Vector2) centrePoint;
                            rotatedPoint = new Vector2((rotatedPoint.x * cosineAngle) - (rotatedPoint.y * sineAngle),
                                    (rotatedPoint.x * sineAngle) + (rotatedPoint.y * cosineAngle)) + (Vector2) centrePoint;
                            if (rotatedPoint.x <= 0 || rotatedPoint.x >= 1 || rotatedPoint.y <= 0 || rotatedPoint.y >= 1) {
                                EditorUtility.DisplayDialog("Out of Bounds", "The shape" + (singleShape ? "" : "s") + " cannot be rotated because the " +
                                        "rotation angle you have chosen will move at least one shape point or tangent out of the bounds of the sprite window.",
                                        "OK");
                                return;
                            }
                        }
                }
        if (registerUndo)
            beginUndo("Rotate Shape" + (singleShape ? "" : "s"));
        for (int l = 0; l < vectorSpritesProperties.selectedEntities.Count; l++)
            for (int k = 0; k < (isShape ? 1 : vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[l].primaryID].shapes.Count);
                    k++) {
                VectorSprites.Shape shape = isShape ? vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[l].primaryID].shapes[
                        vectorSpritesProperties.selectedEntities[l].secondaryID] :
                        vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[l].primaryID].shapes[k];
                for (int i = 0; i < shape.shapePoints.Count; i++)
                    for (int j = 0; j < 3; j++) {
                        Vector2 rotatedPoint = (j == 0 ? shape.shapePoints[i].endPoint : (j == 1 ? shape.shapePoints[i].startTangent :
                                shape.shapePoints[i].endTangent)) - (Vector2) centrePoint;
                        rotatedPoint = new Vector2((rotatedPoint.x * cosineAngle) - (rotatedPoint.y * sineAngle),
                                (rotatedPoint.x * sineAngle) + (rotatedPoint.y * cosineAngle)) + (Vector2) centrePoint;
                        if (j == 0)
                            shape.shapePoints[i].endPoint = rotatedPoint;
                        else if (j == 1)
                            shape.shapePoints[i].startTangent = rotatedPoint;
                        else
                            shape.shapePoints[i].endTangent = rotatedPoint;
                    }
                shape.resetAllMeshes();
            }
        if (registerUndo)
            endUndo(true);
    }

    //Scale the shape, or all shapes within a shape group.
    public void scaleShape(Vector2 scaleFactor, bool originIsShapeCentre, bool registerUndo, bool cancelIfOutOfBounds) {
        bool isShape = vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.Shape;
        bool singleShape = isShape && vectorSpritesProperties.selectedEntities.Count == 1;
        Vector2 centrePoint = getCentrePoint(originIsShapeCentre);
        if (cancelIfOutOfBounds)
            for (int l = 0; l < vectorSpritesProperties.selectedEntities.Count; l++)
                for (int k = 0; k < (isShape ? 1 : vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[l].primaryID].shapes.Count);
                        k++) {
                    VectorSprites.Shape shape = isShape ? vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[l].primaryID].shapes[
                            vectorSpritesProperties.selectedEntities[l].secondaryID] :
                            vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[l].primaryID].shapes[k];
                    for (int i = 0; i < shape.shapePoints.Count; i++)
                        for (int j = 0; j < 3; j++) {
                            Vector2 scaledPoint = (j == 0 ? shape.shapePoints[i].endPoint : (j == 1 ? shape.shapePoints[i].startTangent :
                                    shape.shapePoints[i].endTangent)) - centrePoint;
                            scaledPoint = new Vector2(scaledPoint.x * scaleFactor.x, scaledPoint.y * scaleFactor.y) + centrePoint;
                            if (scaledPoint.x <= 0 || scaledPoint.x >= 1 || scaledPoint.y <= 0 || scaledPoint.y >= 1) {
                                EditorUtility.DisplayDialog("Out of Bounds", "The shape" + (singleShape ? "" : "s") + " cannot be scaled because the scale " +
                                        "factor you have chosen will move at least one shape point or tangent out of the bounds of the sprite window.", "OK");
                                return;
                            }
                        }
                }
        if (registerUndo)
            beginUndo("Scale Shape" + (singleShape ? "" : "s"));
        for (int l = 0; l < vectorSpritesProperties.selectedEntities.Count; l++)
            for (int k = 0; k < (isShape ? 1 : vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[l].primaryID].shapes.Count);
                    k++) {
                VectorSprites.Shape shape = isShape ? vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[l].primaryID].shapes[
                        vectorSpritesProperties.selectedEntities[l].secondaryID] :
                        vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[l].primaryID].shapes[k];
                for (int i = 0; i < shape.shapePoints.Count; i++)
                    for (int j = 0; j < 3; j++) {
                        Vector2 scaledPoint = (j == 0 ? shape.shapePoints[i].endPoint : (j == 1 ? shape.shapePoints[i].startTangent :
                                shape.shapePoints[i].endTangent)) - centrePoint;
                        scaledPoint = new Vector2(scaledPoint.x * scaleFactor.x, scaledPoint.y * scaleFactor.y) + centrePoint;
                        if (j == 0)
                            shape.shapePoints[i].endPoint = scaledPoint;
                        else if (j == 1)
                            shape.shapePoints[i].startTangent = scaledPoint;
                        else
                            shape.shapePoints[i].endTangent = scaledPoint;
                    }
            }
        for (int k = 0; k < vectorSpritesProperties.selectedEntities.Count; k++)
            for (int j = 0; j < (isShape ? 1 : vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[k].primaryID].shapes.Count); j++) {
                VectorSprites.Shape shape = isShape ? vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[k].primaryID].shapes[
                        vectorSpritesProperties.selectedEntities[k].secondaryID] :
                        vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[k].primaryID].shapes[j];
                float clockwiseOrAntiClockwise = 0;
                for (int i = 0; i < shape.shapePoints.Count; i++)
                    clockwiseOrAntiClockwise += (shape.shapePoints[(i + 1) % shape.shapePoints.Count].endPoint.x - shape.shapePoints[i].endPoint.x) *
                            (shape.shapePoints[(i + 1) % shape.shapePoints.Count].endPoint.y + shape.shapePoints[i].endPoint.y);
                if (clockwiseOrAntiClockwise < 0) {
                    List<VectorSprites.ShapePoint> reversedShapePoints = new List<VectorSprites.ShapePoint>();
                    for (int i = 0; i < shape.shapePoints.Count; i++) {
                        VectorSprites.ShapePoint shapePoint = new VectorSprites.ShapePoint(Vector2.zero, shape.shapePoints[shape.shapePoints.Count - i - 1].
                                endPoint);
                        shapePoint.startTangent = shape.shapePoints[(shape.shapePoints.Count - i) % shape.shapePoints.Count].endTangent;
                        shapePoint.endTangent = shape.shapePoints[(shape.shapePoints.Count - i) % shape.shapePoints.Count].startTangent;
                        reversedShapePoints.Add(shapePoint);
                    }
                    shape.shapePoints = reversedShapePoints;
                }
                shape.resetAllMeshes();
            }
        if (registerUndo)
            endUndo(true);
    }

    //Returns the centre point to rotate/scale around depending on the selected origin.
    Vector2 getCentrePoint(bool originIsShapeCentre) {
        if (originIsShapeCentre) {
            bool isShape = vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.Shape;
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue), max = new Vector2(float.MinValue, float.MinValue);
            for (int k = 0; k < vectorSpritesProperties.selectedEntities.Count; k++)
                for (int j = 0; j < (isShape ? 1 : vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[k].primaryID].shapes.Count);
                        j++) {
                    VectorSprites.Shape shape = isShape ? vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[k].primaryID].shapes[
                            vectorSpritesProperties.selectedEntities[k].secondaryID] :
                            vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[k].primaryID].shapes[j];
                    for (int i = 0; i < shape.shapePoints.Count; i++) {
                        if (shape.shapePoints[i].endPoint.x > max.x)
                            max.x = shape.shapePoints[i].endPoint.x;
                        if (shape.shapePoints[i].endPoint.y > max.y)
                            max.y = shape.shapePoints[i].endPoint.y;
                        if (shape.shapePoints[i].endPoint.x < min.x)
                            min.x = shape.shapePoints[i].endPoint.x;
                        if (shape.shapePoints[i].endPoint.y < min.y)
                            min.y = shape.shapePoints[i].endPoint.y;
                    }
                }
            return (min + max) / 2;
        }
        else
            return new Vector2(0.5f, 0.5f);
    }

    //Delete a shape point.
    void deletePoint(object pointIndex) {

        //Only allow point deletion if a single shape is selected.
        if (vectorSpritesProperties.selectedEntity != VectorSprites.SelectableEntity.Shape || vectorSpritesProperties.selectedEntities.Count != 1)
            return;

        //Ensure that the shape has more than three points - otherwise the point cannot be deleted.
        VectorSprites.Shape shape = vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[0].primaryID].shapes[
                vectorSpritesProperties.selectedEntities[0].secondaryID];
        if (shape.shapePoints.Count <= 3) {
            EditorUtility.DisplayDialog("Delete Shape Point", "This shape point cannot be deleted because shapes must have a minimum of 3 points.", "OK");
            return;
        }

        //Delete the point, set the average position of the tangents between this point and the next one and flag the entire shape as dirty so it is redrawn.
        beginUndo("Delete Shape Point");
        int thisPoint = (int) pointIndex;
        shape.shapePoints.RemoveAt(thisPoint);
        int nextPoint = thisPoint % shape.shapePoints.Count, previousPoint = (nextPoint + shape.shapePoints.Count - 1) % shape.shapePoints.Count;
        shape.shapePoints[nextPoint].startTangent = (shape.shapePoints[previousPoint].endPoint * 0.6667f) + (shape.shapePoints[nextPoint].endPoint * 0.3333f);
        shape.shapePoints[nextPoint].endTangent = (shape.shapePoints[previousPoint].endPoint * 0.3333f) + (shape.shapePoints[nextPoint].endPoint * 0.6667f);
        shape.resetAllMeshes();
        endUndo(true);
    }

    //Insert a shape point after a given point.
    void insertShapePointAfter(object pointIndex) {

        //Only allow point insertion if a single shape is selected.
        if (vectorSpritesProperties.selectedEntity != VectorSprites.SelectableEntity.Shape || vectorSpritesProperties.selectedEntities.Count != 1)
            return;

        //Ensure that the shape has less than the maximum number of points - otherwise another point cannot be added.
        VectorSprites.Shape shape = vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[0].primaryID].shapes[
                vectorSpritesProperties.selectedEntities[0].secondaryID];
        if (shape.shapePoints.Count >= maxPointsPerShape) {
            EditorUtility.DisplayDialog("Insert Shape Point", "A shape point cannot be inserted after this point because this shape already has the maximum " +
                    "number of points (" + maxPointsPerShape + ").", "OK");
            return;
        }

        //Insert the point, sort out the tangents and flag the entire shape as dirty so it is redrawn.
        beginUndo("Insert Shape Point");
        int thisPoint = ((int) pointIndex + 1) % shape.shapePoints.Count, previousPoint = (int) pointIndex;
        VectorSprites.ShapePoint newShapePoint = new VectorSprites.ShapePoint(shape.shapePoints[previousPoint].endPoint,
                (shape.shapePoints[previousPoint].endPoint + shape.shapePoints[thisPoint].endPoint) / 2);
        shape.shapePoints.Insert(thisPoint, newShapePoint);
        thisPoint++;
        shape.shapePoints[thisPoint].startTangent = ((shape.shapePoints[thisPoint].startTangent - shape.shapePoints[thisPoint].endPoint) * 0.5f) +
                shape.shapePoints[thisPoint].endPoint;
        shape.shapePoints[thisPoint].endTangent = ((shape.shapePoints[thisPoint].endTangent - shape.shapePoints[thisPoint].endPoint) * 0.5f) +
                shape.shapePoints[thisPoint].endPoint;
        shape.resetAllMeshes();
        endUndo(true);
    }

    //Insert a shape point before a given point.
    void insertShapePointBefore(object pointIndex) {

        //Only allow point insertion if a single shape is selected.
        if (vectorSpritesProperties.selectedEntity != VectorSprites.SelectableEntity.Shape || vectorSpritesProperties.selectedEntities.Count != 1)
            return;

        //Ensure that the shape has less than the maximum number of points - otherwise another point cannot be added.
        VectorSprites.Shape shape = vectorSpritesProperties.shapeGroups[vectorSpritesProperties.selectedEntities[0].primaryID].shapes[
                vectorSpritesProperties.selectedEntities[0].secondaryID];
        if (shape.shapePoints.Count >= maxPointsPerShape) {
            EditorUtility.DisplayDialog("Insert Shape Point", "A shape point cannot be inserted before this point because this shape already has the maximum " +
                    "number of points (" + maxPointsPerShape + ").", "OK");
            return;
        }

        //Insert the point, sort out the tangents and flag the entire shape as dirty so it is redrawn.
        beginUndo("Insert Shape Point");
        int thisPoint = (int) pointIndex, previousPoint = ((int) pointIndex + shape.shapePoints.Count - 1) % shape.shapePoints.Count;
        VectorSprites.ShapePoint newShapePoint = new VectorSprites.ShapePoint(shape.shapePoints[previousPoint].endPoint,
                (shape.shapePoints[previousPoint].endPoint + shape.shapePoints[thisPoint].endPoint) / 2);
        newShapePoint.startTangent = ((shape.shapePoints[thisPoint].startTangent - shape.shapePoints[previousPoint].endPoint) * 0.5f) +
                shape.shapePoints[previousPoint].endPoint;
        newShapePoint.endTangent = ((shape.shapePoints[thisPoint].endTangent - shape.shapePoints[previousPoint].endPoint) * 0.5f) +
                shape.shapePoints[previousPoint].endPoint;
        shape.shapePoints.Insert(thisPoint, newShapePoint);
        thisPoint++;
        shape.shapePoints[thisPoint].startTangent = (newShapePoint.endPoint * 0.6667f) + (shape.shapePoints[thisPoint].endPoint * 0.3333f);
        shape.shapePoints[thisPoint].endTangent = (newShapePoint.endPoint * 0.3333f) + (shape.shapePoints[thisPoint].endPoint * 0.6667f);
        shape.resetAllMeshes();
        endUndo(true);
    }

    //Draw an icon.
    void drawIcon(Icons icon, Rect rectangle, int pixelYFrom, int pixelYTo) {
        if (pixelYFrom < rectangle.xMax && pixelYTo >= 0) {
            materials[(int) Materials.Icons].SetInt("iconIndex", (int) icon);
            materials[(int) Materials.Icons].SetInt("pixelYFrom", pixelYFrom);
            materials[(int) Materials.Icons].SetInt("pixelYTo", pixelYTo);
            materials[(int) Materials.Icons].SetPass(0);
            EditorGUI.DrawPreviewTexture(rectangle, Texture2D.whiteTexture, materials[(int) Materials.Icons]);
        }
    }

    //Draws a line using "Handles.DrawLine()", but keeps it within grid bounds.
    void drawLine(Vector2 startPoint, Vector2 endPoint, Rect grid) {

        //Flag whether the grid contains the start/end points.
        bool gridContainsStartPoint = grid.Contains(startPoint), gridContainsEndPoint = grid.Contains(endPoint);

        //If the grid contains neither the start nor end points, don't draw anything.
        if (!gridContainsStartPoint && !gridContainsEndPoint)
            return;

        //If one of the points is not inside the grid, adjust one of the points to shorten the line so it goes up to the edge of the grid and not beyond.
        if (!gridContainsStartPoint || !gridContainsEndPoint) {
            if (!gridContainsStartPoint) {
                Vector2 tempPoint = startPoint;
                startPoint = endPoint;
                endPoint = tempPoint;
            }
            float minPercentage = 0, maxPercentage = 1, testPercentage = 0;
            while (maxPercentage - minPercentage > 0.0001f) {
                testPercentage = (minPercentage + maxPercentage) / 2;
                Vector2 testPoint = (startPoint * (1 - testPercentage)) + (endPoint * testPercentage);
                if (grid.Contains(testPoint))
                    minPercentage = testPercentage;
                else
                    maxPercentage = testPercentage;
            }
            endPoint = (startPoint * (1 - testPercentage)) + (endPoint * testPercentage);
        }

        //Draw the line.
        Handles.DrawLine(startPoint, endPoint);
    }
}