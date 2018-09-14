using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayOnClick : MonoBehaviour {

	ScrollSnapRect scrollSnapRect;

	// Use this for initialization
	void Start () {
		
		Debug.Log ("TEST");
	}
	
	// Update is called once per frame
	void Update () 
	{
		bool wasTouched = (Input.touchCount > 0 || Input.GetMouseButtonDown (0));

		scrollSnapRect = this.transform.parent.GetComponentInParent<ScrollSnapRect> ();
		scrollSnapRect.LerpToPage (4);


		if (wasTouched) {


		}

		
	}
}
