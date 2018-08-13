using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AllNetXR
{
    public enum eUIStateAdditive
    {
        None = -1,
        DirectionIndicator = 0,
        PlayerMarker = 1,
        ErrorMessage = 2,
        Count
    }

    public class UIManagerAdditive : MonoBehaviour
    {
      /*
       *public static UIManagerAdditive Instance;
        public static string RequestParmSuffix;
        public static string ActiveParmSuffix;
        public bool DebugMode = true;
        public List<UIViewControllerAdditive> UIAdditions;
        public List<bool> IsAdditiveUIShowing;

        private Animator UIStateMachine;
        private eUIStateAdditive LatestAdditiveState;
        private UIViewControllerAdditive LatestAdditiveVC;
        private int LatestAdditiveVCIdx;

        // Use this for initialization
        private void Awake()
        {
            Instance = this;
            RequestParmSuffix = "Request";
            ActiveParmSuffix = "Active";
        }

        public void Reset()
        {
            IsAdditiveUIShowing = new List<bool>();
            
            foreach (UIViewControllerAdditive UIState in UIAdditions)
            {
                if (UIState == null) continue;
                UIState.View.gameObject.SetActive(false);
                UIState.gameObject.SetActive(false);
                IsAdditiveUIShowing.Add(false);              
            }
            
        }

        public void Start()
        {
            Reset();
            UIStateMachine = GameManager.Instance.UISystemManager.UIStateMachine;
         }

    #region mecanim interface requirements - IAnimatableUI

    public bool ShowViewForState(eUIStateAdditive aState)
        {
            if (aState == eUIStateAdditive.None)
            {
                HandleNoneRequest();
                return false;
            }

            if (IsAdditiveUIShowing[(int)aState])
            {
                return false;
            }

            UIViewControllerAdditive vc = (UIViewControllerAdditive)UIAdditions[(int)aState];

            if (!vc.ShouldShow())
            {
                return false;
            }

            UIStateMachine.SetBool(aState.ToString() + "Request", true);

            LatestAdditiveVC = vc;
            LatestAdditiveVC.gameObject.SetActive(true);
            LatestAdditiveVC.View.gameObject.SetActive(true);

            IsAdditiveUIShowing[(int)aState] = true;
            return true;
        }

        public bool HideViewForState(eUIStateAdditive aState)
        {
            if (aState == eUIStateAdditive.None)
            {
                return false;
            }

            LatestAdditiveVC = (UIViewControllerAdditive)UIAdditions[(int)aState];
            if (LatestAdditiveVC.CanDismiss) // note: this can also be IsValid
            {
                UIStateMachine.SetBool(aState.ToString() + RequestParmSuffix, false);  // trigger close if 
                return true;
            }

            return false;
        }

        public void OnClipOpenComplete(eUIStateAdditive aState)
        {
            UIStateMachine.SetBool(aState + ActiveParmSuffix, true);  // triggers wait state            
        }

        public void OnClipOpenStarted(eUIStateAdditive aState)
        {

        }

        public void OnClipCloseComplete(eUIStateAdditive aState)  // not allowed to hide
        {
            LatestAdditiveVC = (UIViewControllerAdditive)UIAdditions[(int)aState];
            LatestAdditiveVC.gameObject.SetActive(false);
            LatestAdditiveVC.View.gameObject.SetActive(false);

            IsAdditiveUIShowing[(int)aState] = false;

            UIStateMachine.SetBool(aState.ToString() + ActiveParmSuffix, false);

            return;
        }
        
        private void HandleNoneRequest()
        {
            foreach (eUIStateAdditive aState in System.Enum.GetValues(typeof(eUIStateAdditive)))
            {
                if (DebugMode) Debug.Log(aState.ToString());
                bool result = HideViewForState(aState);
            }
        }

        #endregion
        */
    }
}
