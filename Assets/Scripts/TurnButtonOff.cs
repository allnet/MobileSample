using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class TurnButtonOff : MonoBehaviour {


	private Button button; 
	// Use this for initialization
	void Start () {

//		button = gameObject.GetComponent <Button> ();
//		button.Select ();
//

		EventSystem.current.GetComponent<EventSystem>().SetSelectedGameObject(null);
	}
 
}
