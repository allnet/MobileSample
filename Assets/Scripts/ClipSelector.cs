using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using TMPro;

public class ClipSelector : MonoBehaviour
{
    public GameObject baseVideo;  // first video of series use name to get index

    public TMP_Dropdown selector;
    public VideoPlayer videoPlayer;
    public List<VideoClip> videoClips;

    public int currentClipIndex = 0;
    private GameObject currentHotSpotScreen;
    //public Dropdown selector;  // only thing that ties it to the dropdown

    void Start()
    {

        if (videoPlayer == null)
        {
            Debug.LogError("You can't attach this component without Video Player!");
            //DestroyImmediate(this);
            return;
        }

        if (selector != null)
        {
            //Add listener for when the value of the Dropdown changes, to take action
            selector.onValueChanged.AddListener(delegate
            {
                OnSelectionChanged(selector);
            });

            SwitchTo(currentClipIndex);
        }

        //    else
        //{
        //    Debug.LogError("You can't attach this component without a selector!");
        //    //DestroyImmediate(this);
        //    return;
        //}
    }

    public void OnOpenVideoFileAtIndex(int index)
    {
        SwitchTo(index);
    }

    public void OnSelectionChanged(TMP_Dropdown dropdownSelection) //unity event
    {
        int selection = dropdownSelection.value;
        Debug.Log("selection = " + selection.ToString());  // 
                                                           // Debug.Log("Made it");
        SwitchTo(selection);
    }

    public void SwitchTo(GameObject videoScreen)
    {

        if (currentHotSpotScreen != null)
        {
            videoPlayer.Stop();
            videoPlayer.clip = null;
            currentHotSpotScreen.SetActive(false);
        }

        currentHotSpotScreen = videoScreen;

        SwitchIfValidIndex(videoScreen.name);
    }

    private void SwitchIfValidIndex(string videoName)
    {

        string lastNumber = videoName.Substring(baseVideo.name.Length);

        Debug.Log("Index = " + lastNumber);

        int index;
        bool isSuccess = int.TryParse(lastNumber, out index);
        if (isSuccess)
        {
            SwitchTo(index);
        }
    }

    public void SwitchTo(int index)
    {
        //guards  
        if (videoPlayer == null) return;
        if (index >= videoClips.Count) return;

        if (currentHotSpotScreen != null) currentHotSpotScreen.SetActive(true);

        videoPlayer.clip = videoClips[index];
        videoPlayer.Play();

        Debug.Log("Clip name = " + videoPlayer.clip.name);
    }

    public void SwitchTo(string videoName)
    {


    }

}
