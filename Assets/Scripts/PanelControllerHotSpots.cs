using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RenderHeads.Media.AVProVideo;
using RenderHeads.Media.AVProVideo.Demos;  //FIX-ME - gets me access to class defiinitions

public class PanelControllerHotSpots : MonoBehaviour {

	public Text textInvite;

    public ClipSelector vcr;
	public MediaPlayer mediaPlayer;
	public DisplayIMGUI[] hotSpotVideos;
	public GameObject[] closeButtons;
	public GameObject[] openButtons;

	private int previousIndex;
	[HideInInspector] public bool isHotSpotDisplayed;


    // ======================================================

	public void Start()
	{
		InvokeRepeating ("AnimateIcons", 3.0f, 3.0f);

	}

	public void AnimateIcons() 
	{
		if (isHotSpotDisplayed) return;  // skip the cycle if something is active
		
		for (int i = 0;  i < openButtons.Length; i++)
		{
			GameObject openButton = openButtons[i] as GameObject;
			Animator animator = openButton.GetComponent <Animator> ();
			animator.SetBool("Highlighted", true);

		}

		StartCoroutine(ResetIcons(1.0f));
	}

	IEnumerator ResetIcons(float time)
	{
		yield return new WaitForSeconds(time);

		for (int i = 0;  i < openButtons.Length; i++)
		{
			GameObject openButton = openButtons[i] as GameObject;
			Animator animator = openButton.GetComponent <Animator> ();
			animator.SetBool("Normal", true);
		}

	}

	//  end of auto animation 


	public void TurnOffAllVideosAndCloseButtons() 
	{

		foreach (DisplayIMGUI videoDisplay in hotSpotVideos)
		{
			videoDisplay.gameObject.SetActive (false);
		}


		foreach (GameObject closeButton in closeButtons)
		{
			closeButton.SetActive (false);
		}

		textInvite.gameObject.SetActive (false);

	}

	public void TurnOnButtonsExceptRequested(int index) 
	{
		for (int i = 0;  i < openButtons.Length; i++)
		{
			GameObject openButton = openButtons[i] as GameObject;

			if (i != index) { openButton.SetActive (true); } // otherwise handled in code
		}

	}

	public void SetHotSpotButtonsToActive(bool aBool)
	{

		for (int i = 0;  i < openButtons.Length; i++)
			{
				GameObject openButton = openButtons[i] as GameObject;
			  	openButton.SetActive (aBool); 
			}

		//textInvite.gameObject.SetActive (false);
		SetDimState (false);
	}


	public void OnHotSpotPressed(int index)

	{
		TurnOffAllVideosAndCloseButtons ();

		TurnOnButtonsExceptRequested (index);

		vcr.OnOpenVideoFileAtIndex (index);  // increments and plays using _videoIndex

		DisplayIMGUI videoDisplay = hotSpotVideos [index] as DisplayIMGUI;	
		videoDisplay._mediaPlayer.Control.SetLooping (false);

		videoDisplay.gameObject.SetActive (true);
		Debug.Log ("On start hotspot pressed = " + index);

		isHotSpotDisplayed = true;
	}


	public void OnHotSpotShoesPressed(int index)  // different approach
	{
//		for (int i = 0;  i < openButtons.Length; i++)
//		{
//			GameObject openButton = openButtons[i] as GameObject;
//			Animator animator = openButton.GetComponent <Animator> ();
//			animator.SetBool("Normal", true);
//		}

		//OnCloseButtonHit (previousIndex);

		TurnOffAllVideosAndCloseButtons ();

		SetDimState (true);	

			vcr.OnOpenVideoFileAtIndex (index);  // increments and plays using _videoIndex	
			DisplayIMGUI videoDisplay = hotSpotVideos [index] as DisplayIMGUI;
				//videoDisplay._mediaPlayer.Control.SetLooping (false);
			videoDisplay.gameObject.SetActive (true);	
			videoDisplay._mediaPlayer.Play ();
			Debug.Log ("On start hotspot pressed = " + index);

		isHotSpotDisplayed = true;
	}
		

	public void SetDimState(bool aBool) 
	{
		for (int i = 0;  i < openButtons.Length; i++)
		{
			GameObject openButton = openButtons[i] as GameObject;
			Animator animator = openButton.GetComponent <Animator> ();
			animator.SetBool("Disabled", aBool);

			if (!aBool) animator.SetBool("Normal", true);  // restore normal

			//if (i != index) { openButton.SetActive (true); } // otherwise handled in code
		}
	}

	public void OnCloseButtonHit (int index)
	{

		for (int i = 0;  i < openButtons.Length; i++)
		{
			GameObject openButton = openButtons[i] as GameObject;

			if (i != index) { openButton.SetActive (true); } // otherwise handled in code
			openButton.SetActive (true);

		}

		SetDimState (false);
		isHotSpotDisplayed = false;
	}


	//	public void TurnOnHotSpotAtIndex(int index)
	//	{
	//
	//		TurnOffAllVideosAndCloseButtons ();
	//
	//		TurnOnButtonsExceptRequested (index);
	//
	//		DisplayIMGUI videoDisplay = hotSpotVideos [index];
	//
	//		videoDisplay._mediaPlayer.Rewind(false);
	//		videoDisplay._mediaPlayer.Play ();
	//
	//		closeButtons[index].SetActive (true);
	//
	//		videoDisplay._mediaPlayer.m_AutoStart = true;  // eliminate flash
	//		videoDisplay.gameObject.SetActive (true);
	//	}

}
