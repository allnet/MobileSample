using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using UnityEngine.UI;


namespace EVgo
{
    public class JsonLocationsParser : MonoBehaviour
    {

        public InputField txtDebugOutput;

        void Awake()
        {
            APIRequester.OnLocationDataLoad += APIRequester_OnLocationDataLoad;
        }

        void APIRequester_OnLocationDataLoad(string aJsonString)
        {
            string newJsonString = "{\"rows\":" + aJsonString + "}";
            //var locations = JsonConvert.DeserializeObject<dynamic>(newJsonString);     // convert into object  
            JsonLocationsSchema locations = JsonConvert.DeserializeObject<JsonLocationsSchema>(newJsonString);     // convert into object 
            Debug.Log(locations.rows[0].name + locations.rows[0].fullAddress);
            Debug.Log(locations.rows[0].stations[0].name); // 1st station name
            Debug.Log(locations.rows[0].stations[0].stationPorts[0].stationPortStatus);  // 1st port
            Debug.Log(locations.rows[0].stations[0].stationPorts[0].powerLevel);
            Debug.Log(locations.rows[0].stations[0].stationPorts[0].handicapAccessible);

            //location.name = "Darryl-changed-me";
            //string modJsonString = JsonConvert.SerializeObject(location, Formatting.None);
            //JsonConvert.PopulateObject(modJsonString, location);
            //Debug.Log(locations.row[0].name);
            //0Debug.Log("Original - " + location);
            //Debug.Log("Info = " + location[0].name + location[0].fullAddress);
            //Debug.Log("Changed = " + modJsonString);

            // needs try and catch 
        }

    }

    #region DebugOutput

    //private void HeyThere(string strDebugText)
    //{
    //    try
    //    {
    //        System.Diagnostics.Debug.Write(strDebugText + Environment.NewLine);
    //        //.Text = txtDebugOutput.text + strDebugText + Environment.NewLine;
    //        //txtDebugOutput.SelectionStart = txtDebugOutput.TextLength;
    //        //txtDebugOutput.ScrollToCaret();

    //    }
    //    catch (System.Exception ex)
    //    {

    //        System.Diagnostics.Debug.Write(ex.Message.ToString() + Environment.NewLine);


    //    }

    //}
    #endregion
}