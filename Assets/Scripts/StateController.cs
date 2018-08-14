using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AllNetXR
{
    public abstract class StateController : MonoBehaviour  // controls its state once it is awake
    {
        //public IUIAnimatableSingle viewController;

        // abstract super class for view controller with default implementations
        // open
        // close 
        // ready

        public bool isReady;
        public GameObject view;

        // == no standard game loop methods
        private void Awake()
        {
            isReady = false;
        }

        private void OnEnable()
        {
            
        }

        private void OnDisable()
        {
           
        }

        public virtual void Begin()
        {
            //if (isReady) return;  // already turned on 

            Debug.Log("BEGIN --");
            gameObject.SetActive(true);
            isReady = false;
            if (view != null) view.SetActive(true);  //s/b opening  if animatable                  
        }

        public virtual void End()
        {
            Debug.Log("END --");
            gameObject.SetActive(false);
            isReady = false;
            if (view != null) view.SetActive(false);  //s/b closing animation if animatable
        }

        public virtual void DoReady()
        {
            // do whatever the controller was intended to do
        }

        // NEED CALLBACK WHEN OBJECT IS READY FOR ACTION
        public virtual void OpenCompleteHandler()  // callback from dotween
        {
            Debug.Log("OpenCompleteHandler");
            isReady = true;
            DoReady();  //wait for input, ask questions, show video, some
        }

    }
}