using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof(AudioSource))]

public class RawImageMoviePlayer : MonoBehaviour {


	public MovieTexture movie;
	private AudioSource audio;


	// Use this for initialization
	void Start () {
		Debug.Log ("testing");

		GetComponent<RawImage> ().texture = movie as MovieTexture;
		audio = GetComponent<AudioSource> ();
		audio.clip = movie.audioClip;
		movie.Play ();
		audio.Play ();
 	}
	
	// Update is called once per frame
	void Update ()
	{
// 		if (Input.GetKeyDown (KeyCode.Space)) 
//		{
//			if (movie.isPlaying) {
//				movie.Pause();
//				audio.Pause();
//			}
//			if (!movie.isPlaying) {
//				movie.Play();
//				audio.Play ();
//			}
//		}


		if (Input.GetMouseButtonDown (0)) 
		{
			if (movie.isPlaying) {
				movie.Pause();
				audio.Pause();
			}
			if (!movie.isPlaying) {
				movie.Play();
				audio.Play ();
			}

		}


	}
}
