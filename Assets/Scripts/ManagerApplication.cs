using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ManagerApplication : MonoBehaviour {

	//  Singleton
	public static ManagerApplication Instance;

	//public ManagerData dataManager;
	public ManagerInput managerInput;
	public ManagerScene managerScene;
	bool isInitialized = false;
	public string videoFileName;


	void Awake()
	{
		// Singleton
		if (Instance != null && Instance != this)
		{
			Destroy(this.gameObject);
		} else { 
			Instance = this;
			DontDestroyOnLoad(this.gameObject);       
		}
	}

	 void Start () {


//		//base.init ();
    	//DontDestroyOnLoad (this.gameObject);
//
//
//		if (!isInitialized) {
//			SceneManager.LoadScene (1);
//			isInitialized = true;
//		}

		SceneManager.LoadScene ("_Start");
	}


	public void Quit () 
	{
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#else
		Application.Quit();
		#endif
	}

//	void OnLevelWasLoaded(int level) {
//
//	}
}
