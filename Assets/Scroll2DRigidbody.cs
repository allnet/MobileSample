using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scroll2DRigidbody : MonoBehaviour {

    private Rigidbody2D rigidbody;
    private float speed = -1.5f;
    [SerializeField] private bool shouldStopScrolling;

	// Use this for initialization
	void Awake () {

        rigidbody = this.gameObject.GetComponent<Rigidbody2D>();
        rigidbody.velocity = new Vector2(0, speed);
	}
	
	// Update is called once per frame
	void Update () {

        if (shouldStopScrolling) rigidbody.velocity = Vector2.zero;
        else rigidbody.velocity = new Vector2(0, speed);

		
	}
}
