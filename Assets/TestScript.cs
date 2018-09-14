using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using RenderHeads.Media.AVProVideo;

public class TestScript : MonoBehaviour {

	public MediaPlayer  mediaPlayer;
	private AudioSource myAudioSource;
	private bool isMediaPlaying;


	void start () 
	{
		
		Debug.Log("Made it");

	}


	void update () 
	{

		if (Input.GetKeyDown (KeyCode.Space)) 
		{
			if (isMediaPlaying) {
				mediaPlayer.Pause();
				isMediaPlaying = false;
			}
			if (!isMediaPlaying) {
				mediaPlayer.Play();
				isMediaPlaying = true;
			}
		}
	}

	// Callback function to handle events
	public void OnVideoEvent(MediaPlayer mp, MediaPlayerEvent.EventType et,
		ErrorCode errorCode)
	{
		switch (et) {
		case MediaPlayerEvent.EventType.ReadyToPlay:
			mp.Control.Play();
			break;
		case MediaPlayerEvent.EventType.FirstFrameReady:
			Debug.Log("First frame ready");
			break;
		case MediaPlayerEvent.EventType.FinishedPlaying:
			mp.Control.Rewind();
			break;
		}
		Debug.Log("Event: " + et.ToString());
	}


}

