using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {
    [SerializeField]
    SpriteRenderer first;
    [SerializeField]
    SpriteRenderer second;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            first.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            second.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            first.maskInteraction = SpriteMaskInteraction.None;
            second.maskInteraction = SpriteMaskInteraction.None;
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            first.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
            second.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
        }
    }
}
