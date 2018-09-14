using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using UnityEngine.UI;
using RenderHeads.Media.AVProVideo;
using RenderHeads.Media.AVProVideo.Demos;  //FIX-ME - gets me access to class defiinitions


public class PanelControllerMenu : MonoBehaviour {


	public GameObject[] mainMenuDisplayObjects;


	public void DoPlayMenuVideo(DisplayIMGUI videoDisplay) 
	{
		
		videoDisplay.gameObject.SetActive (true);
		videoDisplay._mediaPlayer.Rewind (false);
		videoDisplay._mediaPlayer.Play ();

		// videoDisplay._mediaPlayer.Events.AddListener(OnMenuMediaPlayer);
	}


	public void DoShowSwipeScroller(GameObject swipePanel )
	{
		
		swipePanel.gameObject.SetActive (true);
	}

	public void TurnOffMenuElements()
	{
		foreach (GameObject go in mainMenuDisplayObjects) {
			go.SetActive (false);
		}

	}
}
