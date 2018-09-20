using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

enum SpriteType { BushLarge, BushSmall, BushDead, Crate, SignArrow, SignSquare, Skeleton, Round, Cross, Tree }

public class AtlasTest : MonoBehaviour
{
    [SerializeField]
    private SpriteType currentType;

    private SpriteType lastType;

    [SerializeField]
    private SpriteAtlas atlas;

    private SpriteRenderer myRenderer;


	// Use this for initialization
	void Start () {
        myRenderer = GetComponent<SpriteRenderer>();
        lastType = SpriteType.Crate;

	}
	
	// Update is called once per frame
	void Update () {
        ChangeSprite();
	}

    private void ChangeSprite()
    {
        if (currentType != lastType)
        {
            myRenderer.sprite = atlas.GetSprite(currentType.ToString());

            lastType = currentType;
        }
    }
}
