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
        public float stateDuration;
        public int layerIndex;  // int for layer in FSM

        public AnimatorStateInfoHelper(AnimatorStateInfo animatorStateInfo, int layerIndex, string[] stateNames = null)  // constru
        {
            this.layerIndex = layerIndex;  // only 1 at present
            ParseStateInfo(animatorStateInfo, stateNames);
        }

        public void ParseStateInfo(AnimatorStateInfo animatorStateInfo, string[] stateNames = null )  // enum based
        {
            if (stateNames == null) return;  // return from uncharted waters         
            
            int i = -1;
            foreach (string val in stateNames)
            {
                i++;
                //Debug.Log("search val = " + searchVal);
                if (animatorStateInfo.IsName(val))  // only way att
                {
                    this.stateName = val;
                    break;
                }
            }
            this.stateDuration = animatorStateInfo.length;
        }

        //public void ParseStateInfo(AnimatorStateInfo animatorStateInfo)  // enum based
        //{
        //    duration = animatorStateInfo.length;

        //    foreach (eAppState enumVal in Enum.GetValues(typeof(eAppState)))
        //    {
        //        stateName = (enumVal.ToString() == EnumEndString) ? "< State Mismatch >" : enumVal.ToString();

        //        //Debug.Log("search val = " + searchVal);
        //        if (animatorStateInfo.IsName(stateName))  // only way att
        //        {
        //            stateIndex = (int)enumVal;
        //            break;
        //        }
        //    }
        //}

    }
}