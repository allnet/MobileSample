using System;
using System.Collections.Generic;
using UnityEngine;

public class VectorSprites : MonoBehaviour {

    //Enumerated Types.
    public enum FillStyle {
        None, AngledBars, AngledBilinear, AngledLinear, Checkerboard, HorizontalBars, HorizontalBilinear, HorizontalLinear, Noise, Radial,
        SolidColour, VerticalBars, VerticalBilinear, VerticalLinear
    };
    public enum FillArea { Shape, Sprite };
    public enum NoiseType { Random, RGB };
    public enum EdgeShadingType { Angle, Circle };
    public enum ShapeStyleType { Fill, Glow, Outline, Pillow, Shadow };
    public enum SelectableEntity { ShapeGroup, Shape, NewShape, Sprite, SpriteSheet };
    public enum SpriteRectangleTransform { Crop, Scale };
    public enum AlphaBlendMode { Blend, Overwrite };
    public enum Quality { Worst, Low, Average, High, Best };
    public enum TransformType { Translate, Rotate, Scale };
    public enum TransformOrigin { ShapeCentre, SpriteCentre };

    [Serializable]
    public class VectorSpritesProperties {

        //Constants.
        public const string currentVersion = "1.1.0";

        //Variables.
        public string version = currentVersion;
        public string name = "";
        public List<SpriteSheet> spriteSheets = new List<SpriteSheet>();
        public List<VectorSprite> vectorSprites = new List<VectorSprite>();
        public List<ShapeGroup> shapeGroups = new List<ShapeGroup>();
        public int gridWidth = 32, gridHeight = 32;
        public bool showGrid = true, snapToGrid = true, gridOnTop = false, showGuidelines = true;
        public SelectableEntity selectedEntity;
        public List<SelectedEntity> selectedEntities = new List<SelectedEntity>();
        public Vector2 shapesScrollPosition = Vector2.zero, spritesScrollPosition = Vector2.zero;
        public Quality editorQuality = Quality.Average;
        public Quality gameQuality = Quality.Best;
        public TransformType transformType = TransformType.Translate;
        public TransformOrigin transformOrigin = TransformOrigin.ShapeCentre;
        public Vector2 shapeTranslation = Vector2.zero;
        public float shapeRotation = 0;
        public Vector2 shapeScale = Vector2.one;
        public bool showOtherShapesWhenCreatingNewShape = false;
        public float zoom = 1;
        public Vector2 zoomCentre = new Vector2(0.5f, 0.5f);

        //Update the version number.
        public bool updateVersion() {

            //If the version matches, return true.
            if (version == currentVersion)
                return true;

            //If the version is behind the current version, perform any necessary updates and increment the version number.
            if (version == "1.0.0") {

                //Set up the zoom parameters.
                zoom = 1;
                zoomCentre = new Vector2(0.5f, 0.5f);

                //Individual shapes can now be associated with sprites, as well as shape groups. There is now a shape groups list and a shapes list, and to
                //represent the association of a shape group with a sprite, the shape groups list should contain the shape group ID and the shapes list should
                //contain -1. So for each shape group in the shape groups list, add a -1 to the shapes list.
                for (int i = 0; i < vectorSprites.Count; i++) {
                    vectorSprites[i].shapeIDs.Clear();
                    for (int j = 0; j < vectorSprites[i].shapeGroupIDs.Count; j++)
                        vectorSprites[i].shapeIDs.Add(-1);
                }

                //Reset the selected entities list. The previously-selected entity will be lost but this is no big deal.
                selectedEntities = new List<SelectedEntity>();

                //Create a single sprite sheet and assign all sprites to it.
                spriteSheets.Clear();
                spriteSheets.Add(new SpriteSheet("All Sprites"));
                for (int i = 0; i < vectorSprites.Count; i++)
                    vectorSprites[i].spriteSheet = 0;

                //Set the new version number.
                version = "1.1.0";
            }

            //Return whether the versions match.
            return version == currentVersion;
        }

        //Creates a copy of a sprite, adds it to the list of sprites and returns the index of the new sprite.
        public int copySprite(int index) {
            VectorSprite vectorSprite = new VectorSprite(vectorSprites[index].name + " - Copy");
            vectorSprite.width = vectorSprites[index].width;
            vectorSprite.height = vectorSprites[index].height;
            vectorSprite.antialias = vectorSprites[index].antialias;
            vectorSprite.shapeGroupsScrollPosition = vectorSprites[index].shapeGroupsScrollPosition;
            vectorSprite.spriteRectangleTransform = vectorSprites[index].spriteRectangleTransform;
            vectorSprite.spriteSheet = vectorSprites[index].spriteSheet;
            vectorSprite.shapeGroupIDs = new List<int>();
            for (int i = 0; i < vectorSprites[index].shapeGroupIDs.Count; i++)
                vectorSprite.shapeGroupIDs.Add(vectorSprites[index].shapeGroupIDs[i]);
            vectorSprite.shapeIDs = new List<int>();
            for (int i = 0; i < vectorSprites[index].shapeIDs.Count; i++)
                vectorSprite.shapeIDs.Add(vectorSprites[index].shapeIDs[i]);
            vectorSprites.Add(vectorSprite);
            return vectorSprites.Count - 1;
        }
    }
    [Serializable]
    public class SelectedEntity {
        public int primaryID, secondaryID;
        public SelectedEntity(int primaryID) {
            this.primaryID = primaryID;
            this.secondaryID = -1;
        }
        public SelectedEntity(int primaryID, int secondaryID) {
            this.primaryID = primaryID;
            this.secondaryID = secondaryID;
        }
    }
    [Serializable]
    public class SpriteSheet {
        public string name;
        public bool expanded = true;
        public string exportPath = "";
        public SpriteSheet(string name) {
            this.name = name;
        }
    }
    [Serializable]
    public class VectorSprite {
        public string name = "";
        public int width = 64;
        public int height = 64;
        public bool antialias = true;
        public Vector2 shapeGroupsScrollPosition = Vector2.zero;
        public List<int> shapeGroupIDs = new List<int>();
        public List<int> shapeIDs = new List<int>();
        public SpriteRectangleTransform spriteRectangleTransform = SpriteRectangleTransform.Crop;
        [NonSerialized]
        public Sprite sprite;
        public int spriteSheet = -1;
        public string exportPath = "";
        public VectorSprite(string name) {
            this.name = name;
        }
    }
    [Serializable]
    public class ShapeGroup {
        public string name = "";
        public List<Shape> shapes = new List<Shape>();
        public bool expanded = true;
        public ShapeGroup(string name) {
            this.name = name;
        }

        //Creates a copy of a shape in this group, add it to this shape group and return the index of the new shape within this group.
        public int copyShape(int index) {
            Shape shape = new Shape(shapes[index].name + " - Copy");
            shape.wrapLeft = shapes[index].wrapLeft;
            shape.wrapRight = shapes[index].wrapRight;
            shape.wrapTop = shapes[index].wrapTop;
            shape.wrapBottom = shapes[index].wrapBottom;
            shape.wrapLeftOnTop = shapes[index].wrapLeftOnTop;
            shape.wrapRightOnTop = shapes[index].wrapRightOnTop;
            shape.wrapTopOnTop = shapes[index].wrapTopOnTop;
            shape.wrapBottomOnTop = shapes[index].wrapBottomOnTop;
            shape.outlineWidth = shapes[index].outlineWidth;
            shape.outlines = new Fill[shapes[index].outlines.Length];
            for (int i = 0; i < shape.outlines.Length; i++) {
                shape.outlines[i] = new Fill();
                shape.outlines[i].style = shapes[index].outlines[i].style;
                shape.outlines[i].colour1 = shapes[index].outlines[i].colour1;
                shape.outlines[i].colour2 = shapes[index].outlines[i].colour2;
                shape.outlines[i].colourBias = shapes[index].outlines[i].colourBias;
                shape.outlines[i].colourBands = shapes[index].outlines[i].colourBands;
                shape.outlines[i].area = shapes[index].outlines[i].area;
                shape.outlines[i].angle = shapes[index].outlines[i].angle;
                shape.outlines[i].bars = shapes[index].outlines[i].bars;
                shape.outlines[i].noiseType = shapes[index].outlines[i].noiseType;
                shape.outlines[i].noiseLevel = shapes[index].outlines[i].noiseLevel;
                shape.outlines[i].centre = shapes[index].outlines[i].centre;
                shape.outlines[i].radialSize = shapes[index].outlines[i].radialSize;
            }
            shape.shapes = new Fill[shapes[index].shapes.Length];
            for (int i = 0; i < shape.shapes.Length; i++) {
                shape.shapes[i] = new Fill();
                shape.shapes[i].style = shapes[index].shapes[i].style;
                shape.shapes[i].colour1 = shapes[index].shapes[i].colour1;
                shape.shapes[i].colour2 = shapes[index].shapes[i].colour2;
                shape.shapes[i].colourBias = shapes[index].shapes[i].colourBias;
                shape.shapes[i].colourBands = shapes[index].shapes[i].colourBands;
                shape.shapes[i].area = shapes[index].shapes[i].area;
                shape.shapes[i].angle = shapes[index].shapes[i].angle;
                shape.shapes[i].bars = shapes[index].shapes[i].bars;
                shape.shapes[i].noiseType = shapes[index].shapes[i].noiseType;
                shape.shapes[i].noiseLevel = shapes[index].shapes[i].noiseLevel;
                shape.shapes[i].centre = shapes[index].shapes[i].centre;
                shape.shapes[i].radialSize = shapes[index].shapes[i].radialSize;
            }
            shape.shadows = new Shadow[shapes[index].shadows.Length];
            for (int i = 0; i < shape.shadows.Length; i++) {
                shape.shadows[i] = new Shadow();
                shape.shadows[i].visible = shapes[index].shadows[i].visible;
                shape.shadows[i].offset = shapes[index].shadows[i].offset;
                shape.shadows[i].colour = shapes[index].shadows[i].colour;
            }
            shape.pillows = new Edge[shapes[index].pillows.Length];
            for (int i = 0; i < shape.pillows.Length; i++) {
                shape.pillows[i] = new Edge(Color.white);
                shape.pillows[i].visible = shapes[index].pillows[i].visible;
                shape.pillows[i].distance = shapes[index].pillows[i].distance;
                shape.pillows[i].colourFrom = shapes[index].pillows[i].colourFrom;
                shape.pillows[i].colourTo = shapes[index].pillows[i].colourTo;
                shape.pillows[i].colourBias = shapes[index].pillows[i].colourBias;
                shape.pillows[i].colourBands = shapes[index].pillows[i].colourBands;
                shape.pillows[i].type = shapes[index].pillows[i].type;
                shape.pillows[i].angle = shapes[index].pillows[i].angle;
            }
            shape.glows = new Edge[shapes[index].glows.Length];
            for (int i = 0; i < shape.glows.Length; i++) {
                shape.glows[i] = new Edge(Color.white);
                shape.glows[i].visible = shapes[index].glows[i].visible;
                shape.glows[i].distance = shapes[index].glows[i].distance;
                shape.glows[i].colourFrom = shapes[index].glows[i].colourFrom;
                shape.glows[i].colourTo = shapes[index].glows[i].colourTo;
                shape.glows[i].colourBias = shapes[index].glows[i].colourBias;
                shape.glows[i].colourBands = shapes[index].glows[i].colourBands;
                shape.glows[i].type = shapes[index].glows[i].type;
                shape.glows[i].angle = shapes[index].glows[i].angle;
            }
            shape.shapePoints = new List<ShapePoint>();
            for (int i = 0; i < shapes[index].shapePoints.Count; i++) {
                shape.shapePoints.Add(new ShapePoint(Vector2.zero, shapes[index].shapePoints[i].endPoint));
                shape.shapePoints[i].startTangent = shapes[index].shapePoints[i].startTangent;
                shape.shapePoints[i].endTangent = shapes[index].shapePoints[i].endTangent;
            }
            shape.outlineSelectedLayer = shapes[index].outlineSelectedLayer;
            shape.fillSelectedLayer = shapes[index].fillSelectedLayer;
            shape.shadowSelectedLayer = shapes[index].shadowSelectedLayer;
            shape.pillowSelectedLayer = shapes[index].pillowSelectedLayer;
            shape.glowSelectedLayer = shapes[index].glowSelectedLayer;
            shape.shapeStyleType = shapes[index].shapeStyleType;
            shape.alphaBlendMode = shapes[index].alphaBlendMode;
            shapes.Add(shape);
            return shapes.Count - 1;
        }
    }
    [Serializable]
    public class Shape {
        public string name = "";
        public bool wrapLeft = false, wrapRight = false, wrapTop = false, wrapBottom = false;
        public bool wrapLeftOnTop = false, wrapRightOnTop = false, wrapTopOnTop = false, wrapBottomOnTop = false;
        public float outlineWidth = 0.1f;
        public Fill[] outlines = new Fill[] { new Fill(), new Fill(), new Fill(), new Fill() };
        public Fill[] shapes = new Fill[] { new Fill(), new Fill(), new Fill(), new Fill() };
        public Shadow[] shadows = new Shadow[] { new Shadow() };
        public Edge[] pillows = new Edge[] { new Edge(new Color(0, 0, 0, 0.5f)), new Edge(new Color(0, 0, 0, 0.5f)) };
        public Edge[] glows = new Edge[] { new Edge(new Color(1, 1, 1, 0.5f)) };
        public List<ShapePoint> shapePoints = new List<ShapePoint>();
        public int outlineSelectedLayer = 0;
        public int fillSelectedLayer = 0;
        public int shadowSelectedLayer = 0;
        public int pillowSelectedLayer = 0;
        public int glowSelectedLayer = 0;
        [NonSerialized]
        public Mesh outlineMesh;
        [NonSerialized]
        public Vector2 outlineMeshMinimumCoordinates;
        [NonSerialized]
        public Vector2 outlineMeshMaximumCoordinates;
        [NonSerialized]
        public Mesh shapeMesh;
        [NonSerialized]
        public Vector2 shapeMeshMinimumCoordinates;
        [NonSerialized]
        public Vector2 shapeMeshMaximumCoordinates;
        [NonSerialized]
        public Mesh[] pillowMeshes;
        [NonSerialized]
        public RenderTexture[] pillowMeshesRenderTextures;
        [NonSerialized]
        public Mesh[] glowMeshes;
        [NonSerialized]
        public RenderTexture[] glowMeshesRenderTextures;
        public ShapeStyleType shapeStyleType = ShapeStyleType.Fill;
        public AlphaBlendMode alphaBlendMode = AlphaBlendMode.Blend;

        //Constructor.
        public Shape(string name) {
            this.name = name;
            shapes[0].style = FillStyle.SolidColour;
        }

        //Return whether the shape has various parts to it.
        public bool hasOutline() {
            for (int i = 0; i < outlines.Length; i++)
                if (outlines[i].style != FillStyle.None)
                    return true;
            return false;
        }
        public bool hasFillOrShadow() {
            for (int i = 0; i < shapes.Length; i++)
                if (shapes[i].style != FillStyle.None)
                    return true;
            for (int i = 0; i < shadows.Length; i++)
                if (shadows[i].visible)
                    return true;
            return false;
        }
        public bool hasPillow() {
            for (int i = 0; i < pillows.Length; i++)
                if (pillows[i].visible)
                    return true;
            return false;
        }
        public bool hasGlow() {
            for (int i = 0; i < glows.Length; i++)
                if (glows[i].visible)
                    return true;
            return false;
        }

        //Reset all meshes.
        public void resetAllMeshes() {
            if (outlineMesh != null)
                DestroyImmediate(outlineMesh);
            outlineMesh = null;
            if (shapeMesh != null)
                DestroyImmediate(shapeMesh);
            shapeMesh = null;
            if (pillowMeshesRenderTextures != null)
                for (int i = 0; i < pillowMeshesRenderTextures.Length; i++) {
                    if (pillowMeshesRenderTextures[i] != null)
                        DestroyImmediate(pillowMeshesRenderTextures[i]);
                    pillowMeshesRenderTextures[i] = null;
                }
            if (pillowMeshes != null)
                for (int i = 0; i < pillowMeshes.Length; i++) {
                    if (pillowMeshes[i] != null)
                        DestroyImmediate(pillowMeshes[i]);
                    pillowMeshes[i] = null;
                }
            if (glowMeshesRenderTextures != null)
                for (int i = 0; i < glowMeshesRenderTextures.Length; i++) {
                    if (glowMeshesRenderTextures[i] != null)
                        DestroyImmediate(glowMeshesRenderTextures[i]);
                    glowMeshesRenderTextures[i] = null;
                }
            if (glowMeshes != null)
                for (int i = 0; i < glowMeshes.Length; i++) {
                    if (glowMeshes[i] != null)
                        DestroyImmediate(glowMeshes[i]);
                    glowMeshes[i] = null;
                }
        }
    }
    [Serializable]
    public class Fill {
        public FillStyle style = FillStyle.None;
        public Color colour1 = Color.white;
        public Color colour2 = Color.black;
        public float colourBias = 0.5f;
        public int colourBands = 255;
        public FillArea area = FillArea.Shape;
        public float angle = 0;
        public int bars = 8;
        public NoiseType noiseType = NoiseType.RGB;
        public float noiseLevel = 0.1f;
        public Vector2 centre = new Vector2(0.5f, 0.5f);
        public float radialSize = 0.5f;
    }
    [Serializable]
    public class Shadow {
        public bool visible = false;
        public Vector2 offset = new Vector2(1, 1);
        public Color colour = new Color(0, 0, 0, 0.5f);
    }
    [Serializable]
    public class Edge {
        public bool visible = false;
        public float distance = 0.25f;
        public Color colourFrom = new Color(0, 0, 0, 0.5f);
        public Color colourTo = new Color(0, 0, 0, 0);
        public float colourBias = 0.5f;
        public int colourBands = 255;
        public EdgeShadingType type = EdgeShadingType.Circle;
        public float angle = 0;
        public Edge(Color baseColour) {
            colourFrom = new Color(baseColour.r, baseColour.g, baseColour.b, baseColour.a);
            colourTo = new Color(baseColour.r, baseColour.g, baseColour.b, 0);
        }
    }
    [Serializable]
    public class ShapePoint {
        public Vector2 endPoint;
        public Vector2 startTangent = Vector2.zero;
        public Vector2 endTangent = Vector2.zero;
        public ShapePoint(Vector2 previousPoint, Vector2 endPoint) {
            this.endPoint = endPoint;
            startTangent = (previousPoint * 0.6667f) + (endPoint * 0.3333f);
            endTangent = (previousPoint * 0.3333f) + (endPoint * 0.6667f);
        }
    }

    //Properties.
    public VectorSpritesProperties vectorSpritesProperties;
    [HideInInspector]
    public Material shapeMaterial;
    [HideInInspector]
    public Material pillowAndGlowMeshMaterial;
    [HideInInspector]
    public Material pillowAndGlowRenderTextureMaterial;
    [HideInInspector, NonSerialized]
    public Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

    //Awake.
    void Awake() {

        //Detect whether the materials can be found. If not, report an error.
        if (shapeMaterial == null || pillowAndGlowMeshMaterial == null || pillowAndGlowRenderTextureMaterial == null) {
            Debug.LogError("At least one of the material properties on this Vector Sprites instance have been removed. The materials need to be assigned in " +
                "order to create the sprites. Please re-assign them to this Vector Sprites instance (you may need to comment out the [HideInInspector] " +
                "attributes on the material properties in \"VectorSprites.cs\" in order to assign them again).");
            return;
        }

        //Clear the sprites dictionary.
        sprites.Clear();

        //If the version is not up to date, don't do anything.
        if (!vectorSpritesProperties.updateVersion())
            return;

        //Set all meshes to dirty to ensure they are re-generated (the game quality might not match the editor quality).
        for (int i = 0; i < vectorSpritesProperties.shapeGroups.Count; i++)
            for (int j = 0; j < vectorSpritesProperties.shapeGroups[i].shapes.Count; j++)
                vectorSpritesProperties.shapeGroups[i].shapes[j].resetAllMeshes();

        //Create a Vector Sprites Renderer game object so the Vector Sprites can be rendered to textures.
        GameObject vectorSpritesRendererGameObject = new GameObject("Vector Sprites Renderer");
        vectorSpritesRendererGameObject.hideFlags = HideFlags.HideAndDontSave;
        VectorSpritesRenderer vectorSpritesRenderer = vectorSpritesRendererGameObject.AddComponent<VectorSpritesRenderer>();
        vectorSpritesRenderer.createdFromVectorSpritesInstance.flag = true;

        //Store the old selection so it can be restored after the sprites are generated.
        SelectableEntity oldSelectedEntity = vectorSpritesProperties.selectedEntity;
        List<int> oldSelectedEntityPrimaryIDs = new List<int>();
        List<int> oldSelectedEntitySecondaryIDs = new List<int>();
        for (int i = 0; i < vectorSpritesProperties.selectedEntities.Count; i++) {
            oldSelectedEntityPrimaryIDs.Add(vectorSpritesProperties.selectedEntities[i].primaryID);
            oldSelectedEntitySecondaryIDs.Add(vectorSpritesProperties.selectedEntities[i].secondaryID);
        }

        //Create the array of sprites, one for each Vector Sprite and loop over them in order to create them.
        for (int i = 0; i < vectorSpritesProperties.vectorSprites.Count; i++) {

            //Work out the sprite's scale.
            Vector2 meshScale = new Vector2(2, 2);
            if (vectorSpritesProperties.vectorSprites[i].spriteRectangleTransform == SpriteRectangleTransform.Crop) {
                if (vectorSpritesProperties.vectorSprites[i].width > vectorSpritesProperties.vectorSprites[i].height)
                    meshScale.y *= (float) vectorSpritesProperties.vectorSprites[i].width / vectorSpritesProperties.vectorSprites[i].height;
                else
                    meshScale.x *= (float) vectorSpritesProperties.vectorSprites[i].height / vectorSpritesProperties.vectorSprites[i].width;
            }

            //Render the sprite to a render texture. Double the size of the render texture if anti-aliasing is enabled (so it can be scaled back down).
            RenderTexture renderTexture = new RenderTexture(vectorSpritesProperties.vectorSprites[i].width *
                    (vectorSpritesProperties.vectorSprites[i].antialias ? 2 : 1), vectorSpritesProperties.vectorSprites[i].height *
                    (vectorSpritesProperties.vectorSprites[i].antialias ? 2 : 1), 16, RenderTextureFormat.ARGB32);
            renderTexture.hideFlags = HideFlags.HideAndDontSave;
            vectorSpritesProperties.selectedEntity = SelectableEntity.Sprite;
            vectorSpritesProperties.selectedEntities.Clear();
            vectorSpritesProperties.selectedEntities.Add(new SelectedEntity(i));
            vectorSpritesRenderer.render(vectorSpritesProperties, shapeMaterial, pillowAndGlowMeshMaterial, pillowAndGlowRenderTextureMaterial, renderTexture,
                    meshScale, false);

            //Get the texture directly from the render texture.
            RenderTexture.active = renderTexture;
            Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0, false);
            texture.Apply();
            RenderTexture.active = null;
            DestroyImmediate(renderTexture);

            //Copy the texture to the master texture, scaling down for anti-aliasing purposes if required.
            if (vectorSpritesProperties.vectorSprites[i].antialias) {
                renderTexture = new RenderTexture(vectorSpritesProperties.vectorSprites[i].width, vectorSpritesProperties.vectorSprites[i].height, 16,
                        RenderTextureFormat.ARGB32);
                renderTexture.hideFlags = HideFlags.HideAndDontSave;
                Graphics.Blit(texture, renderTexture);
                DestroyImmediate(texture);
                texture = new Texture2D(vectorSpritesProperties.vectorSprites[i].width, vectorSpritesProperties.vectorSprites[i].height, TextureFormat.ARGB32,
                        false);
                RenderTexture.active = renderTexture;
                texture.ReadPixels(new Rect(0, 0, vectorSpritesProperties.vectorSprites[i].width, vectorSpritesProperties.vectorSprites[i].height), 0, 0,
                        false);
                RenderTexture.active = null;
                DestroyImmediate(renderTexture);
                texture.Apply();
            }

            //Create the sprite.
            vectorSpritesProperties.vectorSprites[i].sprite = Sprite.Create(texture, new Rect(0, 0, vectorSpritesProperties.vectorSprites[i].width,
                    vectorSpritesProperties.vectorSprites[i].height), new Vector2(0.5f, 0.5f));

            //Add the sprite to the dictionary if it doesn't contain one with the same name.
            if (!sprites.ContainsKey(vectorSpritesProperties.vectorSprites[i].name))
                sprites.Add(vectorSpritesProperties.vectorSprites[i].name, vectorSpritesProperties.vectorSprites[i].sprite);
        }

        //Restore the previous selected entity.
        vectorSpritesProperties.selectedEntity = oldSelectedEntity;
        vectorSpritesProperties.selectedEntities.Clear();
        for (int i = 0; i < oldSelectedEntityPrimaryIDs.Count; i++)
            vectorSpritesProperties.selectedEntities.Add(new VectorSprites.SelectedEntity(oldSelectedEntityPrimaryIDs[i], oldSelectedEntitySecondaryIDs[i]));

        //Reset all shapes' meshes and render textures now that it has been rendered.
        for (int i = 0; i < vectorSpritesProperties.shapeGroups.Count; i++)
            for (int j = 0; j < vectorSpritesProperties.shapeGroups[i].shapes.Count; j++)
                vectorSpritesProperties.shapeGroups[i].shapes[j].resetAllMeshes();

        //Destroy the temporary Vector Sprites Renderer game object.
        Destroy(vectorSpritesRendererGameObject);
    }
}