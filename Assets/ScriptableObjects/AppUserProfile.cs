using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AllNetXR
{

    public enum eUserType //DH
    {
        None,
        Dormant,
        New,
        Active,
        Registered,
        LoggedOut,
        Invalid,
        Deactivated,
        Count
    }

    [CreateAssetMenu (fileName = "AppUserProfiles", menuName = "AppUserProfile")]
    public abstract class AppUserProfile : ScriptableObject
    {
        public eUserType userType;
        public eAppState startState;
        public LocationInfo locationInfo;

        public abstract void Initialize(GameObject obj);  // replaces awake or start callbacks
        public abstract void ReadyAction();   // any method with no objects  
        public abstract void IsValidAction();
    }
}
