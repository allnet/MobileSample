using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class VectorSpritesExport : EditorWindow {

    //Classes
    class PackingSpace {
        public int X, Y;
        public int width, height;
        public int vectorSpriteIndex;
        public PackingSpace(int X, int Y, int width, int height) : this(X, Y, width, height, -1) { }
        public PackingSpace(int X, int Y, int width, int height, int vectorSpriteIndex) {
            this.X = X;
            this.Y = Y;
            this.width = width;
            this.height = height;
            this.vectorSpriteIndex = vectorSpriteIndex;
        }
    }

    //Variables.
    VectorSprites.VectorSpritesProperties vectorSpritesProperties;
    Texture2D texture = null;
    List<PackingSpace> packingSpaces = new List<PackingSpace>();
    bool tiled = false;
    int spriteSheetIndex;
    int spriteIndex;

    //Initialise.
    public void initialise(VectorSprites.VectorSpritesProperties vectorSpritesProperties, Material shapeMaterial, Material pillowAndGlowMeshMaterial,
            Material pillowAndGlowRenderTextureMaterial, VectorSpritesRenderer vectorSpritesRenderer) {

        //Store properties.
        this.vectorSpritesProperties = vectorSpritesProperties;
        if (vectorSpritesProperties.selectedEntity == VectorSprites.SelectableEntity.SpriteSheet) {
            spriteSheetIndex = vectorSpritesProperties.selectedEntities[0].primaryID;
            spriteIndex = -1;
        }
        else {
            spriteSheetIndex = -1;
            spriteIndex = vectorSpritesProperties.selectedEntities[0].primaryID;
        }

        //Set all meshes to dirty to ensure they are re-generated (the export quality might not match the editor quality).
        for (int i = 0; i < vectorSpritesProperties.shapeGroups.Count; i++)
            for (int j = 0; j < vectorSpritesProperties.shapeGroups[i].shapes.Count; j++)
                vectorSpritesProperties.shapeGroups[i].shapes[j].resetAllMeshes();

        //Create a list of packing spaces to pack all sprites in. If there is a single sprite, just create a single packing space, otherwise try to fit all
        //sprites in as smaller space as possible.
        packingSpaces.Clear();
        if (spriteSheetIndex == -1)
            packingSpaces.Add(new PackingSpace(0, 0, vectorSpritesProperties.vectorSprites[vectorSpritesProperties.selectedEntities[0].primaryID].width,
                    vectorSpritesProperties.vectorSprites[vectorSpritesProperties.selectedEntities[0].primaryID].height,
                    vectorSpritesProperties.selectedEntities[0].primaryID));

        //If multiple sprites are being added to the sprite sheet, pack the sprites on the sprite sheet in the most efficient way possible.
        else {

            //Sort the sprites in descending order of size (maximum dimension), which should produce more efficient packing.
            List<int> spriteOrder = new List<int>();
            for (int i = 0; i < vectorSpritesProperties.vectorSprites.Count; i++)
                if (vectorSpritesProperties.vectorSprites[i].spriteSheet == spriteSheetIndex)
                    spriteOrder.Add(i);
            bool spriteOrderSwapChanges = true;
            while (spriteOrderSwapChanges) {
                spriteOrderSwapChanges = false;
                for (int i = 0; i < spriteOrder.Count - 1; i++) {
                    int thisMaxCoordinate = Math.Max(vectorSpritesProperties.vectorSprites[spriteOrder[i]].width,
                            vectorSpritesProperties.vectorSprites[spriteOrder[i]].height);
                    int nextMaxCoordinate = Math.Max(vectorSpritesProperties.vectorSprites[spriteOrder[i + 1]].width,
                            vectorSpritesProperties.vectorSprites[spriteOrder[i + 1]].height);
                    int thisMinCoordinate = Math.Min(vectorSpritesProperties.vectorSprites[spriteOrder[i]].width,
                            vectorSpritesProperties.vectorSprites[spriteOrder[i]].height);
                    int nextMinCoordinate = Math.Min(vectorSpritesProperties.vectorSprites[spriteOrder[i + 1]].width,
                            vectorSpritesProperties.vectorSprites[spriteOrder[i + 1]].height);
                    if (thisMaxCoordinate < nextMaxCoordinate || (thisMaxCoordinate == nextMaxCoordinate && thisMinCoordinate < nextMinCoordinate)) {
                        int spriteOrderSwap = spriteOrder[i];
                        spriteOrder[i] = spriteOrder[i + 1];
                        spriteOrder[i + 1] = spriteOrderSwap;
                        spriteOrderSwapChanges = true;
                    }
                }
            }

            //Start with a single packing space, and loop over the sprites to place into spaces.
            packingSpaces.Add(new PackingSpace(0, 0, 65536, 65536));
            for (int j = 0; j < spriteOrder.Count; j++) {

                //Get the Vector Sprite.
                VectorSprites.VectorSprite vectorSprite = vectorSpritesProperties.vectorSprites[spriteOrder[j]];

                //Find the best space to put this sprite such that it keeps the overall area as small as possible.
                ulong lowestTotalArea = ulong.MaxValue;
                int smallestWastedArea = int.MaxValue;
                int bestPackingSpaceIndex = -1;
                for (int k = 0; k < packingSpaces.Count; k++) {
                    if (packingSpaces[k].vectorSpriteIndex == -1) {
                        packingSpaces[k].vectorSpriteIndex = spriteOrder[j];
                        int oldWidth = packingSpaces[k].width, oldHeight = packingSpaces[k].height;
                        packingSpaces[k].width = vectorSprite.width;
                        packingSpaces[k].height = vectorSprite.height;
                        int newTextureSizeX, newTextureSizeY;
                        getMaximumPackingSize(out newTextureSizeX, out newTextureSizeY);
                        ulong newArea = ((ulong) newTextureSizeX * (ulong) newTextureSizeX) + ((ulong) newTextureSizeY * (ulong) newTextureSizeY);
                        int newWastedArea = (oldWidth * oldHeight) - (vectorSprite.width * vectorSprite.height);
                        if (newArea < lowestTotalArea || (newArea == lowestTotalArea && newWastedArea < smallestWastedArea)) {
                            lowestTotalArea = newArea;
                            smallestWastedArea = newWastedArea;
                            bestPackingSpaceIndex = k;
                        }
                        packingSpaces[k].vectorSpriteIndex = -1;
                        packingSpaces[k].width = oldWidth;
                        packingSpaces[k].height = oldHeight;
                    }
                }

                //Put the sprite in the assigned space.
                int originalWidth = packingSpaces[bestPackingSpaceIndex].width, originalHeight = packingSpaces[bestPackingSpaceIndex].height;
                packingSpaces[bestPackingSpaceIndex].width = vectorSprite.width;
                packingSpaces[bestPackingSpaceIndex].height = vectorSprite.height;
                packingSpaces[bestPackingSpaceIndex].vectorSpriteIndex = spriteOrder[j];
                if (originalWidth - packingSpaces[bestPackingSpaceIndex].width > 0)
                    packingSpaces.Add(new PackingSpace(packingSpaces[bestPackingSpaceIndex].X + packingSpaces[bestPackingSpaceIndex].width,
                            packingSpaces[bestPackingSpaceIndex].Y, originalWidth - packingSpaces[bestPackingSpaceIndex].width,
                            packingSpaces[bestPackingSpaceIndex].height));
                if (originalHeight - packingSpaces[bestPackingSpaceIndex].height > 0)
                    packingSpaces.Add(new PackingSpace(packingSpaces[bestPackingSpaceIndex].X, packingSpaces[bestPackingSpaceIndex].Y +
                            packingSpaces[bestPackingSpaceIndex].height, originalWidth, originalHeight - packingSpaces[bestPackingSpaceIndex].height));
            }
        }
        
        //Create an output texture, the size of which is the maximum size of all packing spaces.
        int textureSizeX, textureSizeY;
        getMaximumPackingSize(out textureSizeX, out textureSizeY);
        if (textureSizeX > 4096 || textureSizeY > 4096) {
            Close();
            EditorUtility.DisplayDialog("Texture Too Big", "The sprite sheet cannot fit within a texture of size 4096 by 4096 pixels. Try reducing the size " +
                    "of some of the individual sprites, or split the sprites into two sprite sheets.", "OK");
            return;
        }
        texture = new Texture2D(textureSizeX, textureSizeY, TextureFormat.ARGB32, false);
        texture.hideFlags = HideFlags.HideAndDontSave;
        texture.wrapMode = TextureWrapMode.Clamp;
        RenderTexture clearRenderTexture = new RenderTexture(textureSizeX, textureSizeY, 16, RenderTextureFormat.ARGB32);
        clearRenderTexture.hideFlags = HideFlags.HideAndDontSave;
        RenderTexture.active = clearRenderTexture;
        GL.Clear(true, true, new Color(0, 0, 0, 0));
        texture.ReadPixels(new Rect(0, 0, textureSizeX, textureSizeY), 0, 0, false);
        RenderTexture.active = null;
        DestroyImmediate(clearRenderTexture);

        //Loop over all the packing spaces that contain sprites, and add those sprites to the sprite sheet.
        VectorSprites.SelectableEntity oldSelectedEntity = vectorSpritesProperties.selectedEntity;
        List<int> oldSelectedEntityPrimaryIDs = new List<int>();
        List<int> oldSelectedEntitySecondaryIDs = new List<int>();
        for (int i = 0; i < vectorSpritesProperties.selectedEntities.Count; i++) {
            oldSelectedEntityPrimaryIDs.Add(vectorSpritesProperties.selectedEntities[i].primaryID);
            oldSelectedEntitySecondaryIDs.Add(vectorSpritesProperties.selectedEntities[i].secondaryID);
        }
        vectorSpritesProperties.selectedEntity = VectorSprites.SelectableEntity.Sprite;
        for (int i = 0; i < packingSpaces.Count; i++) {
            if (packingSpaces[i].vectorSpriteIndex == -1)
                continue;

            //Get the sprite and work out its scale.
            VectorSprites.VectorSprite vectorSprite = vectorSpritesProperties.vectorSprites[packingSpaces[i].vectorSpriteIndex];
            Vector2 meshScale = new Vector2(2, 2);
            if (vectorSprite.spriteRectangleTransform == VectorSprites.SpriteRectangleTransform.Crop) {
                if (vectorSprite.width > vectorSprite.height)
                    meshScale.y *= (float) vectorSprite.width / vectorSprite.height;
                else
                    meshScale.x *= (float) vectorSprite.height / vectorSprite.width;
            }

            //Render the sprite to a render texture. Double the size of the render texture if anti-aliasing is enabled (so it can be scaled back down).
            RenderTexture renderTexture = new RenderTexture(vectorSprite.width * (vectorSprite.antialias ? 2 : 1),
                    vectorSprite.height * (vectorSprite.antialias ? 2 : 1), 16, RenderTextureFormat.ARGB32);
            renderTexture.hideFlags = HideFlags.HideAndDontSave;
            vectorSpritesProperties.selectedEntities.Clear();
            vectorSpritesProperties.selectedEntities.Add(new VectorSprites.SelectedEntity(packingSpaces[i].vectorSpriteIndex));
            vectorSpritesRenderer.render(vectorSpritesProperties, shapeMaterial, pillowAndGlowMeshMaterial, pillowAndGlowRenderTextureMaterial, renderTexture,
                    meshScale, false);

            //Get the texture directly from the render texture.
            RenderTexture.active = renderTexture;
            Texture2D textureFromRenderTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
            textureFromRenderTexture.hideFlags = HideFlags.HideAndDontSave;
            textureFromRenderTexture.wrapMode = TextureWrapMode.Clamp;
            textureFromRenderTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0, false);
            textureFromRenderTexture.Apply();
            RenderTexture.active = null;
            DestroyImmediate(renderTexture);

            //Copy the texture to the master texture, scaling down for anti-aliasing purposes if required.
            renderTexture = new RenderTexture(vectorSprite.width, vectorSprite.height, 16, RenderTextureFormat.ARGB32);
            renderTexture.hideFlags = HideFlags.HideAndDontSave;
            Graphics.Blit(textureFromRenderTexture, renderTexture);
            RenderTexture.active = renderTexture;
            texture.ReadPixels(new Rect(0, 0, vectorSprite.width, vectorSprite.height), packingSpaces[i].X, packingSpaces[i].Y, false);
            RenderTexture.active = null;
            DestroyImmediate(textureFromRenderTexture);
            DestroyImmediate(renderTexture);
        }

        //Apply the changes to the texture.
        texture.Apply();

        //Restore the previous selected entity.
        vectorSpritesProperties.selectedEntity = oldSelectedEntity;
        vectorSpritesProperties.selectedEntities.Clear();
        for (int i = 0; i < oldSelectedEntityPrimaryIDs.Count; i++)
            vectorSpritesProperties.selectedEntities.Add(new VectorSprites.SelectedEntity(oldSelectedEntityPrimaryIDs[i], oldSelectedEntitySecondaryIDs[i]));

        //Set all meshes to dirty again so they can be re-generated in the editor, which may have different quality settings from export.
        for (int i = 0; i < vectorSpritesProperties.shapeGroups.Count; i++)
            for (int j = 0; j < vectorSpritesProperties.shapeGroups[i].shapes.Count; j++)
                vectorSpritesProperties.shapeGroups[i].shapes[j].resetAllMeshes();
    }

    //Update.
    void Update() {

        //Close the window if the selection has changed.
        if ((vectorSpritesProperties.selectedEntity != VectorSprites.SelectableEntity.Sprite &&
                vectorSpritesProperties.selectedEntity != VectorSprites.SelectableEntity.SpriteSheet) ||
                (spriteSheetIndex != -1 && vectorSpritesProperties.selectedEntity != VectorSprites.SelectableEntity.SpriteSheet) ||
                (spriteIndex != -1 && vectorSpritesProperties.selectedEntity != VectorSprites.SelectableEntity.Sprite))
            Close();
    }

    //Draw the GUI.
    void OnGUI() {

        //Draw the preview texture, maintaining its aspect ratio while ensuring it fits inside the window.
        float aspectRatio = (float) texture.width / texture.height;
        float width = Mathf.Min(texture.width, (int) (position.width / (tiled ? 3 : 1)));
        float height = width / aspectRatio;
        if (height > (position.height - 60) / (tiled ? 3 : 1)) {
            width *= ((position.height - 60) / (tiled ? 3 : 1)) / height;
            height *= ((position.height - 60) / (tiled ? 3 : 1)) / height;
        }
        for (int i = tiled ? -1 : 0; i <= (tiled ? 1 : 0); i++)
            for (int j = tiled ? -1 : 0; j <= (tiled ? 1 : 0); j++)
                Graphics.DrawTexture(new Rect((width * i) + ((position.width - width) / 2), (height * j) + ((position.height - height) / 2), width, height),
                        texture);

        //Display the header showing the sprite (sheet) name and size.
        GUIStyle headerLabelStyle = new GUIStyle(GUI.skin.label);
        headerLabelStyle.alignment = TextAnchor.UpperCenter;
        EditorGUI.LabelField(new Rect(0, 8, position.width, 19), spriteSheetIndex == -1 ? "Sprite: " +
                vectorSpritesProperties.vectorSprites[spriteIndex].name + " (" + vectorSpritesProperties.vectorSprites[spriteIndex].width.ToString() + "x" +
                vectorSpritesProperties.vectorSprites[spriteIndex].height.ToString() + " pixels)" :
                ("Sprite Sheet: " + vectorSpritesProperties.spriteSheets[spriteSheetIndex].name + " (" + texture.width + "x" + texture.height + ")"),
                headerLabelStyle);

        //Display the button to export the file as a PNG, which allows an asset file to be selected.
        if (GUI.Button(new Rect((position.width / 2) - 128, position.height - 25, 256, 19), new GUIContent("Export Sprite" +
                (spriteSheetIndex == -1 ? "" : " Sheet"), spriteSheetIndex == -1 ? "Export the sprite as an asset. The sprite is exported in PNG file format." :
                "Export the sprite sheet as an asset. The sprite is exported in PNG file format and will contain sprite sheet information."))) {
            string path = spriteSheetIndex == -1 ? vectorSpritesProperties.vectorSprites[spriteIndex].exportPath :
                    vectorSpritesProperties.spriteSheets[spriteSheetIndex].exportPath;
            try {
                path = Path.GetDirectoryName(path);
            }
            catch { }
            string filename = EditorUtility.SaveFilePanelInProject("Vector Sprites - Export to PNG",
                    spriteSheetIndex == -1 ?
                        (Path.GetFileName(vectorSpritesProperties.vectorSprites[spriteIndex].exportPath) != "" ?
                            Path.GetFileName(vectorSpritesProperties.vectorSprites[spriteIndex].exportPath) :
                            vectorSpritesProperties.vectorSprites[spriteIndex].name) :
                        (Path.GetFileName(vectorSpritesProperties.spriteSheets[spriteSheetIndex].exportPath) != "" ?
                            Path.GetFileName(vectorSpritesProperties.spriteSheets[spriteSheetIndex].exportPath) :
                            vectorSpritesProperties.spriteSheets[spriteSheetIndex].name),
                    "png",
                    "Please enter a filename to save the sprite " + (spriteSheetIndex == -1 ? "" : "sheet ") + "to",
                    path);
            if (filename != "") {

                //Export the texture to a PNG file and import it as an asset, splitting it up into multiple sprites if multiple images were included.
                AssetDatabase.DeleteAsset(filename);
                File.WriteAllBytes(filename, texture.EncodeToPNG());
                AssetDatabase.ImportAsset(filename, ImportAssetOptions.ForceUpdate);
                TextureImporter textureImporter = (TextureImporter) AssetImporter.GetAtPath(filename);
                if (spriteSheetIndex == -1) {
                    vectorSpritesProperties.vectorSprites[spriteIndex].exportPath = filename;
                    textureImporter.spriteImportMode = SpriteImportMode.Single;
                }
                else {
                    vectorSpritesProperties.spriteSheets[spriteSheetIndex].exportPath = filename;
                    for (int i = packingSpaces.Count - 1; i >= 0; i--)
                        if (packingSpaces[i].vectorSpriteIndex == -1)
                            packingSpaces.RemoveAt(i);
                    textureImporter.spriteImportMode = SpriteImportMode.Multiple;
                    SpriteMetaData[] spriteMetadata = new SpriteMetaData[packingSpaces.Count];
                    for (int i = 0; i < spriteMetadata.Length; i++)
                        if (packingSpaces[i].vectorSpriteIndex != -1) {
                            spriteMetadata[i].rect = new Rect(packingSpaces[i].X, packingSpaces[i].Y, packingSpaces[i].width, packingSpaces[i].height);
                            spriteMetadata[i].name = vectorSpritesProperties.vectorSprites[packingSpaces[i].vectorSpriteIndex].name;
                        }
                    textureImporter.spritesheet = spriteMetadata;
                    textureImporter.SaveAndReimport();
                }
            }
        }

        //Add a "tiled" checkbox (for single images).
        if (spriteSheetIndex == -1) {
            GUIStyle alignRight = new GUIStyle(GUI.skin.label);
            alignRight.alignment = TextAnchor.MiddleRight;
            tiled = EditorGUI.ToggleLeft(new Rect(position.width - 64, position.height - 25, 48, 19),
                    new GUIContent("Tiled", "Check to see what the preview image looks like tiled."), tiled, alignRight);
        }
    }

    //Returns the maximum size of all filled "PackingSize" instances.
    void getMaximumPackingSize(out int textureSizeX, out int textureSizeY) {
        textureSizeX = int.MinValue;
        textureSizeY = int.MinValue;
        for (int i = 0; i < packingSpaces.Count; i++)
            if (packingSpaces[i].vectorSpriteIndex != -1) {
                if (packingSpaces[i].X + packingSpaces[i].width > textureSizeX)
                    textureSizeX = packingSpaces[i].X + packingSpaces[i].width;
                if (packingSpaces[i].Y + packingSpaces[i].height > textureSizeY)
                    textureSizeY = packingSpaces[i].Y + packingSpaces[i].height;
            }
    }

    //Called when the window is destroyed.
    void OnDestroy() {
        if (texture != null)
            DestroyImmediate(texture);
    }
}
