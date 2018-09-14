using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;


public class AppManager : Singleton <AppManager> // subclass to get singleton
{
	protected AppManager () {} // guarantee this will be always a singleton only - can't use the constructor!

	public static string nameOfMainCanvas = "MainCanvas";
	public static string nameOfBuilderPanel = "xToMainPanel";
	public static string nameOfHotSpotsPanel = "HotSpotsPanel";
	public static string nameOfMainMenuPanel = "MainMenuBtnsPanel";

	public Animator initiallyOpen;

	//[HideInInspector] public bool isPlayingAttractLoop = true;
	public float restartWaitTime = 30.0f;
	[HideInInspector] public float waitTimestamp = 30.0f;

	public int countdownSeconds = 10;
	[HideInInspector] public int  countdownStartSeconds;
	public Text countdownText;
	int countdownSecs;
	private bool isShowingCountdown = false; 
	private bool isPlayingVideo = false;
	public Button countdownButton;

	public int currentPanelIndex = 0;  // corresponds to the panel
	public Animator[] animators;
	public GameObject[] panels;
	[HideInInspector] public PanelControllerAttract panelControllerAttract;
	[HideInInspector] public MainController mainController;

	private int m_OpenParameterId;
	private Animator m_Open;
	private GameObject m_PreviouslySelected;

	const string k_OpenTransitionName = "Open";
	const string k_ClosedStateName = "Closed";

	[HideInInspector] public enum UIState
		{
			UIStateAttract,
			UIStateMain,
			UIStateWatchVideo,
			UIStateHowItWorks,
			UIStateMoreProducts
		}
	[HideInInspector] public UIState currentState;

	void Start ()
	{
		//InvokeRepeating ("TestForKeyPress", 0.0f, 1.0f);
		DontDestroyOnLoad(this.gameObject);  // make persist

		panelControllerAttract = panels [0].gameObject.GetComponent <PanelControllerAttract> ();
		mainController = panels [1].gameObject.GetComponent <MainController> ();

		countdownButton.gameObject.SetActive (false);
		countdownStartSeconds = countdownSeconds;
	}

	public void TestForKeyPress()
	{
		bool wasTouched = (Input.touchCount > 0 || Input.GetMouseButtonDown (0));  // short cuts \/
		bool isPlayingAttractLoop = (currentState == UIState.UIStateAttract); 

		bool isNotPlayingHotspot = (	currentState == UIState.UIStateHowItWorks || 
								currentState == UIState.UIStateWatchVideo ||
								currentState == UIState.UIStateMoreProducts  );


		if (currentState == UIState.UIStateAttract || wasTouched || isNotPlayingHotspot) {
			waitTimestamp = Time.time + restartWaitTime;  // reset target
		}

		if (wasTouched || isPlayingAttractLoop || isNotPlayingHotspot) { StopCountdown (); }

		if (!wasTouched && !isPlayingAttractLoop  && !isNotPlayingHotspot)
		{ 
			// no touch  //print ("Time.time" + elapsedTime + "  waitTimestamp" + waitTimestamp);
			if (Time.time >= waitTimestamp) { RestartExperience ();	} 

			CountdownHandler ();		
		} 

	}

	public void CountdownHandler ()
	{
		// countdown handling
		//	 print ("time to restart = " + (waitTimestamp - Time.time).ToString () );	
		countdownSeconds = Mathf.FloorToInt (waitTimestamp - Time.time);
		bool isTimeForCountdown = (countdownSeconds <= countdownStartSeconds && countdownSeconds != 0);
		if (isTimeForCountdown) 
		{
			if (isShowingCountdown)	UpdateCountdown ();  else  StartCountdown ();				
		} 
	}


	public void StartCountdown () 
	{					
		countdownSeconds = countdownStartSeconds; // restore orignal setting
		countdownButton.gameObject.SetActive (true);

		isShowingCountdown = true;
		UpdateCountdown();
 	}

	private void UpdateCountdown () 
	{
		if (countdownSeconds <= 0) { StopCountdown (); }

		if (isShowingCountdown) countdownText.text = "" + countdownSeconds.ToString();
	}

	public void StopCountdown () 
	{
		isShowingCountdown = false;	
		countdownButton.gameObject.SetActive (false);
	}
		


	public void RestartExperience () 
	{
		StopCountdown ();
	
		mainController.TurnAllSelectionsOff ();
 
		panels [1].gameObject.SetActive (false); //TODO:   iterate through the rest fo the panels if more than 2

		currentPanelIndex = 0;	
		//panels [0].gameObject.SetActive (false); 
		panels [0].gameObject.SetActive (true);

		panelControllerAttract.isInitialized = false;
		panelControllerAttract.vcr.OnOpenVideoFileAtIndex (0);  //DH
		panelControllerAttract.videoDisplay.gameObject.SetActive (true);

		currentState = UIState.UIStateAttract;
		waitTimestamp = Time.time + restartWaitTime;	  // touch occured

	}

	public void OnEnable()
	{
		m_OpenParameterId = Animator.StringToHash (k_OpenTransitionName);

		if (initiallyOpen == null)
			return;

		OpenPanel(initiallyOpen);
	}

public void BasicOpenNextPanel () 
	{
		//currentPanelIndex = 0;  //TODO:  need a better way to do this
		print ("currentPanelIndex = " + currentPanelIndex);	
		//Animator animator = gameObject.GetComponent<Animator> ();
		GameObject gameObject = panels[currentPanelIndex];
		// RUN close animation
		gameObject.SetActive (false);

		currentPanelIndex++;
		GameObject panel = panels[currentPanelIndex];
		// RUN open animation
		panel.SetActive (true);
	}

	public void OpenNextPanel () {

		BasicOpenNextPanel ();

		OpenPanel (animators [currentPanelIndex]);

		Debug.Log ("OpenNextPanel - " + currentPanelIndex);
	}

	public void OpenPreviousPanel () {

		if (currentPanelIndex == 0)	return;

		//Animator animator = gameObject.GetComponent<Animator> ();
		GameObject gameObject = panels[currentPanelIndex];

		gameObject.SetActive (false);  // RUN close animation
		 
		currentPanelIndex --;
		GameObject panel = panels[currentPanelIndex];

		panel.SetActive (true);  		// RUN open animation

		OpenPanel (animators [currentPanelIndex]);

		Debug.Log ("OpenNextPanel - " + currentPanelIndex);
	}

	public void OpenStartPanel () 
	{
		
		Debug.Log ("OpeStartPanel - " + currentPanelIndex);

		if (currentPanelIndex == 0)	return;

		//Animator animator = gameObject.GetComponent<Animator> ();
		GameObject gameObject = panels[currentPanelIndex];
		// RUN close animation
		gameObject.SetActive (false);

		currentPanelIndex = 0;
		GameObject panel = panels[currentPanelIndex];
		// RUN open animation
		panel.SetActive (true);

		//OpenPanel (animators [currentPanelIndex]);

	}


	public void OpenPanel (Animator anim)
	{
		if (m_Open == anim)
			return;

		anim.gameObject.SetActive(true);
		var newPreviouslySelected = EventSystem.current.currentSelectedGameObject;

		anim.transform.SetAsLastSibling();

		CloseCurrent();

		m_PreviouslySelected = newPreviouslySelected;

		m_Open = anim;
		m_Open.SetBool(m_OpenParameterId, true);

		GameObject go = FindFirstEnabledSelectable(anim.gameObject);

		SetSelected(go);
	}

	static GameObject FindFirstEnabledSelectable (GameObject gameObject)
	{
		GameObject go = null;
		var selectables = gameObject.GetComponentsInChildren<Selectable> (true);
		foreach (var selectable in selectables) {
			if (selectable.IsActive () && selectable.IsInteractable ()) {
				go = selectable.gameObject;
				break;
			}
		}
		return go;
	}

	public void CloseCurrent()
	{
		if (m_Open == null)
			return;

		m_Open.SetBool(m_OpenParameterId, false);
		SetSelected(m_PreviouslySelected);
		StartCoroutine(DisablePanelDeleyed(m_Open));
		m_Open = null;
	}

	IEnumerator DisablePanelDeleyed(Animator anim)
	{
		bool closedStateReached = false;
		bool wantToClose = true;
		while (!closedStateReached && wantToClose)
		{
			if (!anim.IsInTransition(0))
				closedStateReached = anim.GetCurrentAnimatorStateInfo(0).IsName(k_ClosedStateName);

			wantToClose = !anim.GetBool(m_OpenParameterId);

			yield return new WaitForEndOfFrame();
		}

		if (wantToClose)
			anim.gameObject.SetActive(false);
	}

	private void SetSelected(GameObject go)
	{
		EventSystem.current.SetSelectedGameObject(go);
	}
		

}