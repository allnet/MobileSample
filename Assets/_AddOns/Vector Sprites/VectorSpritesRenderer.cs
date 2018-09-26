using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable, ExecuteInEditMode]
public class VectorSpritesRenderer : MonoBehaviour {

    //Constants.
    static readonly int[] bezierSubdivisions = { 8, 16, 24, 32, 64 };

    //Enumerated Types
    public enum OutlineMeshType { Glow, Outline, Pillow };

    //Classes.
    [Serializable]
    public class CreatedFromVectorSpritesInstance {
        public bool flag = false;
    }

    //Variables.
    VectorSprites.VectorSpritesProperties vectorSpritesProperties;
    Material shapeMaterial, pillowAndGlowMeshMaterial, pillowAndGlowRenderTextureMaterial;
    Camera renderCamera;
    Mesh pillowOrGlowMeshToRender = null, pillowOrGlowAssociatedFillMesh = null;
    Color pillowOrGlowMeshColour1, pillowOrGlowMeshColour2;
    float pillowOrGlowMeshColourBias;
    int pillowOrGlowMeshColourBands;
    int pillowOrGlowMeshStencilTestValue;
    Vector2 meshScale;
    Mesh quad = null;
    bool editor;
    public CreatedFromVectorSpritesInstance createdFromVectorSpritesInstance = new CreatedFromVectorSpritesInstance();

    //Creates a temporary camera for rendering the scene onto a texture. Initialises the camera and forces a render with everything culled (so nothing is
    //rendered), to allow mesh creation and drawing to occur in "OnPostRender()".
    public void render(VectorSprites.VectorSpritesProperties vectorSpritesProperties, Material shapeMaterial, Material pillowAndGlowMeshMaterial,
            Material pillowAndGlowRenderTextureMaterial, RenderTexture renderTexture, Vector2 meshScale, bool editor) {

        //Set properties.
        this.vectorSpritesProperties = vectorSpritesProperties;
        this.shapeMaterial = shapeMaterial;
        this.pillowAndGlowMeshMaterial = pillowAndGlowMeshMaterial;
        this.pillowAndGlowRenderTextureMaterial = pillowAndGlowRenderTextureMaterial;
        this.meshScale = meshScale;
        this.editor = editor;

        //Get the quality from the vector sprite properties.
        VectorSprites.Quality quality = editor ? vectorSpritesProperties.editorQuality : vectorSpritesProperties.gameQuality;

        //Set the transform position and initialise the camera if it hasn't already been set up.
        transform.position = new Vector3(0, 0, -1);
        renderCamera = gameObject.GetComponent<Camera>();
        if (renderCamera == null) {
            renderCamera = gameObject.AddComponent<Camera>();
            renderCamera.projectionMatrix = Matrix4x4.Ortho(-1, 1, 1, -1, 1, 32768);
            renderCamera.cullingMask = 0;
            renderCamera.enabled = false;
            renderCamera.clearFlags = CameraClearFlags.SolidColor;
            renderCamera.backgroundColor = new Color(0, 0, 0, 0);
        }

        //Create the quad for drawing render textures if it hasn't already been created.
        if (quad == null) {
            quad = new Mesh();
            quad.hideFlags = HideFlags.HideAndDontSave;
            quad.vertices = new Vector3[] { new Vector3(-1, 1, 0), new Vector3(-1, -1, 0), new Vector3(1, 1, 0), new Vector3(1, -1, 0) };
            quad.triangles = new int[] { 0, 1, 2, 1, 3, 2 };
            quad.uv = new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0), new Vector2(1, 1) };
            quad.RecalculateNormals();
            ;
        }

        //Create any required meshes.
        List<int> shapeGroupsAlreadyProcessed = new List<int>();
        List<int> shapesAlreadyProcessed = new List<int>();
        for (int l = 0; l < vectorSpritesProperties.selectedEntities.Count; l++) {
            if (vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.Shape) {
                if (shapeGroupsAlreadyProcessed.Contains(vectorSpritesProperties.selectedEntities[l].primaryID))
                    continue;
                else
                    shapeGroupsAlreadyProcessed.Add(vectorSpritesProperties.selectedEntities[l].primaryID);
            }
            for (int i = vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.Sprite ? 0 :
                    vectorSpritesProperties.selectedEntities[l].primaryID;
                    i < (vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.Sprite ? vectorSpritesProperties.shapeGroups.Count :
                    vectorSpritesProperties.selectedEntities[l].primaryID + 1); i++)
                for (int j = 0; j < vectorSpritesProperties.shapeGroups[i].shapes.Count; j++) {
                    if (vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.Sprite) {
                        if (!shapeAssociatedWithSelectedSprite(l, i, j))
                            continue;
                        bool alreadyProcessed = false;
                        for (int m = 0; m < shapeGroupsAlreadyProcessed.Count; m++)
                            if (shapeGroupsAlreadyProcessed[m] == i && shapesAlreadyProcessed[m] == j) {
                                alreadyProcessed = true;
                                break;
                            }
                        if (alreadyProcessed)
                            continue;
                        else {
                            shapeGroupsAlreadyProcessed.Add(i);
                            shapesAlreadyProcessed.Add(j);
                        }
                    }
                    VectorSprites.Shape shape = vectorSpritesProperties.shapeGroups[i].shapes[j];
                    if (shape.shapeMesh == null && (shape.hasFillOrShadow() || shape.hasPillow() || shape.hasGlow()))
                        shape.shapeMesh = getFillMesh(shape, out shape.shapeMeshMinimumCoordinates, out shape.shapeMeshMaximumCoordinates, quality);
                    if (shape.outlineMesh == null && shape.hasOutline())
                        shape.outlineMesh = getOutlineMesh(shape, OutlineMeshType.Outline, -1, out shape.outlineMeshMinimumCoordinates,
                                out shape.outlineMeshMaximumCoordinates, quality);
                    Vector2 dummy;
                    if (shape.pillowMeshes == null)
                        shape.pillowMeshes = new Mesh[2];
                    if (shape.pillowMeshesRenderTextures == null)
                        shape.pillowMeshesRenderTextures = new RenderTexture[2];
                    for (int k = 0; k < shape.pillowMeshes.Length; k++)
                        if (shape.pillows[k].visible) {
                            bool renderTextureDirty = false;
                            if (shape.pillowMeshesRenderTextures[k] == null || shape.pillowMeshesRenderTextures[k].width != renderTexture.width ||
                                    shape.pillowMeshesRenderTextures[k].height != renderTexture.height) {
                                if (shape.pillowMeshesRenderTextures[k] != null)
                                    DestroyImmediate(shape.pillowMeshesRenderTextures[k]);
                                shape.pillowMeshesRenderTextures[k] = new RenderTexture(renderTexture.width * (editor ? 1 : 2),
                                        renderTexture.height * (editor ? 1 : 2), 24);
                                shape.pillowMeshesRenderTextures[k].hideFlags = HideFlags.HideAndDontSave;
                                renderTextureDirty = true;
                            }
                            if (shape.pillowMeshes[k] == null) {
                                shape.pillowMeshes[k] = getOutlineMesh(shape, OutlineMeshType.Pillow, k, out dummy, out dummy, quality);
                                renderTextureDirty = true;
                            }
                            if (renderTextureDirty) {
                                pillowOrGlowMeshToRender = shape.pillowMeshes[k];
                                pillowOrGlowAssociatedFillMesh = shape.shapeMesh;
                                pillowOrGlowMeshColour1 = shape.pillows[k].colourFrom;
                                pillowOrGlowMeshColour2 = shape.pillows[k].colourTo;
                                pillowOrGlowMeshColourBias = shape.pillows[k].colourBias;
                                pillowOrGlowMeshColourBands = shape.pillows[k].colourBands;
                                pillowOrGlowMeshStencilTestValue = 1;
                                renderCamera.targetTexture = shape.pillowMeshesRenderTextures[k];
                                renderCamera.Render();
                            }
                        }
                    if (shape.glowMeshes == null)
                        shape.glowMeshes = new Mesh[1];
                    if (shape.glowMeshesRenderTextures == null)
                        shape.glowMeshesRenderTextures = new RenderTexture[1];
                    for (int k = 0; k < shape.glowMeshes.Length; k++)
                        if (shape.glows[k].visible) {
                            bool renderTextureDirty = false;
                            if (shape.glowMeshesRenderTextures[k] == null || shape.glowMeshesRenderTextures[k].width != renderTexture.width ||
                                    shape.glowMeshesRenderTextures[k].height != renderTexture.height) {
                                if (shape.glowMeshesRenderTextures[k] != null)
                                    DestroyImmediate(shape.glowMeshesRenderTextures[k]);
                                shape.glowMeshesRenderTextures[k] = new RenderTexture(renderTexture.width * (editor ? 1 : 2),
                                        renderTexture.height * (editor ? 1 : 2), 24);
                                shape.glowMeshesRenderTextures[k].hideFlags = HideFlags.HideAndDontSave;
                                renderTextureDirty = true;
                            }
                            if (shape.glowMeshes[k] == null) {
                                shape.glowMeshes[k] = getOutlineMesh(shape, OutlineMeshType.Glow, k, out dummy, out dummy, quality);
                                renderTextureDirty = true;
                            }
                            if (renderTextureDirty) {
                                pillowOrGlowMeshToRender = shape.glowMeshes[k];
                                pillowOrGlowAssociatedFillMesh = shape.shapeMesh;
                                pillowOrGlowMeshColour1 = shape.glows[k].colourFrom;
                                pillowOrGlowMeshColour2 = shape.glows[k].colourTo;
                                pillowOrGlowMeshColourBias = shape.glows[k].colourBias;
                                pillowOrGlowMeshColourBands = shape.glows[k].colourBands;
                                pillowOrGlowMeshStencilTestValue = 0;
                                renderCamera.targetTexture = shape.glowMeshesRenderTextures[k];
                                renderCamera.Render();
                            }
                        }
                }
        }

        //Render.
        pillowOrGlowMeshToRender = null;
        renderCamera.targetTexture = renderTexture;
        renderCamera.Render();
    }

    //On Post Render.
    void OnPostRender() {

        //If there is a specific pillow/glow mesh to render, do so using the pillow/glow material. These meshes are rendered to a separate render texture, which
        //is itself drawn on the main render texture when the main render is executed.
        if (pillowOrGlowMeshToRender != null) {
            pillowAndGlowMeshMaterial.SetColor("colour1", pillowOrGlowMeshColour1);
            pillowAndGlowMeshMaterial.SetColor("colour2", pillowOrGlowMeshColour2);
            pillowAndGlowMeshMaterial.SetFloat("colourBias", pillowOrGlowMeshColourBias);
            pillowAndGlowMeshMaterial.SetInt("colourBands", pillowOrGlowMeshColourBands);
            pillowAndGlowMeshMaterial.SetInt("stencilTestValue", pillowOrGlowMeshStencilTestValue);
            pillowAndGlowMeshMaterial.SetPass(0);
            Graphics.DrawMeshNow(pillowOrGlowAssociatedFillMesh, Matrix4x4.TRS(Vector3.zero, Quaternion.identity, meshScale / (editor ? 1 : 2)));
            pillowAndGlowMeshMaterial.SetPass(1);
            Graphics.DrawMeshNow(pillowOrGlowMeshToRender, Matrix4x4.TRS(Vector3.zero, Quaternion.identity, meshScale / (editor ? 1 : 2)));
        }

        //Otherwise if this is the main render call, do that.
        else {
            
            //Render the shapes.
            Vector3 depth = new Vector3(0, 0, 32766);
            List<int> shapeGroupsAlreadyProcessed = new List<int>();
            List<int> shapesAlreadyProcessed = new List<int>();
            for (int m = 0; m < vectorSpritesProperties.selectedEntities.Count; m++) {
                if (vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.Shape) {
                    if (shapeGroupsAlreadyProcessed.Contains(vectorSpritesProperties.selectedEntities[m].primaryID))
                        continue;
                    else
                        shapeGroupsAlreadyProcessed.Add(vectorSpritesProperties.selectedEntities[m].primaryID);
                }
                for (int l = vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.Sprite ? 0 :
                        vectorSpritesProperties.selectedEntities[m].primaryID;
                        l < (vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.Sprite ? vectorSpritesProperties.shapeGroups.Count :
                        vectorSpritesProperties.selectedEntities[m].primaryID + 1); l++)
                    for (int i = 0; i < vectorSpritesProperties.shapeGroups[l].shapes.Count; i++) {
                        if (vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.Sprite) {
                            if (!shapeAssociatedWithSelectedSprite(m, l, i))
                                continue;
                            bool alreadyProcessed = false;
                            for (int n = 0; n < shapeGroupsAlreadyProcessed.Count; n++)
                                if (shapeGroupsAlreadyProcessed[n] == l && shapesAlreadyProcessed[n] == i) {
                                    alreadyProcessed = true;
                                    break;
                                }
                            if (alreadyProcessed)
                                continue;
                            else {
                                shapeGroupsAlreadyProcessed.Add(l);
                                shapesAlreadyProcessed.Add(i);
                            }
                        }
                        VectorSprites.Shape shape = vectorSpritesProperties.shapeGroups[l].shapes[i];
                        for (int j = 0; j < shape.shadows.Length; j++)
                            if (shape.shadows[j].visible) {
                                shapeMaterial.SetInt("fillStyle", (int) VectorSprites.FillStyle.SolidColour);
                                shapeMaterial.SetColor("colour1", shape.shadows[j].colour);
                                shapeMaterial.SetPass(0);
                                drawMeshWithWrapping(shape, shape.shapeMesh, true, new Vector3((shape.shadows[j].offset.x / 100),
                                        (shape.shadows[j].offset.y / 100), depth.z), false, null);
                                depth.z -= 1;
                            }
                        for (int k = 0; k < 3; k++) {
                            for (int j = 0; j < shape.shapes.Length; j++)
                                if (shape.shapes[j].style != VectorSprites.FillStyle.None) {
                                    setShaderParameters(shape.shapes[j], shape.shapeMeshMinimumCoordinates, shape.shapeMeshMaximumCoordinates,
                                            shape.alphaBlendMode, vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.Shape,
                                            vectorSpritesProperties.zoom, vectorSpritesProperties.zoomCentre);
                                    if (k == 0 || k == 2) {
                                        if (drawMeshWithWrapping(shape, shape.shapeMesh, false, depth, false, k == 2))
                                            depth.z -= 1;
                                    }
                                    else {
                                        Graphics.DrawMeshNow(shape.shapeMesh, Matrix4x4.TRS(depth, Quaternion.identity, meshScale));
                                        depth.z -= 1;
                                    }
                                }
                            for (int j = 0; j < shape.pillows.Length; j++)
                                if (shape.pillows[j].visible) {
                                    pillowAndGlowRenderTextureMaterial.SetTexture("_MainTex", shape.pillowMeshesRenderTextures[j]);
                                    pillowAndGlowRenderTextureMaterial.SetPass(0);
                                    if (k == 0 || k == 2) {
                                        if (drawMeshWithWrapping(shape, quad, false, depth, true, k == 2))
                                            depth.z -= 1;
                                    }
                                    else {
                                        Graphics.DrawMeshNow(quad, Matrix4x4.TRS((editor ? new Vector3(
                                                (0.5f - vectorSpritesProperties.zoomCentre.x) * vectorSpritesProperties.zoom * 2,
                                                (0.5f - vectorSpritesProperties.zoomCentre.y) * vectorSpritesProperties.zoom * 2, 0) : Vector3.zero) + depth,
                                                Quaternion.identity, Vector2.one * (editor ? vectorSpritesProperties.zoom : 2)));
                                        depth.z -= 1;
                                    }
                                }
                            for (int j = 0; j < shape.glows.Length; j++)
                                if (shape.glows[j].visible) {
                                    pillowAndGlowRenderTextureMaterial.SetTexture("_MainTex", shape.glowMeshesRenderTextures[j]);
                                    pillowAndGlowRenderTextureMaterial.SetPass(0);
                                    if (k == 0 || k == 2) {
                                        if (drawMeshWithWrapping(shape, quad, false, depth, true, k == 2))
                                            depth.z -= 1;
                                    }
                                    else {
                                        Graphics.DrawMeshNow(quad, Matrix4x4.TRS((editor ? new Vector3(
                                                (0.5f - vectorSpritesProperties.zoomCentre.x) * vectorSpritesProperties.zoom * 2,
                                                (0.5f - vectorSpritesProperties.zoomCentre.y) * vectorSpritesProperties.zoom * 2, 0) : Vector3.zero) + depth,
                                                Quaternion.identity, Vector2.one * (editor ? vectorSpritesProperties.zoom : 2)));
                                        depth.z -= 1;
                                    }
                                }
                            for (int j = 0; j < shape.outlines.Length; j++)
                                if (shape.outlines[j].style != VectorSprites.FillStyle.None) {
                                    setShaderParameters(shape.outlines[j], shape.outlineMeshMinimumCoordinates, shape.outlineMeshMaximumCoordinates,
                                            shape.alphaBlendMode, vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.Shape,
                                            vectorSpritesProperties.zoom, vectorSpritesProperties.zoomCentre);
                                    if (k == 0 || k == 2) {
                                        if (drawMeshWithWrapping(shape, shape.outlineMesh, false, depth, false, k == 2))
                                            depth.z -= 1;
                                    }
                                    else {
                                        Graphics.DrawMeshNow(shape.outlineMesh, Matrix4x4.TRS(depth, Quaternion.identity, meshScale));
                                        depth.z -= 1;
                                    }
                                }
                        }
                    }
            }
        }
    }

    //Creates and returns a mesh that represents the outline of a shape, with a specified bezier subdivision. Also used for "pillow" and "glow" shading, which
    //are like an outline but on the inside and outside of the mesh respectively.
    static Mesh getOutlineMesh(VectorSprites.Shape shape, OutlineMeshType outlineMeshType, int pillowOrGlowShadingIndex, out Vector2 minimumCoordinates,
            out Vector2 maximumCoordinates, VectorSprites.Quality quality) {

        //Construct a list of bezier points for the outline, and direction vectors between the previous and current points.
        List<Vector2> bezierPoints = new List<Vector2>();
        Vector3 previousEndPoint = shape.shapePoints[shape.shapePoints.Count - 1].endPoint;
        for (int i = 0; i < shape.shapePoints.Count; i++) {
            bezierPoints.Add(previousEndPoint);
            for (int k = 1; k < bezierSubdivisions[(int) quality]; k++)
                bezierPoints.Add(getPointOnBezier(previousEndPoint, shape.shapePoints[i].endPoint, shape.shapePoints[i].startTangent,
                        shape.shapePoints[i].endTangent, (float) k / bezierSubdivisions[(int) quality]));
            previousEndPoint = shape.shapePoints[i].endPoint;
        }

        //Determine how much the co-ordinates should be scales depending on the outline/pillow/glow thickness.
        float coordinatesScale = (outlineMeshType == OutlineMeshType.Outline ? shape.outlineWidth :
                (outlineMeshType == OutlineMeshType.Pillow ? shape.pillows[pillowOrGlowShadingIndex].distance :
                shape.glows[pillowOrGlowShadingIndex].distance)) * 0.1f;

        //Pre-calculate the pillow/glow angle direction, if required.
        Vector2 pillowOrGlowAngleDirection = Vector2.zero;
        bool usesPillowOrGlowAngles = false;
        if (outlineMeshType == OutlineMeshType.Pillow && shape.pillows[pillowOrGlowShadingIndex].type == VectorSprites.EdgeShadingType.Angle) {
            pillowOrGlowAngleDirection = new Vector2(Mathf.Sin(-shape.pillows[pillowOrGlowShadingIndex].angle * Mathf.Deg2Rad),
                    Mathf.Cos(-shape.pillows[pillowOrGlowShadingIndex].angle * Mathf.Deg2Rad)) * coordinatesScale;
            usesPillowOrGlowAngles = true;
        }
        else if (outlineMeshType == OutlineMeshType.Glow && shape.glows[pillowOrGlowShadingIndex].type == VectorSprites.EdgeShadingType.Angle) {
            pillowOrGlowAngleDirection = new Vector2(Mathf.Sin(-shape.glows[pillowOrGlowShadingIndex].angle * Mathf.Deg2Rad),
                    Mathf.Cos(-shape.glows[pillowOrGlowShadingIndex].angle * Mathf.Deg2Rad)) * coordinatesScale;
            usesPillowOrGlowAngles = true;
        }

        //Add the vertices, offsetting each one depending on the draw properties.
        int halfVertexCount = bezierPoints.Count;
        Vector2[] vertices = new Vector2[halfVertexCount * 2];
        for (int i = 0; i < halfVertexCount; i++) {
            Vector2 direction;
            if (usesPillowOrGlowAngles)
                direction = pillowOrGlowAngleDirection;
            else {
                direction = bezierPoints[(i + bezierPoints.Count - 1) % bezierPoints.Count] - bezierPoints[i];
                direction = new Vector2(direction.y, -direction.x).normalized * coordinatesScale;
            }
            vertices[i] = bezierPoints[i] - (outlineMeshType == OutlineMeshType.Glow ? Vector2.zero : direction);
            vertices[i + halfVertexCount] = bezierPoints[i] + (outlineMeshType == OutlineMeshType.Pillow ? Vector2.zero : direction);
        }

        //Set the triangles.
        int[] triangles = new int[halfVertexCount * 6];
        for (int i = 0; i < halfVertexCount; i++) {
            triangles[i * 6] = i;
            triangles[(i * 6) + 1] = (i + 1) % halfVertexCount;
            triangles[(i * 6) + 2] = i + halfVertexCount;
            triangles[(i * 6) + 3] = (i + 1) % halfVertexCount;
            triangles[(i * 6) + 4] = ((i + 1) % halfVertexCount) + halfVertexCount;
            triangles[(i * 6) + 5] = i + halfVertexCount;
        }

        //Store the minimum/maximum vertex co-ordinates of the mesh
        minimumCoordinates = new Vector2(float.MaxValue, float.MaxValue);
        maximumCoordinates = new Vector2(float.MinValue, float.MinValue);
        Vector3[] vertices3D = new Vector3[vertices.Length];
        for (int i = 0; i < vertices.Length; i++) {
            vertices3D[i] = transformPointToRenderTextureBounds(vertices[i]);
            if (vertices3D[i].x < minimumCoordinates.x)
                minimumCoordinates.x = vertices3D[i].x;
            if (vertices3D[i].x > maximumCoordinates.x)
                maximumCoordinates.x = vertices3D[i].x;
            if (vertices3D[i].y < minimumCoordinates.y)
                minimumCoordinates.y = vertices3D[i].y;
            if (vertices3D[i].y > maximumCoordinates.y)
                maximumCoordinates.y = vertices3D[i].y;
        }

        //Create the mesh from the vertex and triangle data above, and return it.
        Mesh mesh = new Mesh();
        mesh.hideFlags = HideFlags.HideAndDontSave;
        mesh.vertices = vertices3D;
        mesh.triangles = triangles;

        //Set the UV co-ordinates to allow shading if this is for pillow/glow shading (irrelevant for outlines).
        if (outlineMeshType == OutlineMeshType.Pillow || outlineMeshType == OutlineMeshType.Glow) {
            Vector2[] uvCoordinates = new Vector2[vertices.Length];
            for (int i = 0; i < uvCoordinates.Length; i++)
                uvCoordinates[i] = new Vector2(outlineMeshType == OutlineMeshType.Pillow ^ i < halfVertexCount ? 0 : 1, 0);
            mesh.uv = uvCoordinates;
        }

        //Return the mesh.
        mesh.RecalculateNormals();
        ;
        return mesh;
    }

    //Creates and returns a mesh to fill a given shape, with a specified bezier subdivision.
    static Mesh getFillMesh(VectorSprites.Shape shape, out Vector2 minimumCoordinates, out Vector2 maximumCoordinates, VectorSprites.Quality quality) {

        //Get the bezier points for each of the lines in the shape and put them into the vertices array.
        List<Vector2> vertices = new List<Vector2>();
        Vector2 previousPoint = shape.shapePoints[shape.shapePoints.Count - 1].endPoint;
        for (int i = 0; i < shape.shapePoints.Count; i++) {
            vertices.Add(previousPoint);
            for (int j = 1; j < bezierSubdivisions[(int) quality]; j++)
                vertices.Add(getPointOnBezier(previousPoint, shape.shapePoints[i].endPoint, shape.shapePoints[i].startTangent, shape.shapePoints[i].endTangent,
                        (float) j / bezierSubdivisions[(int) quality]));
            previousPoint = shape.shapePoints[i].endPoint;
        }

        //Construct a linked list of vertices and flag whether they are reflex.
        LinkedList<int> allVertices = new LinkedList<int>();
        for (int i = 0; i < vertices.Count; i++)
            allVertices.AddLast(i + (isAngleReflex(vertices[(i + vertices.Count - 1) % vertices.Count], vertices[(i + 1) % vertices.Count], vertices[i]) ?
                    1000000 : 0));

        //Initialise the triangles array.
        int[] triangles = new int[(vertices.Count - 2) * 3];
        int triangleIndex = 0;

        //Repeatedly loop over the linked list of vertices and use an "ear clipping" method to remove polygon ears. Loop while there is at least another
        //triangle remaining (and the algorithm hasn't failed).
        LinkedListNode<int> currentNode = allVertices.First;
        int failedAttempts = 0;
        while (allVertices.Count > 2 && failedAttempts < allVertices.Count) {

            //Get the next convex point.
            while (currentNode.Value >= 1000000 && failedAttempts < allVertices.Count) {
                currentNode = currentNode.Next;
                if (currentNode == null)
                    currentNode = allVertices.First;
                failedAttempts++;
            }
            if (failedAttempts == allVertices.Count)
                break;

            //Check that no other (reflex) points are in the triangle made by this convex point and its neighbours. If there are no other points inside the
            //triangle, we have an ear.
            int previousIndex = (currentNode.Previous == null ? allVertices.Last.Value : currentNode.Previous.Value) % 1000000;
            int thisIndex = currentNode.Value;
            int nextIndex = (currentNode.Next == null ? allVertices.First.Value : currentNode.Next.Value) % 1000000;
            bool ear = true;
            float minX = Mathf.Min(Mathf.Min(vertices[previousIndex].x, vertices[thisIndex].x), vertices[nextIndex].x);
            float maxX = Mathf.Max(Mathf.Max(vertices[previousIndex].x, vertices[thisIndex].x), vertices[nextIndex].x);
            float minY = Mathf.Min(Mathf.Min(vertices[previousIndex].y, vertices[thisIndex].y), vertices[nextIndex].y);
            float maxY = Mathf.Max(Mathf.Max(vertices[previousIndex].y, vertices[thisIndex].y), vertices[nextIndex].y);
            for (LinkedListNode<int> i = allVertices.First; i != null; i = i.Next) {
                int vertexIndex = i.Value - 1000000;
                if (vertexIndex >= 0 && vertexIndex != previousIndex && vertexIndex != thisIndex && vertexIndex != nextIndex &&
                        vertices[vertexIndex].x >= minX && vertices[vertexIndex].x <= maxX &&
                        vertices[vertexIndex].y >= minY && vertices[vertexIndex].y <= maxY &&
                        pointInTriangle(vertices[previousIndex], vertices[thisIndex], vertices[nextIndex], vertices[vertexIndex])) {
                    ear = false;
                    break;
                }
            }

            //Create the ear triangle and remove the vertex at the centre of it.
            if (ear) {
                triangles[triangleIndex++] = nextIndex;
                triangles[triangleIndex++] = thisIndex;
                triangles[triangleIndex++] = previousIndex;
                LinkedListNode<int> newNodeToCalculate = (currentNode.Previous == null ? allVertices.Last : currentNode.Previous);
                allVertices.Remove(currentNode);
                if (allVertices.Count <= 2)
                    break;
                for (int i = 0; i < 2; i++) {
                    if (newNodeToCalculate.Value >= 1000000 &&
                            !isAngleReflex(vertices[(newNodeToCalculate.Previous == null ? allVertices.Last.Value : newNodeToCalculate.Previous.Value) %
                            1000000], vertices[(newNodeToCalculate.Next == null ? allVertices.First.Value : newNodeToCalculate.Next.Value) % 1000000],
                            vertices[newNodeToCalculate.Value - 1000000]))
                        newNodeToCalculate.Value -= 1000000;
                    if (i == 0)
                        newNodeToCalculate = (newNodeToCalculate.Next == null ? allVertices.First : newNodeToCalculate.Next);
                }
                currentNode = newNodeToCalculate;
                failedAttempts = 0;
            }
            else
                currentNode = currentNode.Next == null ? allVertices.First : currentNode.Next;
        }

        //Store the minimum/maximum vertex co-ordinates of the mesh.
        minimumCoordinates = new Vector2(float.MaxValue, float.MaxValue);
        maximumCoordinates = new Vector2(float.MinValue, float.MinValue);
        Vector3[] vertices3D = new Vector3[vertices.Count];
        for (int i = 0; i < vertices3D.Length; i++) {
            vertices3D[i] = transformPointToRenderTextureBounds(vertices[i]);
            if (vertices3D[i].x < minimumCoordinates.x)
                minimumCoordinates.x = vertices3D[i].x;
            if (vertices3D[i].x > maximumCoordinates.x)
                maximumCoordinates.x = vertices3D[i].x;
            if (vertices3D[i].y < minimumCoordinates.y)
                minimumCoordinates.y = vertices3D[i].y;
            if (vertices3D[i].y > maximumCoordinates.y)
                maximumCoordinates.y = vertices3D[i].y;
        }

        //Create the mesh from the vertex and triangle data above, and return it.
        Mesh mesh = new Mesh();
        mesh.hideFlags = HideFlags.HideAndDontSave;
        if (allVertices.Count <= 2) {
            mesh.vertices = vertices3D;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            ;
        }
        return mesh;
    }

    //Transforms a point whose co-ordinates are in the range 0..1 to be within render texture bounds (-1..1).
    static Vector2 transformPointToRenderTextureBounds(Vector2 point) {
        return new Vector2((point.x * 2) - 1, (point.y * 2) - 1);
    }

    //Returns whether a point is in a triangle.
    static bool pointInTriangle(Vector2 point1, Vector2 point2, Vector2 point3, Vector2 testPoint) {
        bool b1 = ((testPoint.x - point2.x) * (point1.y - point2.y)) - ((point1.x - point2.x) * (testPoint.y - point2.y)) <= 0;
        bool b2 = ((testPoint.x - point3.x) * (point2.y - point3.y)) - ((point2.x - point3.x) * (testPoint.y - point3.y)) <= 0;
        if (b1 != b2)
            return false;
        bool b3 = ((testPoint.x - point1.x) * (point3.y - point1.y)) - ((point3.x - point1.x) * (testPoint.y - point1.y)) <= 0;
        return b1 == b2 && b2 == b3;
    }

    //Sets various generic shader parameters for filling in a shape/outline.
    void setShaderParameters(VectorSprites.Fill fill, Vector2 minimumCoordinates, Vector2 maximumCoordinates, VectorSprites.AlphaBlendMode alphaBlendMode,
            bool showingIndividualShape, float zoom, Vector2 zoomCentre) {
        if (alphaBlendMode == VectorSprites.AlphaBlendMode.Blend) {
            shapeMaterial.SetInt("colourSourceBlend", (int) BlendMode.SrcAlpha);
            shapeMaterial.SetInt("colourDestinationBlend", (int) BlendMode.OneMinusSrcAlpha);
            shapeMaterial.SetInt("alphaSourceBlend", (int) BlendMode.One);
            shapeMaterial.SetInt("alphaDestinationBlend", (int) BlendMode.OneMinusSrcAlpha);
            shapeMaterial.SetInt("alphaBlendOperation", (int) BlendOp.Add);
        }
        else if (alphaBlendMode == VectorSprites.AlphaBlendMode.Overwrite) {
            if (showingIndividualShape) {
                shapeMaterial.SetInt("colourSourceBlend", (int) BlendMode.One);
                shapeMaterial.SetInt("colourDestinationBlend", (int) BlendMode.Zero);
                shapeMaterial.SetInt("alphaSourceBlend", (int) BlendMode.One);
                shapeMaterial.SetInt("alphaDestinationBlend", (int) BlendMode.Zero);
                shapeMaterial.SetInt("alphaBlendOperation", (int) BlendOp.Add);
            }
            else {
                shapeMaterial.SetInt("colourSourceBlend", (int) BlendMode.DstColor);
                shapeMaterial.SetInt("colourDestinationBlend", (int) BlendMode.Zero);
                shapeMaterial.SetInt("alphaSourceBlend", (int) BlendMode.One);
                shapeMaterial.SetInt("alphaDestinationBlend", (int) BlendMode.Zero);
                shapeMaterial.SetInt("alphaBlendOperation", (int) BlendOp.Min);
            }
        }
        shapeMaterial.SetInt("fillStyle", (int) fill.style);
        Color colour1;
        if (alphaBlendMode == VectorSprites.AlphaBlendMode.Overwrite) {
            if (showingIndividualShape)
                colour1 = new Color(fill.colour1.a, fill.colour1.a, fill.colour1.a, 1);
            else
                colour1 = new Color(1, 1, 1, fill.colour1.a);
        }
        else
            colour1 = fill.colour1;
        shapeMaterial.SetColor("colour1", colour1);
        if (fill.style != VectorSprites.FillStyle.SolidColour && fill.style != VectorSprites.FillStyle.Noise) {
            Color colour2;
            if (alphaBlendMode == VectorSprites.AlphaBlendMode.Overwrite) {
                if (showingIndividualShape)
                    colour2 = new Color(fill.colour2.a, fill.colour2.a, fill.colour2.a, 1);
                else
                    colour2 = new Color(1, 1, 1, fill.colour2.a);
            }
            else
                colour2 = fill.colour2;
            shapeMaterial.SetColor("colour2", colour2);
            if (fill.style == VectorSprites.FillStyle.AngledLinear || fill.style == VectorSprites.FillStyle.HorizontalLinear ||
                    fill.style == VectorSprites.FillStyle.Radial || fill.style == VectorSprites.FillStyle.VerticalLinear ||
                    fill.style == VectorSprites.FillStyle.AngledBilinear || fill.style == VectorSprites.FillStyle.HorizontalBilinear ||
                    fill.style == VectorSprites.FillStyle.VerticalBilinear) {
                shapeMaterial.SetFloat("colourBias", fill.colourBias);
                shapeMaterial.SetInt("colourBands", fill.colourBands);
            }
        }
        float meshAspectRatio = meshScale.y / meshScale.x;
        if (Math.Max(meshScale.x, meshScale.y) < 1.0001f) {
            shapeMaterial.SetFloat("areaFromX", fill.area == VectorSprites.FillArea.Sprite ? 0.25f : (minimumCoordinates.x + 1) / 2);
            shapeMaterial.SetFloat("areaToX", fill.area == VectorSprites.FillArea.Sprite ? 0.75f : (maximumCoordinates.x + 1) / 2);
            shapeMaterial.SetFloat("areaFromY", fill.area == VectorSprites.FillArea.Sprite ? 0.75f : 1 - ((minimumCoordinates.y + 1) / 2));
            shapeMaterial.SetFloat("areaToY", fill.area == VectorSprites.FillArea.Sprite ? 0.25f : 1 - ((maximumCoordinates.y + 1) / 2));
        }
        else if (fill.area == VectorSprites.FillArea.Sprite) {
            shapeMaterial.SetFloat("areaFromX", meshAspectRatio >= 1 ? 0 : (-(0.5f / meshAspectRatio) + 0.5f));
            shapeMaterial.SetFloat("areaToX", meshAspectRatio >= 1 ? 1 : ((0.5f / meshAspectRatio) + 0.5f));
            shapeMaterial.SetFloat("areaFromY", meshAspectRatio < 1 ? 1 : ((0.5f * meshAspectRatio) + 0.5f));
            shapeMaterial.SetFloat("areaToY", meshAspectRatio < 1 ? 0 : (-(0.5f * meshAspectRatio) + 0.5f));
        }
        else {
            shapeMaterial.SetFloat("areaFromX", (minimumCoordinates.x / Mathf.Min(meshAspectRatio, 1)) + 0.5f);
            shapeMaterial.SetFloat("areaToX", (maximumCoordinates.x / Mathf.Min(meshAspectRatio, 1)) + 0.5f);
            shapeMaterial.SetFloat("areaFromY", 1 - ((minimumCoordinates.y * Mathf.Max(meshAspectRatio, 1)) + 0.5f));
            shapeMaterial.SetFloat("areaToY", 1 - ((maximumCoordinates.y * Mathf.Max(meshAspectRatio, 1)) + 0.5f));
        }
        if (fill.style == VectorSprites.FillStyle.AngledLinear || fill.style == VectorSprites.FillStyle.AngledBars ||
                fill.style == VectorSprites.FillStyle.Checkerboard || fill.style == VectorSprites.FillStyle.AngledBilinear)
            shapeMaterial.SetFloat("angle", fill.angle * Mathf.Deg2Rad);
        if (fill.style == VectorSprites.FillStyle.HorizontalBars || fill.style == VectorSprites.FillStyle.VerticalBars ||
                fill.style == VectorSprites.FillStyle.AngledBars || fill.style == VectorSprites.FillStyle.Checkerboard)
            shapeMaterial.SetInt("bars", fill.bars);
        if (fill.style == VectorSprites.FillStyle.Noise) {
            shapeMaterial.SetInt("noiseType", (int) fill.noiseType);
            shapeMaterial.SetFloat("noiseLevel", fill.noiseLevel);
        }
        if (fill.style == VectorSprites.FillStyle.Radial)
            shapeMaterial.SetFloat("radialSize", fill.radialSize);
        if (fill.style == VectorSprites.FillStyle.AngledBars || fill.style == VectorSprites.FillStyle.AngledLinear ||
                fill.style == VectorSprites.FillStyle.Checkerboard || fill.style == VectorSprites.FillStyle.HorizontalLinear ||
                fill.style == VectorSprites.FillStyle.Radial || fill.style == VectorSprites.FillStyle.VerticalBars ||
                fill.style == VectorSprites.FillStyle.AngledBilinear || fill.style == VectorSprites.FillStyle.HorizontalBilinear)
            shapeMaterial.SetFloat("centreX", fill.centre.x);
        if (fill.style == VectorSprites.FillStyle.Checkerboard || fill.style == VectorSprites.FillStyle.HorizontalBars ||
                fill.style == VectorSprites.FillStyle.Radial || fill.style == VectorSprites.FillStyle.VerticalLinear ||
                fill.style == VectorSprites.FillStyle.VerticalBilinear)
            shapeMaterial.SetFloat("centreY", fill.centre.y);
        shapeMaterial.SetFloat("zoom", editor ? zoom : 1);
        shapeMaterial.SetFloat("zoomCentreX", editor ? zoomCentre.x : 0.5f);
        shapeMaterial.SetFloat("zoomCentreY", editor ? zoomCentre.y : 0.5f);
        shapeMaterial.SetPass(0);
    }

    //Return a point on one of the bezier curves.
    public static Vector2 getPointOnBezier(Vector2 startPoint, Vector2 endPoint, Vector2 startTangent, Vector2 endTangent, float amount) {
        float oneMinusAmount = 1 - amount;
        float oneMinusAmountCubed = oneMinusAmount * oneMinusAmount * oneMinusAmount;
        float amountCubed = amount * amount * amount;
        return new Vector2((oneMinusAmountCubed * startPoint.x) + (amount * oneMinusAmount * oneMinusAmount * startTangent.x * 3) +
                (amount * amount * oneMinusAmount * endTangent.x * 3) + (amountCubed * endPoint.x),
                (oneMinusAmountCubed * startPoint.y) + (amount * oneMinusAmount * oneMinusAmount * startTangent.y * 3) +
                (amount * amount * oneMinusAmount * endTangent.y * 3) + (amountCubed * endPoint.y));
    }

    //Draws a mesh, wrapping to the left, right, top and/or bottom if those options are enabled.
    bool drawMeshWithWrapping(VectorSprites.Shape shape, Mesh mesh, bool drawOriginalMeshToo, Vector3 offset, bool pillowOrGlow, bool? onTop) {
        bool wrapLeft = shape.wrapLeft && (onTop == null || (!shape.wrapLeftOnTop ^ (bool) onTop));
        bool wrapRight = shape.wrapRight && (onTop == null || (!shape.wrapRightOnTop ^ (bool) onTop));
        bool wrapTop = shape.wrapTop && (onTop == null || (!shape.wrapTopOnTop ^ (bool) onTop));
        bool wrapBottom = shape.wrapBottom && (onTop == null || (!shape.wrapBottomOnTop ^ (bool) onTop));
        float originalAreaFromX = shapeMaterial.GetFloat("areaFromX"), originalAreaToX = shapeMaterial.GetFloat("areaToX");
        float originalAreaFromY = shapeMaterial.GetFloat("areaFromY"), originalAreaToY = shapeMaterial.GetFloat("areaToY");
        if (wrapLeft || wrapRight) {
            if (wrapLeft) {
                if (!pillowOrGlow) {
                    shapeMaterial.SetFloat("areaFromX", originalAreaFromX - (meshScale.x / 2));
                    shapeMaterial.SetFloat("areaToX", originalAreaToX - (meshScale.x / 2));
                    shapeMaterial.SetPass(0);
                }
                Graphics.DrawMeshNow(mesh, Matrix4x4.TRS(new Vector3(-meshScale.x, 0, 0) + offset, Quaternion.identity,
                        pillowOrGlow ? Vector2.one * (editor ? 1 : 2) : meshScale));
                if (wrapTop) {
                    if (!pillowOrGlow) {
                        shapeMaterial.SetFloat("areaFromY", originalAreaFromY + (meshScale.y / 2));
                        shapeMaterial.SetFloat("areaToY", originalAreaToY + (meshScale.y / 2));
                        shapeMaterial.SetPass(0);
                    }
                    Graphics.DrawMeshNow(mesh, Matrix4x4.TRS(new Vector3(-meshScale.x, -meshScale.y, 0) + offset, Quaternion.identity,
                            pillowOrGlow ? Vector2.one * (editor ? 1 : 2) : meshScale));
                }
                if (wrapBottom) {
                    if (!pillowOrGlow) {
                        shapeMaterial.SetFloat("areaFromY", originalAreaFromY - (meshScale.y / 2));
                        shapeMaterial.SetFloat("areaToY", originalAreaToY - (meshScale.y / 2));
                        shapeMaterial.SetPass(0);
                    }
                    Graphics.DrawMeshNow(mesh, Matrix4x4.TRS(new Vector3(-meshScale.x, meshScale.y, 0) + offset, Quaternion.identity,
                            pillowOrGlow ? Vector2.one * (editor ? 1 : 2) : meshScale));
                }
            }
            if (wrapRight) {
                if (!pillowOrGlow) {
                    shapeMaterial.SetFloat("areaFromX", originalAreaFromX + (meshScale.x / 2));
                    shapeMaterial.SetFloat("areaToX", originalAreaToX + (meshScale.x / 2));
                    shapeMaterial.SetPass(0);
                }
                Graphics.DrawMeshNow(mesh, Matrix4x4.TRS(new Vector3(meshScale.x, 0, 0) + offset, Quaternion.identity,
                        pillowOrGlow ? Vector2.one * (editor ? 1 : 2) : meshScale));
                if (wrapTop) {
                    if (!pillowOrGlow) {
                        shapeMaterial.SetFloat("areaFromY", originalAreaFromY + (meshScale.y / 2));
                        shapeMaterial.SetFloat("areaToY", originalAreaToY + (meshScale.y / 2));
                        shapeMaterial.SetPass(0);
                    }
                    Graphics.DrawMeshNow(mesh, Matrix4x4.TRS(new Vector3(meshScale.x, -meshScale.y, 0) + offset, Quaternion.identity,
                            pillowOrGlow ? Vector2.one * (editor ? 1 : 2) : meshScale));
                }
                if (wrapBottom) {
                    if (!pillowOrGlow) {
                        shapeMaterial.SetFloat("areaFromY", originalAreaFromY - (meshScale.y / 2));
                        shapeMaterial.SetFloat("areaToY", originalAreaToY - (meshScale.y / 2));
                        shapeMaterial.SetPass(0);
                    }
                    Graphics.DrawMeshNow(mesh, Matrix4x4.TRS(new Vector3(meshScale.x, meshScale.y, 0) + offset, Quaternion.identity,
                            pillowOrGlow ? Vector2.one * (editor ? 1 : 2) : meshScale));
                }
            }
            if (!pillowOrGlow) {
                shapeMaterial.SetFloat("areaFromX", originalAreaFromX);
                shapeMaterial.SetFloat("areaToX", originalAreaToX);
            }
        }
        if (wrapTop || wrapBottom) {
            if (wrapTop) {
                if (!pillowOrGlow) {
                    shapeMaterial.SetFloat("areaFromY", originalAreaFromY + (meshScale.y / 2));
                    shapeMaterial.SetFloat("areaToY", originalAreaToY + (meshScale.y / 2));
                    shapeMaterial.SetPass(0);
                }
                Graphics.DrawMeshNow(mesh, Matrix4x4.TRS(new Vector3(0, -meshScale.y, 0) + offset, Quaternion.identity,
                        pillowOrGlow ? Vector2.one * (editor ? 1 : 2) : meshScale));
            }
            if (wrapBottom) {
                if (!pillowOrGlow) {
                    shapeMaterial.SetFloat("areaFromY", originalAreaFromY - (meshScale.y / 2));
                    shapeMaterial.SetFloat("areaToY", originalAreaToY - (meshScale.y / 2));
                    shapeMaterial.SetPass(0);
                }
                Graphics.DrawMeshNow(mesh, Matrix4x4.TRS(new Vector3(0, meshScale.y, 0) + offset, Quaternion.identity,
                        pillowOrGlow ? Vector2.one * (editor ? 1 : 2) : meshScale));
            }
            if (!pillowOrGlow) {
                shapeMaterial.SetFloat("areaFromY", originalAreaFromY);
                shapeMaterial.SetFloat("areaToY", originalAreaToY);
            }
        }
        if (drawOriginalMeshToo) {
            if (!pillowOrGlow && (wrapLeft || wrapRight || wrapTop || wrapBottom))
                shapeMaterial.SetPass(0);
            Graphics.DrawMeshNow(mesh, Matrix4x4.TRS(offset, Quaternion.identity, pillowOrGlow ? Vector2.one * (editor ? 1 : 2) : meshScale));
        }
        return drawOriginalMeshToo || wrapLeft || wrapRight || wrapTop || wrapBottom;
    }

    //Returns whether an angle is reflex.
    static bool isAngleReflex(Vector2 vector1, Vector2 vector2, Vector2 origin) {
        vector1 -= origin;
        vector2 -= origin;
        return (vector1.x * vector2.y) - (vector1.y * vector2.x) < 0;
    }

    //Called when the Monobehavious is destroyed.
    void OnDestroy() {
        if (quad != null)
            DestroyImmediate(quad);
    }

    //Returns whether a given shape within a shape group is associated with the currently-selected sprite.
    bool shapeAssociatedWithSelectedSprite(int entityIndex, int shapeGroupID, int shapeID) {
        for (int i = 0; i < vectorSpritesProperties.vectorSprites[vectorSpritesProperties.selectedEntities[entityIndex].primaryID].shapeGroupIDs.Count; i++)
            if (vectorSpritesProperties.vectorSprites[vectorSpritesProperties.selectedEntities[entityIndex].primaryID].shapeGroupIDs[i] == shapeGroupID &&
                    (vectorSpritesProperties.vectorSprites[vectorSpritesProperties.selectedEntities[entityIndex].primaryID].shapeIDs[i] == shapeID ||
                    vectorSpritesProperties.vectorSprites[vectorSpritesProperties.selectedEntities[entityIndex].primaryID].shapeIDs[i] == -1))
                return true;
        return false;
    }
}