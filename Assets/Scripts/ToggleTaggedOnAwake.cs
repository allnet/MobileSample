using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eTagLabel
{
    HotSpotVideo, MainMenuVideo
}

public class ToggleTaggedOnAwake : MonoBehaviour
{
    public bool isOn = false;
    public string tagToFind;
    //public eTagLabel tagValue; 
    [SerializeField] private Transform[] taggedTransforms;


    void Start()
    {
        Reset();
    }

    private void Reset()
    {
        taggedTransforms = this.transform.FindChildrenByTag(tagToFind).ToArray();
        //find all implementers of hotspotvideoplayer
        foreach (Transform t in taggedTransforms)
        {
            t.gameObject.SetActive(isOn);
        }
    }


}
