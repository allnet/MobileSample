using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AllNetXR
{
    public class NewUserProfile : AppUserProfile
    {
        public new eAppState startState = eAppState.State0;
        public new eUserType userType = eUserType.New;



        public override void Initialize(GameObject obj)
        {
            throw new System.NotImplementedException();
        }

        public override void IsValidAction()
        {
            throw new System.NotImplementedException();
        }

        public override void ReadyAction()
        {
            throw new System.NotImplementedException();
        }
    }
}