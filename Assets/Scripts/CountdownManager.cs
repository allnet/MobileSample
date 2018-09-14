using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CountdownManager : MonoBehaviour {


	public int countdownSeconds = 10;
	[HideInInspector] public int  countdownStartSeconds;
	//public Button countdownButton;
	public Text countdownText;
	public bool isShowingCountdown = false;

	// Use this for initialization
	void Start () {
		countdownStartSeconds = countdownSeconds;
	}

	public void StartCountdown () 
	{					
		countdownSeconds = countdownStartSeconds; // restore orignal setting
		//countdownButton.gameObject.SetActive (true);

		isShowingCountdown = true;
		//InvokeRepeating ("Countdown", 0.0f, 1.0f);
		UpdateCountdown();

		print ("Start"); 
	}

	public void UpdateCountdown () 
	{
		if (countdownSeconds <= 0) {
			StopCountdown ();
		}

		if (isShowingCountdown) countdownText.text = "" + countdownSeconds.ToString();
	}

	public void StopCountdown () 
	{
		isShowingCountdown = false;
		//CancelInvoke ("Countdown");

		//countdownButton.gameObject.SetActive (false);
	}
}
