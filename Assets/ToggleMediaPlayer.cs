using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using RenderHeads.Media.AVProVideo;

//[RequireComponent (typeof(AudioSource))]

public class TogggleMediaPlayer : MonoBehaviour {


	public MediaPlayer  mediaPlayer;
	private AudioSource audio;
	private bool isMediaPlaying;


	void start () 
	{
		//		// this line of code will make the Movie Texture begin playing
		//		((MovieTexture)GetComponent<Renderer>().material.mainTexture).Play();
		Debug.Log("Made it");

		//		GetComponent<RawImage> ().texture = movie as MovieTexture;
		//		audio = GetComponent<AudioSource> ();
		//		audio.clip = movie.audioClip;
		//		movie.Play ();
		//		audio.Play ();
		isMediaPlaying = false;
		mediaPlayer.Events.AddListener(OnVideoEvent);
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
