using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClickPositioner : MonoBehaviour  //iPointerclicker
{
    public Transform videoScreen;  // typically a quad or some object
    public Vector3 startPosition;
    public PointerEventData lastPointerEventData;


    public void SetToPosition(Vector3 newPosition)
    {
        //lastPointerEventData = e;
        Debug.Log("Clicked at " + newPosition.ToString());

        videoScreen.transform.position = new Vector3(newPosition.x - 185, newPosition.y - 235, newPosition.z);
    }

    public void SetToScale(Vector3 scale) 
    {


    }

    void OnGUI()  // checks every update
    {
        //Event m_Event = Event.current;

        //switch (m_Event.type) 
        //{

        //    case EventType.MouseUp:
        //        Debug.Log("Mouse Up.");
        //        //SetTo
        //        break;

        //    case EventType.MouseDown:
        //        Debug.Log("Mouse Down.");
        //        break;

        //    case EventType.MouseDrag:
        //        Debug.Log("Mouse drag.");
        //        break;
        //}


        if (Input.GetMouseButtonDown(0)) {
            Debug.Log("Mouse Down.");
            SetToPosition(Input.mousePosition);
            
        }

    }

    //public void OnPointerClick(PointerEventData eventData)
    //{
    //    //throw new System.NotImplementedException();

    //    Debug.Log("Clicked at " + eventData.position.ToString());
    //    SetToPosition(eventData);
    //}
	

	// Update is called once per frame
	void Update () {
		
	}



}
