using System.Collections;
using UnityEngine;
using System.Collections;

public class FadeObjectInOut : MonoBehaviour
{

	// publically editable speed
	public float fadeDelay = 0.0f; 
	public float fadeTime = 0.5f; 
	public bool fadeInOnStart = false; 
	public bool fadeOutOnStart = false;
	public bool isHelperVisible = false;

	public Animator animator;
	public string propertyNameForAnimator = "shouldShowHelper";
	private bool logInitialFadeSequence = false; 

	private Color[] colors; 

	//private SteamVR_TrackedObject trackedObj;
	//private SteamVR_Controller.Device Controller
	//{
	//	get { return SteamVR_Controller.Input((int)trackedObj.index); }
	//}

	//void Awake()
	//{
	//	trackedObj = GetComponent<SteamVR_TrackedObject>();
	//}


	// allow automatic fading on the start of the scene
	IEnumerator Start ()
	{
		//yield return null; 
		yield return new WaitForSeconds (fadeDelay); 

		if (fadeInOnStart)
		{
			            logInitialFadeSequence = true; 
			FadeIn (); 
		}

		if (fadeOutOnStart)
		{
			FadeOut (fadeTime); 
		}
	}

	//void Update () 
	//{
		
	//	if (Input.GetKeyDown (KeyCode.Alpha0)) { FadeToggle (); 	}

	//	if (Controller.GetPressDown (SteamVR_Controller.ButtonMask.Grip))
	//		{
	//			print(gameObject.name + "grip press");
	//			FadeToggle ();
	//		}

	//}



	void FadeToggle() 
	{
		if (animator.GetBool (propertyNameForAnimator)) {FadeOut (); } 
		else {FadeIn (); }

		print ("fade toggle");
	}

	// check the alpha value of most opaque object
	float MaxAlpha()
	{

		Renderer[] renderObjects = new Renderer[20];

		float maxAlpha = 0.0f; 
		Renderer[] rendererObjects = GetComponentsInChildren<Renderer>(); 
		rendererObjects [rendererObjects.Length] = GetComponent <Renderer> (); 

		foreach (Renderer item in rendererObjects)
		{
			maxAlpha = Mathf.Max (maxAlpha, item.material.color.a); 
		}
		return maxAlpha; 
	}

	// fade sequence
	IEnumerator FadeSequence (float fadingOutTime)
	{
		// log fading direction, then precalculate fading speed as a multiplier
		bool fadingOut = (fadingOutTime < 0.0f);
		float fadingOutSpeed = 1.0f / fadingOutTime; 

		// grab all child objects
		Renderer[] rendererObjects = GetComponentsInChildren<Renderer>(); 
		if (colors == null)
		{
			//create a cache of colors if necessary
			colors = new Color[rendererObjects.Length]; 

			// store the original colours for all child objects
			for (int i = 0; i < rendererObjects.Length; i++)
			{
				colors[i] = rendererObjects[i].material.color; 
			}
		}

		// make all objects visible
		for (int i = 0; i < rendererObjects.Length; i++)
		{
			rendererObjects[i].enabled = true;
		}


		// get current max alpha
		float alphaValue = MaxAlpha();  


		// This is a special case for objects that are set to fade in on start. 
		// it will treat them as alpha 0, despite them not being so. 
		if (logInitialFadeSequence && !fadingOut)
		{
			alphaValue = 0.0f; 
			logInitialFadeSequence = false; 
		}

		// iterate to change alpha value 
		while ( (alphaValue >= 0.0f && fadingOut) || (alphaValue <= 1.0f && !fadingOut)) 
		{
			alphaValue += Time.deltaTime * fadingOutSpeed; 

			for (int i = 0; i < rendererObjects.Length; i++)
			{
				Color newColor = (colors != null ? colors[i] : rendererObjects[i].material.color);
				newColor.a = Mathf.Min ( newColor.a, alphaValue ); 
				newColor.a = Mathf.Clamp (newColor.a, 0.0f, 1.0f); 				
				rendererObjects[i].material.SetColor("_Color", newColor) ; 
			}

			yield return null; 
		}

		// turn objects off after fading out
		if (fadingOut)
		{
			for (int i = 0; i < rendererObjects.Length; i++)
			{
				rendererObjects[i].enabled = false; 
			}
		}

		Debug.Log ("fade sequence end : " + fadingOut); 

	}


	void FadeIn ()
	{
		FadeIn (fadeTime); 
	}

	void FadeOut ()
	{
		FadeOut (fadeTime); 		
	}

	void FadeIn (float newFadeTime)
	{
		StopAllCoroutines(); 
		StartCoroutine("FadeSequence", newFadeTime); 
		animator.SetBool (propertyNameForAnimator, true);
	}

	void FadeOut (float newFadeTime)
	{
		StopAllCoroutines(); 
		StartCoroutine("FadeSequence", -newFadeTime); 
		animator.SetBool (propertyNameForAnimator, false);
	}



}