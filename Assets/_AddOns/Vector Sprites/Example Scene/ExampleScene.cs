using UnityEngine;

public class ExampleScene : MonoBehaviour {

    //Constants.
    static readonly string[,] tilesBackground = {
        { "", "", "", "", "", "", "", "Soil + Grass - Right", "Soil + Grass" },
        { "Soil + Grass", "Soil + Grass", "Soil + Grass - Left", "", "", "", "", "Soil Right", "Soil" },
        { "Soil + Grass", "Soil + Grass", "Soil + Grass", "Soil + Grass - Left", "", "", "", "Soil Right", "Soil" },
        { "Soil + Grass", "Soil + Grass", "Soil + Grass", "Soil + Grass", "Soil + Grass", "Soil + Grass - Left", "Soil + Grass - Right", "Soil + Grass",
                "Soil + Grass" },
        { "Soil", "Soil", "Soil", "Soil", "Soil", "Soil Left", "Soil Right", "Soil", "Soil" }
    };
    static readonly string[,] tilesForeground = {
        { "Green Apple", "", "", "", "", "", "", "", "" },
        { "", "", "Red Apple", "", "", "", "", "", "" },
        { "", "", "", "", "Green Apple", "", "", "Spring", "" },
        { "", "", "", "", "", "", "", "", "" },
        { "", "", "", "", "", "Spikes", "Spikes", "", "" }
    };

    //Properties.
    public VectorSprites backgrounds, tileSet;

	//Start.
	void Start() {

        //Set the background sprite.
        GetComponent<SpriteRenderer>().sprite = backgrounds.sprites["Background 1"];

        //Create game objects containing sprite renderers for the tiles.
        for (int k = 0; k < 2; k++)
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 5; j++)
                    if ((k == 0 && tilesBackground[4 - j, i] != "") || (k == 1 && tilesForeground[4 - j, i] != "")) {
                        GameObject tile = new GameObject("Tile (" + i.ToString() + ", " + j.ToString() + ") - " + (k == 0 ? "Background" : "Foreground"));
                        tile.transform.SetParent(transform, true);
                        tile.transform.position = new Vector3((i - 4) * 1.275f, (j - 2) * 1.275f, 0);
                        SpriteRenderer spriteRenderer = tile.AddComponent<SpriteRenderer>();
                        spriteRenderer.sprite = tileSet.sprites[k == 0 ? tilesBackground[4 - j, i] : tilesForeground[4 - j, i]];
                    }
	}
}
