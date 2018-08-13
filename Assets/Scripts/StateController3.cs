using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AllNetXR
{
    public class StateController3 : StateController  // controls its state once it is awake
    {
        public new void Begin()  // or override member hiding 
        {
            base.Begin();

        }

        public new void End()
        {
            base.End();

        }

    }
}