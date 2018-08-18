using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DoozyUI;

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
        public bool shouldControlUI = true;  // allows for parameter navigation
        //public GameObject view;
        public UIElement uiElement;

        // == no standard game loop methods
        private void Awake()
        {
            isReady = false;
        }

        private void OnEnableState()
        {

        }

        private void OnDisableState()
        {

        }

        public virtual void Begin()
        {
            //if (isReady) return;  // already turned on 

            Debug.Log("BEGIN --");
            this.gameObject.SetActive(true);
            isReady = false;
            //if (view != null) view.SetActive(true);  //s/b opening  if animatable 

            if (shouldControlUI) ShowElement();   // 
        }

        private void ShowElement()
        {
            if (uiElement != null && uiElement.GetType() == typeof(UIElement))
            {
                uiElement.Show(true); //instant action
            }
        }

        public virtual void End()
        {
            Debug.Log("END --");
            this.gameObject.SetActive(false);
            isReady = false;
            //if (view != null) view.SetActive(false);  //s/b closing animation if animatable 

            if (shouldControlUI) HideElement(); //DH
        }

        private void HideElement()
        {
            if (uiElement != null && uiElement.GetType() == typeof(UIElement))
            {
                uiElement.Hide(true, false); //instant action
            }
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