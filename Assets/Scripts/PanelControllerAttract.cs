using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RenderHeads.Media.AVProVideo;
using RenderHeads.Media.AVProVideo.Demos;  //FIX-ME - gets me access to class defiinitions


public class PanelControllerAttract : MonoBehaviour {

	public ClipSelector vcr;
	public DisplayIMGUI videoDisplay;
	public MediaPlayer mediaPlayer;

	private AppManager _appManager;

	[HideInInspector] public bool isInitialized = false;


	void Start ()
	{

		print("PanelControllerAttrat > Start " + vcr);	
	
	}

	// Update is called once per frame
	void FixedUpdate () {

		if (!isInitialized) 
		{
			mediaPlayer.SetDebugGuiEnabled (false);// turns off onboard debugger
			mediaPlayer.Control.SetLooping (true); 
			videoDisplay.gameObject.SetActive (true);

			isInitialized = true;

		}
	}

	public void OnStartPressed ()  //on full screen button pressed - based on context
	{
		//vcr.OnOpenVideoFile ();  // increments and plays using _videoIndex
		vcr.OnOpenVideoFileAtIndex (1); 
		mediaPlayer.Events.AddListener(OnMediaPlayerEvent);
		mediaPlayer.Control.SetLooping (false); 
		videoDisplay.gameObject.SetActive (true);

		print ("On start press");
	}

	// Callback function to handle events
	public void OnMediaPlayerEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
	{
		switch (et)
		{
		case MediaPlayerEvent.EventType.ReadyToPlay:
			break;
		case MediaPlayerEvent.EventType.Started:
			break;
		case MediaPlayerEvent.EventType.FirstFrameReady:
			break;
		case MediaPlayerEvent.EventType.MetaDataReady:
			//GatherProperties();
			break;
		case MediaPlayerEvent.EventType.FinishedPlaying:

			Debug.Log ("Video is finished");

			GameObject canvas = GameObject.Find (AppManager.nameOfMainCanvas);
			_appManager = canvas.GetComponent<AppManager> ();

			_appManager.currentPanelIndex = 0;
			_appManager.BasicOpenNextPanel ();
			videoDisplay.gameObject.SetActive (false);
			_appManager.currentState = AppManager.UIState.UIStateMain;

			_appManager.waitTimestamp = Time.time + _appManager.restartWaitTime; // reset timeout

			break;
		}

		//AddEvent(et);
	}


}
