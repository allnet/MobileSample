using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AllNetXR
{
    public interface IUIAnimatableSingle  // single-state
    {
        void ChangeStateTo(eAppState aState);
        void GoToPreviousState();
        void GoToNextState();

        void OnClipOpenStarted(eAppState aState);   // open clip/frame 1 - animation event
        void OnClipWaitStarted(eAppState aState);  // 
        void OnClipCloseComplete(eAppState aState); // after close request this is fired >> UIManagerAdditive.Instance.ShowView(eUIStaeAdditive)
    }

    public interface IUIAnimatableMulti  // multi-state
    {
        bool HideViewForState(eNotificationType aState);  // returns success = true, fail = false
        bool ShowViewForState(eNotificationType aState);  //  returns success = true, fail = false

        void OnClipOpenStarted(eNotificationType aState);   // open clip/frame 1 - animation event
        void OnClipOpenComplete(eNotificationType aState);  // end of open clip - UI ready
        void OnClipCloseComplete(eNotificationType aState); // after close request this is fired >> UIManagerAdditive.Instance.ShowView(eUIStaeAdditive)
    }
}

