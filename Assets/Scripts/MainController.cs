using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RenderHeads.Media.AVProVideo;
using RenderHeads.Media.AVProVideo.Demos;  //FIX-ME - gets me access to class defiinitions


public class MainController : MonoBehaviour {
	
	public AppManager _appManager;

	public GameObject hotSpotsPanel;
	public GameObject mainMenuPanel;

	public Text textInvite;
    public DisplayIMGUI[] hotSpotPreviewVideos;
	public MediaPlayer lastPreviewMediaPlayer;

	int menuBtnIndex;
	bool isMenuItemSelected;


	private PanelControllerHotSpots _hotSpotsController; 
	[HideInInspector] public PanelControllerHotSpots hotSpotsController
	{ 
		get { 	
				//GameObject go = GameObject.Find ("HotSpotsPanel");
				GameObject go = GameObject.Find (AppManager.nameOfHotSpotsPanel);
				_hotSpotsController = go.GetComponent<PanelControllerHotSpots> ();
				return _hotSpotsController;
			}
	}


	private PanelControllerMenu _mainMenuController;
	[HideInInspector] public PanelControllerMenu mainMenuController
	{ 
		get { 	
			//GameObject go = GameObject.Find ("HotSpotsPanel");
			GameObject go = GameObject.Find (AppManager.nameOfMainMenuPanel);
			_mainMenuController = go.GetComponent<PanelControllerMenu> ();
			return _mainMenuController;
			}
	}

	// ================================================

	void Start ()
	{		
		lastPreviewMediaPlayer.Events.AddListener(OnMediaPlayerEvent);

		GameObject canvas = GameObject.Find(AppManager.nameOfMainCanvas);
		_appManager = canvas.GetComponent<AppManager>();
		_appManager.currentState = AppManager.UIState.UIStateMain;

		// InvokeRepeating ("AnimateIcons", 3.0f, 3.0f);

	}
	// Callback function to handle events
	public void OnMediaPlayerEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
	{
		switch (et)
		{
		case MediaPlayerEvent.EventType.ReadyToPlay:
			break;
		case MediaPlayerEvent.EventType.Started:
			print ("Video started");
			break;
		case MediaPlayerEvent.EventType.FirstFrameReady:
			break;
		case MediaPlayerEvent.EventType.MetaDataReady:
			//GatherProperties();
			break;
		case MediaPlayerEvent.EventType.FinishedPlaying:

			hotSpotsPanel.SetActive (true);		
			TurnOffHotSpotPreviewVideos ();

			break;
		}

	}


	public void OnPlayMenuVideo(DisplayIMGUI videoDisplay) 
	{
		TurnAllSelectionsOff ();
		hotSpotsController.SetHotSpotButtonsToActive(false);
		hotSpotsController.textInvite.gameObject.SetActive (false);

		mainMenuController.DoPlayMenuVideo (videoDisplay);  // punt

		switch (videoDisplay.tag)
		{
		case "HowItWorks":
			_appManager.currentState = AppManager.UIState.UIStateHowItWorks;
			break;

		case "WatchVideo":
			_appManager.currentState = AppManager.UIState.UIStateWatchVideo;
			break;

		default:
			return;
		}

	}


	public void OnShowSwipeScroller(GameObject swipePanel )
	{

		TurnAllSelectionsOff ();
		hotSpotsController.SetHotSpotButtonsToActive(false);
		hotSpotsController.textInvite.gameObject.SetActive (false);

	
		mainMenuController.DoShowSwipeScroller (swipePanel);

		_appManager.currentState = AppManager.UIState.UIStateMoreProducts;
	}

	public void OnCloseButtonHit() 
	{

		TurnAllSelectionsOff ();
		hotSpotsController.SetHotSpotButtonsToActive(true);	
		hotSpotsController.textInvite.gameObject.SetActive (true);
	}


	public void TurnAllSelectionsOff()
	{
		//TurnOffMenuElements();

		mainMenuController.TurnOffMenuElements ();	
		hotSpotsController.TurnOffAllVideosAndCloseButtons();

		textInvite.gameObject.SetActive (true);

		_appManager.currentState = AppManager.UIState.UIStateMain;
	}


	public void TurnOffHotSpotPreviewVideos () 
	{

		foreach (DisplayIMGUI videoDisplay in hotSpotPreviewVideos)
		{
			videoDisplay.gameObject.SetActive (false);
		}
	}

	void FixedUpdate ()
	{
		_appManager.TestForKeyPress ();

	}


}
