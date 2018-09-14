using UnityEngine;
using System.Collections;
using UnityEngine.UI;

//[RequireComponent (typeof(AudioSource))]

public class DHPlayVideo : MonoBehaviour {


	public MovieTexture movie;
	private AudioSource audio;


	void start () 
	{
		//		// this line of code will make the Movie Texture begin playing
		//		((MovieTexture)GetComponent<Renderer>().material.mainTexture).Play();
		Debug.Log("Made it");

		GetComponent<RawImage> ().texture = movie as MovieTexture;
		audio = GetComponent<AudioSource> ();
		audio.clip = movie.audioClip;
		movie.Play ();
		audio.Play ();
	}


	void update () 
	{

		if (Input.GetKeyDown (KeyCode.Space)) 
		{
			if (movie.isPlaying) {
				movie.Pause();
			}
			if (!movie.isPlaying) {
				movie.Play();
			}
		}
	}




}
