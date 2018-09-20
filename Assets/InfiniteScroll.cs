using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteScroll : MonoBehaviour {

    private float rightEdge, leftEdge;
    private Vector3 distanceBetweenEdges;
    private float multiplier = 3.0f;
    [SerializeField] private float scrollSpeed;  // - to scroll opposite direction

	// Use this for initialization


	void Start () {

        CalculateEdges();
        distanceBetweenEdges = new Vector3(rightEdge - leftEdge, 0f, 0f);
    }

    void CalculateEdges() {

        var spriteRenderer = GetComponent<SpriteRenderer>();
        rightEdge = transform.position.x + spriteRenderer.bounds.extents.x / multiplier;
        leftEdge = transform.position.y - spriteRenderer.bounds.extents.x / multiplier;
    }


	
	// Update is called once per frame
	void Update () 
    {
        transform.localPosition += scrollSpeed * Vector3.right * Time.deltaTime;
		

        if (hasPassedEdge()) {

            MoveRightSpriteToOppositeEdge();
        }


	}


    bool hasPassedEdge() {

        return scrollSpeed > 0 && transform.position.x > rightEdge ||
                                           scrollSpeed < 0 && transform.position.x < leftEdge;

    }

    void MoveRightSpriteToOppositeEdge() 
    {

        if (scrollSpeed > 0)       
            transform.position -= distanceBetweenEdges;          
            else
                transform.position += distanceBetweenEdges;
     
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawCube(new Vector3(rightEdge, 0f, 0f), new Vector3(0.1f, 2f, 0.1f));
        Gizmos.DrawCube(new Vector3(leftEdge, 0f, 0f), new Vector3(0.1f, 2f, 0.1f));

    }

}
