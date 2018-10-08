using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace EVgo
{
    public class JsonLocationsSchema
    {
        //public enum CharacterType
        //{
        //    oge = 10,
        //    human = 20,
        //    elf = 40
        //}

        public List<LocationInfo> rows;

        public class LocationInfo
        {
            public string name { get; set; }
            public string fullAddress { get; set; }
            public Point point { get; set; } // x, y
            public string externalId { get; set; }
            public List<Station> stations { get; set; }// needs to be parsed 
            public bool hideOnMap { get; set; }
        }

        public class Point
        {
            float x;
            float y;
        }

        public class Station
        {
            public string externalId { get; set; }
            public string name { get; set; }
            public List<StationPort> stationPorts { get; set; }
        }

        public class StationPort
        {
            public string stationPortStatus { get; set; }
            public string powerLevel { get; set; }
            public string qrCode { get; set; }
            public string handicapAccessible { get; set; }
            public string pricingPolicyExternalId { get; set; }
            public string free { get; set; }
        }

    }

}