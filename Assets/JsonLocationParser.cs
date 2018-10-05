using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;



namespace EVgo
{

    public class JsonLocationParser : MonoBehaviour
    {
        //public enum CharacterType
        //{
        //    oge = 10,
        //    human = 20,
        //    elf = 40
        //}

        public struct MapCoordinate
        {

            float longitude;
            float latitude;
        }

        public struct Station
        {


        }

        public class LocationInfo
        {
            public string externalId;
            public string name { get; set; }
            public string fullAddress { get; set; }
            public MapCoordinate mapCoordinate { get; set; } // x, y
            public bool hideOnMap;
         
            public string stations;  // needs to be parsed 

            //public int age { get; set; }  // doesnt exist yet
            //public Stations[] stations;
            //public CharacterType characterType { get; set; }
        }


        void Awake()
        {

            //APIRequester requester = GetComponent<Ap>
            APIRequester.OnLocationDataLoad += APIRequester_OnLocationDataLoad;
        }

        void APIRequester_OnLocationDataLoad(string aJsonString)
        {
            LocationInfo location = JsonConvert.DeserializeObject<LocationInfo>(aJsonString);     // convert into object  
            location.name = "Darryl-changed-me";

            string modJsonString = JsonConvert.SerializeObject(location, Formatting.None);
            JsonConvert.PopulateObject(modJsonString, location);

            Debug.Log("Original  = " + aJsonString);
            Debug.Log("Changed = " + modJsonString);
        }

    }
}