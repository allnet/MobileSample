using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace AllNetXR
{
    public class AnimatorStateInfoHelper 
    {
        private static readonly string EnumEndString = "Count";

        public string stateName;
        public int stateIndex;
        public float duration;

        public AnimatorStateInfoHelper(AnimatorStateInfo animatorStateInfo)  // constru
        {
            ParseStateInfo(animatorStateInfo);
        }

        public void ParseStateInfo(AnimatorStateInfo animatorStateInfo)
        {
            duration = animatorStateInfo.length;

            foreach (eAppState enumVal in Enum.GetValues(typeof(eAppState)))
            {
                stateName = (enumVal.ToString() == EnumEndString) ? "< State Mismatch >" : enumVal.ToString();

                //Debug.Log("search val = " + searchVal);
                if (animatorStateInfo.IsName(stateName))  // only way att
                {
                    stateIndex = (int)enumVal;
                    break;
                }
            }
        }

    }
}