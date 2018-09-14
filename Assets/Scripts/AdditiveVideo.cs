using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class AdditiveVideo : MonoBehaviour
{

	public GameObject videoPlayerfullScreen;


	// Use this for initialization
	public void LoadOverlayVideos () 
	{	
		SceneManager.LoadScene ("VideoPlayerController2", LoadSceneMode.Additive);
	}
	

	public void activateVideoPanel () 
	{
		videoPlayerfullScreen.SetActive (true);
	}

}
