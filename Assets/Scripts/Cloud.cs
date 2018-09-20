using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cloud : MonoBehaviour {

    [SerializeField]
    private float speed;

    private Vector3 startPos;

	// Use this for initialization
	void Start () {
        startPos = transform.position;		
	}
	
	// Update is called once per frame
	void Update () {

        transform.Translate(Vector3.right * Time.deltaTime * speed);
	}

    private void OnBecameInvisible()
    {
        transform.position = startPos;
    }
}
